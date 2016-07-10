# GameCanvas-Unity (UnityGC)
GameCanvas for Unity

This project is work in progress.<br>
このプロジェクトは現在実装途中です

## 現在利用可能な機能

### グラフィック
* `read only` screenWidth
* `read only` screenHeight
* `read only` isPortrait
* `read only` deviceResolution
* frameRate
* isFullScreen
* isScreenAutoRotation
* SetResolution()
* ClearScreen()
* DrawRect()
* DrawCircle()
* DrawLine()
* DrawPoint()
* DrawImage()
* FillRect()
* FillCircle()
* SetColor()

### その他
* Cos()
* Sin()
* Atan2()
* Deg2Rad()
* Rad2Deg()

## 実装メモ

### 今後の実装予定
* 文字描画API
  * ビットマップフォント方式で
* 入力、センサーAPI
* 描画最適化
  * 差分判定でTexture2Dの更新負荷を下げる
