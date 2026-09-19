#import <CoreLocation/CoreLocation.h>
#import <Foundation/Foundation.h>
#import <TargetConditionals.h>

// gc.Location の iOS CoreLocation バックエンド。利用中の許可のみ。座標や識別子は出さない。
// マネージド側はポーリングし、ネイティブから C# へコールバックしない。
//
// Apple:
// https://developer.apple.com/documentation/corelocation/requesting-authorization-to-use-location-services
// https://developer.apple.com/documentation/corelocation/cllocationmanager/requestwheninuseauthorization()
// https://developer.apple.com/documentation/corelocation/cllocationmanagerdelegate/locationmanagerdidchangeauthorization(_:)
// https://developer.apple.com/documentation/corelocation/cllocationmanager/accuracyauthorization
// https://developer.apple.com/documentation/corelocation/cllocationmanager/locationservicesenabled()
// https://developer.apple.com/documentation/corelocation/cllocationmanager/startupdatinglocation()
// https://developer.apple.com/documentation/corelocation/cllocationmanagerdelegate/locationmanager:didupdatelocations:
// https://developer.apple.com/documentation/corelocation/cllocationmanagerdelegate/locationmanager:didfailwitherror:
// https://developer.apple.com/documentation/corelocation/cllocationsourceinformation/issimulatedbysoftware
// https://developer.apple.com/documentation/corelocation/cllocation/horizontalaccuracy
// https://developer.apple.com/documentation/corelocation/cllocation/timestamp

static void GcLocationOnMain(void (^block)(void))
{
    if (NSThread.isMainThread) block();
    else dispatch_sync(dispatch_get_main_queue(), block);
}

@interface GcLocationSession : NSObject <CLLocationManagerDelegate>
- (int)isEnabled;
- (int)authorization;
- (int)accuracy;
- (void)requestWhenInUse;
- (int)start:(int)precise;
- (void)stopUpdates;
- (int)tryReadLatitude:(double *)latitude longitude:(double *)longitude accuracy:(double *)accuracyMeters unixTime:(double *)unixTimeSeconds isMock:(int *)isMock;
- (void)close;
@end

@implementation GcLocationSession
{
    NSLock *_gate;
    CLLocationManager *_manager;
    BOOL _closed;
    BOOL _accepting;
    BOOL _hasSample;
    double _latitude;
    double _longitude;
    double _accuracyMeters;
    double _unixTimeSeconds;
    int _isMock;
    int _authorization;
    int _accuracy;
}

- (instancetype)init
{
    self = [super init];
    if (self == nil) return nil;
    _gate = [[NSLock alloc] init];
    _manager = [[CLLocationManager alloc] init];
    _manager.pausesLocationUpdatesAutomatically = YES;
    _authorization = (int)_manager.authorizationStatus;
    _accuracy = (int)_manager.accuracyAuthorization;
    _manager.delegate = self;
    return self;
}

- (int)isEnabled
{
    __block int enabled = 0;
    GcLocationOnMain(^{
        enabled = [CLLocationManager locationServicesEnabled] ? 1 : 0;
    });
    return enabled;
}

- (int)authorization
{
    [_gate lock];
    int value = _authorization;
    [_gate unlock];
    return value;
}

- (int)accuracy
{
    [_gate lock];
    int value = _accuracy;
    [_gate unlock];
    return value;
}

- (void)captureAuthorization:(CLLocationManager *)manager
{
    int authorization = (int)manager.authorizationStatus;
    int accuracy = (int)manager.accuracyAuthorization;
    [_gate lock];
    if (!_closed)
    {
        _authorization = authorization;
        _accuracy = accuracy;
    }
    [_gate unlock];
}

- (void)requestWhenInUse
{
    GcLocationOnMain(^{
        if (self->_closed || self->_manager == nil) return;
        [self captureAuthorization:self->_manager];
        if (self->_manager.authorizationStatus == kCLAuthorizationStatusNotDetermined)
            [self->_manager requestWhenInUseAuthorization];
    });
}

- (int)start:(int)precise
{
    __block int started = 0;
    GcLocationOnMain(^{
        [self->_gate lock];
        if (self->_closed || self->_manager == nil)
        {
            [self->_gate unlock];
            return;
        }
        self->_accepting = YES;
        self->_hasSample = NO;
        [self->_gate unlock];
        self->_manager.desiredAccuracy = precise ? kCLLocationAccuracyBest : kCLLocationAccuracyReduced;
        self->_manager.distanceFilter = 1.0;
        [self->_manager startUpdatingLocation];
        started = 1;
    });
    return started;
}

- (void)stopUpdates
{
    GcLocationOnMain(^{
        [self->_gate lock];
        self->_accepting = NO;
        self->_hasSample = NO;
        [self->_gate unlock];
        [self->_manager stopUpdatingLocation];
    });
}

- (int)tryReadLatitude:(double *)latitude longitude:(double *)longitude accuracy:(double *)accuracyMeters unixTime:(double *)unixTimeSeconds isMock:(int *)isMock
{
    if (latitude == NULL || longitude == NULL || accuracyMeters == NULL || unixTimeSeconds == NULL || isMock == NULL)
        return 0;
    [_gate lock];
    int ok = (!_closed && _accepting && _hasSample) ? 1 : 0;
    if (ok)
    {
        *latitude = _latitude;
        *longitude = _longitude;
        *accuracyMeters = _accuracyMeters;
        *unixTimeSeconds = _unixTimeSeconds;
        *isMock = _isMock;
    }
    [_gate unlock];
    return ok;
}

- (void)close
{
    GcLocationOnMain(^{
        [self->_gate lock];
        if (self->_closed)
        {
            [self->_gate unlock];
            return;
        }
        self->_closed = YES;
        self->_accepting = NO;
        self->_hasSample = NO;
        [self->_gate unlock];
        self->_manager.delegate = nil;
        [self->_manager stopUpdatingLocation];
        self->_manager = nil;
    });
}

- (void)locationManagerDidChangeAuthorization:(CLLocationManager *)manager
{
    [self captureAuthorization:manager];
}

- (void)locationManager:(CLLocationManager *)manager didUpdateLocations:(NSArray<CLLocation *> *)locations
{
    CLLocation *location = locations.lastObject;
    if (location == nil || location.horizontalAccuracy < 0) return;

    int mock = 0;
#if TARGET_OS_SIMULATOR
    mock = 1;
#endif
    if (location.sourceInformation.isSimulatedBySoftware) mock = 1;

    double latitude = location.coordinate.latitude;
    double longitude = location.coordinate.longitude;
    double accuracyMeters = location.horizontalAccuracy;
    double unixTimeSeconds = location.timestamp.timeIntervalSince1970;

    [_gate lock];
    if (!_closed && _accepting)
    {
        _latitude = latitude;
        _longitude = longitude;
        _accuracyMeters = accuracyMeters;
        _unixTimeSeconds = unixTimeSeconds;
        _isMock = mock;
        _hasSample = YES;
    }
    [_gate unlock];
}

- (void)locationManager:(CLLocationManager *)manager didFailWithError:(NSError *)error
{
    if (error == nil || ![error.domain isEqualToString:kCLErrorDomain]) return;
    if (error.code != kCLErrorDenied && error.code != kCLErrorPromptDeclined) return;
    [_gate lock];
    if (!_closed) _authorization = (int)kCLAuthorizationStatusDenied;
    [_gate unlock];
}

@end

static GcLocationSession *GcLocationCast(void *handle)
{
    if (handle == NULL) return nil;
    id object = (__bridge id)handle;
    return [object isKindOfClass:[GcLocationSession class]] ? (GcLocationSession *)object : nil;
}

extern "C" void *GcLocationCreate(void)
{
    __block GcLocationSession *session = nil;
    GcLocationOnMain(^{ session = [GcLocationSession new]; });
    return (__bridge_retained void *)session;
}

extern "C" void GcLocationDestroy(void *handle)
{
    if (handle == NULL) return;
    GcLocationOnMain(^{
        [GcLocationCast(handle) close];
    });
    CFRelease(handle);
}

extern "C" int GcLocationIsEnabled(void *handle)
{
    GcLocationSession *session = GcLocationCast(handle);
    return session == nil ? 0 : [session isEnabled];
}

extern "C" int GcLocationAuthorization(void *handle)
{
    GcLocationSession *session = GcLocationCast(handle);
    return session == nil ? (int)kCLAuthorizationStatusDenied : [session authorization];
}

extern "C" int GcLocationAccuracy(void *handle)
{
    GcLocationSession *session = GcLocationCast(handle);
    return session == nil ? (int)CLAccuracyAuthorizationReducedAccuracy : [session accuracy];
}

extern "C" void GcLocationRequestWhenInUse(void *handle)
{
    [GcLocationCast(handle) requestWhenInUse];
}

extern "C" int GcLocationStart(void *handle, int precise)
{
    GcLocationSession *session = GcLocationCast(handle);
    return session == nil ? 0 : [session start:precise];
}

extern "C" void GcLocationStop(void *handle)
{
    [GcLocationCast(handle) stopUpdates];
}

extern "C" int GcLocationTryRead(void *handle, double *latitude, double *longitude, double *accuracyMeters, double *unixTimeSeconds, int *isMock)
{
    GcLocationSession *session = GcLocationCast(handle);
    if (session == nil) return 0;
    return [session tryReadLatitude:latitude longitude:longitude accuracy:accuracyMeters unixTime:unixTimeSeconds isMock:isMock];
}
