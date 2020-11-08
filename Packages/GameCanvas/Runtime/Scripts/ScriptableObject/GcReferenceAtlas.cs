/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace GameCanvas
{
    sealed class GcReferenceAtlas : GcReference<SpriteAtlas, GcReferenceAtlas>
    {
        private Dictionary<string, Sprite> m_SpriteDict;

        public Sprite GetSprite(in string name)
        {
            if (m_SpriteDict != null && m_SpriteDict.TryGetValue(name, out Sprite sprite))
            {
                return sprite;
            }

            if (TryGet(out SpriteAtlas asset))
            {
                sprite = asset.GetSprite(name);

                if (m_SpriteDict == null)
                {
                    m_SpriteDict = new Dictionary<string, Sprite>(asset.spriteCount);
                }
                m_SpriteDict.Add(name, sprite);
                return sprite;
            }
            return null;
        }

        public bool TryGet(in string name, out Sprite sprite)
        {
            if (m_SpriteDict != null && m_SpriteDict.TryGetValue(name, out sprite))
            {
                return (sprite != null);
            }

            if (TryGet(out SpriteAtlas asset))
            {
                sprite = asset.GetSprite(name);

                if (m_SpriteDict == null)
                {
                    m_SpriteDict = new Dictionary<string, Sprite>(asset.spriteCount);
                }
                m_SpriteDict.Add(name, sprite);
                return (sprite != null);
            }
            sprite = null;
            return false;
        }

        public override void Unload()
        {
            m_SpriteDict = null;

            base.Unload();
        }
    }
}
