using GameCanvas;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public sealed class Game : GameBase
{
    /// <summary>
    /// <see href="https://unsplash.com/photos/uztw2giebSc"/>
    /// </summary>
    const string k_ImageUrl = "https://images.unsplash.com/photo-1512692723619-8b3e68365c9c?fit=crop&w=720&q=80";
    /// <summary>
    /// <see href="https://freesound.org/people/Timbre/sounds/546483/"/>
    /// </summary>
    const string k_SoundUrl = "https://freesound.org/data/previews/545/545403_9497060-lq.mp3";

    static readonly float2 k_LabelPos1 = new float2(12, 12);
    static readonly float2 k_LabelPos2 = new float2(12, 60);

    Texture2D m_OnlineImage;
    GcAvailability m_OnlineImageState;
    AudioClip m_OnlineSound;
    GcAvailability m_OnlineSoundState;

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
        gc.SetColor(gc.ColorBlack);
        gc.SetFontSize(32);
    }

    public override void UpdateGame()
    {
        if (m_OnlineImage == null && m_OnlineImageState != GcAvailability.NotAvailable)
        {
            m_OnlineImageState = gc.TryGetOnlineImage(k_ImageUrl, out m_OnlineImage);
        }

        if (m_OnlineSound == null && m_OnlineSoundState != GcAvailability.NotAvailable)
        {
            m_OnlineSoundState = gc.TryGetOnlineSound(k_SoundUrl, out m_OnlineSound);

            if (m_OnlineSoundState == GcAvailability.Ready)
            {
                gc.PlaySound(m_OnlineSound, GcSoundTrack.BGM1, true);
            }
        }
    }

    public override void DrawGame()
    {
        gc.ClearScreen();

        switch (m_OnlineImageState)
        {
            case GcAvailability.NotAvailable:
                gc.DrawString("画像 DL失敗", k_LabelPos1);
                break;

            case GcAvailability.NotReady:
                gc.DrawString("画像 DL中...", k_LabelPos1);
                break;

            case GcAvailability.Ready:
                gc.DrawTexture(m_OnlineImage);
                gc.DrawString("画像 表示中", k_LabelPos1);
                break;
        }

        switch (m_OnlineSoundState)
        {
            case GcAvailability.NotAvailable:
                gc.DrawString("音声 DL失敗", k_LabelPos2);
                break;

            case GcAvailability.NotReady:
                gc.DrawString("音声 DL中...", k_LabelPos2);
                break;

            case GcAvailability.Ready:
                gc.DrawString("音声 再生中", k_LabelPos2);
                break;
        }
    }
}
