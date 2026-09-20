#nullable enable
using GameCanvas;

public sealed class NetworkingSample : GameBase
{
    /// <summary>
    /// <see href="https://unsplash.com/photos/uztw2giebSc"/>
    /// </summary>
    const string k_ImageUrl = "https://images.unsplash.com/photo-1512692723619-8b3e68365c9c?fit=crop&w=720&q=80";
    /// <summary>
    /// <see href="https://freesound.org/people/Timbre/sounds/546483/"/>
    /// </summary>
    const string k_SoundUrl = "https://freesound.org/data/previews/545/545403_9497060-lq.mp3";

    GcImageRequest? image;
    GcSoundRequest? sound;
    bool playing;

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
        gc.SetFontSize(32);
        gc.StopSound(GcSoundTrack.BGM1);
        image?.Dispose();
        sound?.Dispose();
        image = null;
        sound = null;
        playing = false;
    }

    public override void UpdateGame()
    {
        if (image == null) image = gc.Network.GetImage(k_ImageUrl);
        if (sound == null) sound = gc.Network.GetSound(k_SoundUrl, GcSoundFormat.Mp3);

        if (!gc.Pointer.Down) return;

        if (image != null &&
            (image.Status == GcRequestState.Failed ||
             image.Status == GcRequestState.Cancelled ||
             image.Status == GcRequestState.TimedOut))
        {
            image.Dispose();
            image = gc.Network.GetImage(k_ImageUrl);
        }

        if (sound != null &&
            (sound.Status == GcRequestState.Failed ||
             sound.Status == GcRequestState.Cancelled ||
             sound.Status == GcRequestState.TimedOut))
        {
            gc.StopSound(GcSoundTrack.BGM1);
            sound.Dispose();
            sound = gc.Network.GetSound(k_SoundUrl, GcSoundFormat.Mp3);
            playing = false;
        }
        else if (sound != null && sound.Status == GcRequestState.Succeeded)
        {
            playing = gc.PlaySound(sound, GcSoundTrack.BGM1, loop: true);
        }
    }

    public override void DrawGame()
    {
        gc.ClearScreen();

        if (image != null && image.Status == GcRequestState.Succeeded)
        {
            gc.SetColor(255, 255, 255);
            gc.DrawImage(image, 0, 0);
        }

        gc.SetColor(0, 0, 0);
        gc.DrawString(ShowImage(), 12, 12);
        gc.DrawString(ShowSound(), 12, 60);
    }

    string ShowImage()
    {
        if (image == null) return "画像を準備しています";
        return image.Status switch
        {
            GcRequestState.Pending => "画像を読み込み中です",
            GcRequestState.Succeeded => $"画像 {image.Width}x{image.Height}",
            GcRequestState.Failed => "画像を取得できません。画面を押すとやり直します",
            GcRequestState.Cancelled => "画像を取り消しました。画面を押すとやり直します",
            GcRequestState.TimedOut => "画像が時間切れです。画面を押すとやり直します",
            GcRequestState.Disposed => "画像を破棄しました",
            _ => "画像を確認中です"
        };
    }

    string ShowSound()
    {
        if (sound == null) return "音声を準備しています";
        if (playing) return $"音声を再生中です {sound.Duration:0.0} 秒";
        return sound.Status switch
        {
            GcRequestState.Pending => "音声を読み込み中です",
            GcRequestState.Succeeded => "画面を押すと音声を再生します",
            GcRequestState.Failed => "音声を取得できません。画面を押すとやり直します",
            GcRequestState.Cancelled => "音声を取り消しました。画面を押すとやり直します",
            GcRequestState.TimedOut => "音声が時間切れです。画面を押すとやり直します",
            GcRequestState.Disposed => "音声を破棄しました",
            _ => "音声を確認中です"
        };
    }
}
