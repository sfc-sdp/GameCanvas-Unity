/**
 * GameCanvas WebSocket Client Example
 * 
 * Copyright (c) 2016 Seibe TAKAHASHI
 * 
 * This software is released under the MIT License.
 * http://opensource.org/licenses/mit-license.php
 */
 using GameCanvas;

public class WebSocketClientExample : GameBase
{
    float ballX;
    float ballY;
    float targetX;
    float targetY;
    string state;

    /// <summary>
    /// サーバー経由で座標を送受信するための構造体です
    /// </summary>
    struct Point
    {
        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float x;
        public float y;
    }

    override public void Start()
    {
        ballX = targetX = 320;
        ballY = targetY = 240;
        state = "Now Connecting...";

        // WebSocketサーバーに接続します
        gc.OpenWS("ws://localhost:3000/", OnOpen, OnMessage, OnClose, OnError);
    }

    override public void Calc()
    {
        // ballX を targetX に少しだけ近づけます
        ballX = ballX * 0.9f + targetX * 0.1f;

        // ballY を targetY に少しだけ近づけます
        ballY = ballY * 0.9f + targetY * 0.1f;

        // タップした時だけ処理します
        if (gc.isTap)
        {
            string message = gc.ConvertToJson(new Point(gc.touchX, gc.touchY));
            gc.SendWS(message);
        }
    }

    override public void Draw()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        // 背景を描画します
        gc.DrawImage(0, 0, 0);

        // 文字を描画します
        gc.DrawString(12, 12, state);

        // ボールを描画します
        gc.DrawImage(1, ballX - 12, ballY - 12);
    }

    void OnOpen()
    {
        state = "Open";
    }

    void OnMessage(string message)
    {
        Point point = gc.ConvertFromJson<Point>(message);

        targetX = point.x;
        targetY = point.y;
        state = message;
    }

    void OnClose()
    {
        state = "Close";
    }

    void OnError(string error)
    {
        state = "Error: " + error;
    }
}
