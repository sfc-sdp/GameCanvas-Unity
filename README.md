# GameCanvas-Unity (UnityGC)
GameCanvas for Unity

This project is work in progress.<br>
このプロジェクトは現在実装途中です

## 現在利用可能な機能

### グラフィックAPI (基本図形)
* GetImageWidth()
* GetImageHeight()
* SetColor()
* SetColorHSV()
* SetLineWidth()
* DrawLine()
* DrawCircle()
* DrawRect()
* DrawRotatedRect()
* DrawImage()
* DrawClippedImage()
* DrawScaledImage()
* DrawRotatedImage()
* DrawImageSRT()
* DrawClippedImageSRT()
* FillCircle()
* FillRect()
* FillRotatedRect()

### グラフィックAPI (その他)
* `read only` screenWidth
* `read only` screenHeight
* `read only` isPortrait
* frameRate
* isFullScreen
* isScreenAutoRotation
* SetResolution()
* ClearScreen()

### タッチ入力API
* `read only` isTouch
* `read only` isTouchBegan
* `read only` isTouchEnded
* `read only` isHold
* `read only` isTap
* `read only` isFlick
* `read only` isPinchInOut
* `read only` isPinchIn
* `read only` isPinchOut
* `read only` touchX
* `read only` touchY
* `read only` touchPoint
* `read only` touchCount
* `read only` touchTimeLength
* `read only` pinchRatio
* `read only` pinchRatioInstant
* GetTouchPoint()

### 数学API
* Cos()
* Sin()
* Atan2()
* Deg2Rad()
* Rad2Deg()

### デバッグAPI
* `read only` isDebug
* Trace()

## 実装メモ

### 今後の実装予定
* 文字描画API(BitmapFont)
  * 英数字
  * かな文字
* 入力API
  * カメラ映像
  * 加速度・ジャイロ
  * GPS・コンパス
  * キー(デバッグ用)
* 当たり判定API
* データ永続化API
* ネットワークAPI
  * テキストDL
  * 画像DL
