/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using UnityEngine;

namespace GameCanvas
{
    sealed class GcReferenceText : GcReference<TextAsset, GcReferenceText>
    {
        private string m_Text;
        private byte[] m_Bytes;

        public string GetText()
        {
            if (m_Text != null)
            {
                return m_Text;
            }
            if (TryGet(out TextAsset asset))
            {
                return m_Text = asset.text;
            }
            return null;
        }

        public byte[] GetBytes()
        {
            if (m_Bytes != null)
            {
                return m_Bytes;
            }
            if (TryGet(out TextAsset asset))
            {
                return m_Bytes = asset.bytes;
            }
            return null;
        }

        public bool TryGet(out string text)
        {
            if (m_Text != null)
            {
                text = m_Text;
                return true;
            }
            if (TryGet(out TextAsset asset))
            {
                text = m_Text = asset.text;
                return true;
            }
            text = null;
            return false;
        }

        public bool TryGet(out byte[] bytes)
        {
            if (m_Bytes != null)
            {
                bytes = m_Bytes;
                return true;
            }
            if (TryGet(out TextAsset asset))
            {
                bytes = m_Bytes = asset.bytes;
                return true;
            }
            bytes = null;
            return false;
        }

        public override void Unload()
        {
            m_Text = null;
            m_Bytes = null;

            base.Unload();
        }
    }
}
