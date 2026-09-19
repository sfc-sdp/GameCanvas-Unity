#import <Foundation/Foundation.h>
#import <TargetConditionals.h>

// IL2CPPのEnvironment.GetCommandLineArgsにはiOS起動引数が渡らないため、OSから読む。
extern "C" int GcProbeShouldRunSimulatorSmoke()
{
#if TARGET_OS_SIMULATOR
    return [[[NSProcessInfo processInfo] arguments] containsObject:@"--gc-probe-simulator-smoke"] ? 1 : 0;
#else
    return 0;
#endif
}
