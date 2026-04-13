
# 新旧API対応表

### v1.2 → v2.0

v1.2 以前に存在し v2.0 で変更・廃止されたAPIの一覧と対応表です

v1.2 | v2.0 | 備考
--- | --- | ---
acceX | `AccelerationLastX` | 関数名の変更
acceY | `AccelerationLastY` | 関数名の変更
acceZ | `AccelerationLastZ` | 関数名の変更
cameraImageHeight | `CurrentCameraHeight` | 関数名の変更
cameraImageWidth | `CurrentCameraWidth` | 関数名の変更
ChangeBGMVolume | `SetSoundVolume` | 関数名と型の変更
ChangeSEVolume |  | `Obsolete`
CheckHitCircle | `CheckHitCircle` | 引数の型の変更
CheckHitImage | `CheckHitImage` | 引数の型の変更
CheckHitRect | `CheckHitRect` | 引数の型の変更
ClearDownloadCache |  | `Obsolete`
CloseWS |  | `Obsolete`
compass |  | `Obsolete`
ConvertFromJson |  | `Obsolete`
ConvertToJson |  | `Obsolete`
Deg2Rad |  | `Obsolete`
DeleteData |  | `Obsolete`
DeleteDataAll |  | `Obsolete`
deltaTime | `TimeSincePrevFrame` | 関数名の変更
DrawCameraImage | `DrawCameraImage` | 引数の型の変更
DrawCameraImageSRT |  | `Obsolete` 代替: `DrawScaledRotateCameraImage`
DrawCircle | `DrawCircle` | 引数の型の変更
DrawClippedCameraImage | `DrawClipCameraImage` | 関数名と型の変更、仕様変更
DrawClippedImage |  | `Obsolete` 代替: `DrawClipImage`
DrawClippedImageUVWH | `DrawClipImage` | 関数名と型の変更、仕様変更
DrawClippedOnlineImage |  | `Obsolete`
DrawImage | `DrawImage` | 引数の型の変更
DrawImageSRT | `DrawScaledRotateImage` | 関数名と型の変更
DrawLine | `DrawLine` | 引数の型の変更
DrawMultiLineString |  | `Obsolete` 代替: `DrawString`
DrawOnlineImage |  | 引数の型の変更
DrawOnlineImageSRT |  | `Obsolete`
DrawRect | `DrawRect` | 引数の型の変更
DrawRotatedCameraImage |  | `Obsolete` 代替: `DrawScaledRotateCameraImage`
DrawRotatedImage |  | `Obsolete` 代替: `DrawScaledRotateImage`
DrawRotatedOnlineImage |  | `Obsolete`
DrawRotatedRect |  | *TODO*
DrawScaledCameraImage |  | `Obsolete` 代替: `DrawScaledRotateCameraImage`
DrawScaledImage |  | `Obsolete` 代替: `DrawScaledRotateImage`
DrawScaledOnlineImage |  | `Obsolete`
DrawString | `DrawString` | 引数順と型の変更、ダイナミックフォント対応
FillCircle | `FillCircle` | 引数の型の変更
FillRect | `FillRect` | 引数の型の変更
FillRotatedRect |  | *TODO*
frameRate | `ConfigFps` | 関数名の変更
GetColorOfCameraImage |  | `Obsolete`
GetColorsOfCameraImage |  | `Obsolete`
GetDay | `CurrentDay` | 関数名の変更
GetDayOfWeek | `CurrentDayOfWeek` | 関数名と型の変更
GetDayOfWeekKanji |  | `Obsolete` 代替: `CurrentDayOfWeek`
GetHour | `CurrentHour` | 関数名の変更
GetIsKeyPress | `GetIsKeyPress` | 引数の型の変更
GetIsKeyPushed | `GetIsKeyBegan` | 引数の型の変更
GetIsKeyReleased | `GetIsKeyEnded` | 引数の型の変更
GetMilliSecond | `CurrentMillisecond` | 関数名の変更
GetMinute | `CurrentMinute` | 関数名の変更
GetMonth | `CurrentMonth` | 関数名の変更
GetSecond | `CurrentSecond` | 関数名の変更
GetTextFromNet |  | `Obsolete`
GetTextFromNetAsync |  | `Obsolete` 代替: `GetOnlineTextAsync`
GetTouchPoint |  | `Obsolete` 代替: `GetPointerX` `GetPointerY`
GetYear | `CurrentYear` | 関数名の変更
gyroX |  | *TODO*
gyroY |  | *TODO*
gyroZ |  | *TODO*
isBackKeyPushed |  | `Obsolete` 代替: `IsPressBackButton`
isCompassEnabled |  | `Obsolete`
isDevelop |  | `Obsolete`
isDownloadedImage |  | `Obsolete`
isFlick |  | `Obsolete`
isFullScreen |  | `Obsolete`
isGyroEnabled |  | `Obsolete`
isHold |  | `Obsolete` 代替: `GetPointerDuration`
isLoaded |  | `Obsolete`
isLocationEnabled | `HasGeolocationPermission` | 関数名の変更
isOpenWS |  | `Obsolete`
isPinchIn |  | `Obsolete`
isPinchInOut |  | `Obsolete`
isPinchOut |  | `Obsolete`
isPortrait |  | `Obsolete`
isRunningLocaltionService | `IsGeolocationRunning` | 関数名の変更、仕様変更
isScreenAutoRotation |  | `Obsolete`
isTap |  | `Obsolete` 代替: `GetPointerDuration`
isTouch |  | `Obsolete` 代替: `HasPointerEvent` `PointerCount`
isTouchBegan |  | `Obsolete` 代替: `GetIsPointerBegan`
isTouchEnded |  | `Obsolete` 代替: `GetIsPointerEnded`
lastLocationLatitude | `GeolocationLastLatitude` | 関数名の変更
lastLocationLongitude | `GeolocationLastLongitude` | 関数名の変更
lastLocationTime | `GeolocationLastTime` | 関数名と型の変更
Load |  | *TODO*
LoadAsInt | `Load` | 関数名と型の変更
LoadAsNumber |  | *TODO*
maxPinchInScale |  | `Obsolete`
maxTapDistance |  | `Obsolete`
maxTapTimeLength |  | `Obsolete`
minFlickDistance |  | `Obsolete`
minHoldTimeLength |  | `Obsolete`
minPinchOutScale |  | `Obsolete`
OpenWS |  | `Obsolete`
PauseBGM | `PauseSound` | 関数名の変更
pinchRatio |  | `Obsolete`
pinchRatioInstant |  | `Obsolete`
PlayBGM | `PlaySound` | 関数名の変更
PlaySE |  | `Obsolete`
Rad2Deg |  | `Obsolete`
ReadDataByStorage |  | `Obsolete`
Save |  | *TODO*
SaveAsInt | `Save` | 関数名と型の変更
SaveAsNumber |  | *TODO*
screenHeight | `CanvasHeight` | 関数名の変更
screenWidth | `CanvasWidth` | 関数名の変更
SendWS |  | `Obsolete`
SetFontSize | `SetFontSize` | 引数の型の変更
SetTextHorizontalRatio |  | `Obsolete`
SetTextLineHeight |  | `Obsolete`
SetTextTracking |  | `Obsolete`
StartCameraService | `StartCameraService` | 引数の型の変更
StartLocationService | `StartGeolocationService` | 関数名の変更
StopBGM | `StopSound` | 関数名の変更
StopLocationService | `StopGeolocationService` | 関数名の変更
time | `TimeSinceStartup` | 関数名の変更
touchCount | `PointerCount` | 関数名の変更
touchPoint |  | `Obsolete`
touchTimeLength |  | `Obsolete` 代替: `GetPointerDuration`
touchX |  | `Obsolete` 代替: `GetPointerX`
touchY |  | `Obsolete` 代替: `GetPointerY`
Trace |  | *TODO*
WriteDataToStorage |  | `Obsolete`


### Java → Unity

[旧Java版](https://github.com/sfc-sdp/GameCanvas-Java/)に存在し Unity v2.0 までに変更・廃止されたAPIの一覧と対応表です  
（関数名がパスカル形式に変更されただけのものは除外しています）

Java | Unity | 備考
--- | --- | ---
atan2 | `Atan2` | 引数の型の変更
changeBGMVolume | `SetSoundVolume` | 関数名の変更
changeSEVolume |  | `Obsolete`
cos | `Cos` | 引数の型の変更
getKeyPressLength |  | `Obsolete` 代替: `GetKeyPressFrameCount`
getMouseClickLength |  | `Obsolete` 代替: `GetPointerFrameCount`
getMouseX |  | `Obsolete` 代替: `GetPointerX`
getMouseY |  | `Obsolete` 代替: `GetPointerY`
HEIGHT | `CanvasHeight` | 関数名の変更
isKeyPress | `GetIsKeyPress` | 関数名の変更
isKeyPushed | `GetIsKeyBegan` | 関数名の変更
isKeyReleased | `GetIsKeyEnded` | 関数名の変更
isMousePress |  | `Obsolete` 代替: `HasPointerEvent` `PointerCount`
isMousePushed |  | `Obsolete` 代替: `GetIsPointerBegan`
isMouseReleased |  | `Obsolete` 代替: `GetIsPointerEnded`
KEY_C |  | `Obsolete`
KEY_DOWN |  | `Obsolete`
KEY_ENTER |  | `Obsolete`
KEY_LEFT |  | `Obsolete`
KEY_RIGHT |  | `Obsolete`
KEY_SPACE |  | `Obsolete`
KEY_UP |  | `Obsolete`
KEY_V |  | `Obsolete`
KEY_X |  | `Obsolete`
KEY_Z |  | `Obsolete`
pauseBGM | `PauseSound` | 関数名の変更
pauseSE |  | `Obsolete`
playBGM | `PlaySound` | 関数名の変更
playSE | `PlaySE` | 引数の変更
rand | `Random` | 関数名の変更
resetGame |  | `Obsolete`
setFont | `SetFont` | 引数の型の変更
setWindowTitle |  | `Obsolete`
showInputDialog |  | `Obsolete`
showYesNoDialog |  | `Obsolete`
sin | `Sin` | 引数の型の変更
sqrt | `Sqrt` | 引数の型の変更
stopBGM | `StopSound` | 関数名の変更
stopSE |  | `Obsolete`
WIDTH | `CanvasWidth` | 関数名の変更
writeScreenImage | `WriteScreenImage` | 返り値の型の変更
