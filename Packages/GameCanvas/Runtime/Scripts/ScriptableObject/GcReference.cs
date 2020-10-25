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
    abstract class GcReference<TAsset, TClass> : ScriptableObject
        where TAsset : Object
        where TClass : GcReference<TAsset, TClass>
    {
        [SerializeField]
        private LazyLoadReference<TAsset> m_Reference;
        [System.NonSerialized]
        private TAsset m_Asset;

        public bool IsValid => m_Reference.isSet;

        public TAsset Get()
        {
            if (m_Asset)
            {
                return m_Asset;
            }
            return m_Asset = (m_Reference.isSet ? m_Reference.asset : null);
        }

        public bool TryGet(out TAsset asset)
        {
            if (m_Asset)
            {
                asset = m_Asset;
                return true;
            }
            if (m_Reference.isSet)
            {
                asset = m_Asset = m_Reference.asset;
                return true;
            }
            asset = null;
            return false;
        }

        public virtual void Unload()
        {
            if (m_Asset)
            {
                Resources.UnloadAsset(m_Asset);
                m_Asset = null;
            }
        }

        public static TClass Load(in string path) => Resources.Load<TClass>(path);

        public static ResourceRequest LoadAsync(in string path) => Resources.LoadAsync<TClass>(path);

#if UNITY_EDITOR
        public static TClass Write(in string assetPath, in TAsset asset)
        {
            var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<TClass>(assetPath);
            if (obj == null)
            {
                obj = CreateInstance<TClass>();
                obj.m_Reference = asset;
                UnityEditor.AssetDatabase.CreateAsset(obj, assetPath);
                UnityEditor.AssetDatabase.SaveAssets();
            }
            else if (obj.m_Reference.asset != asset)
            {
                obj.m_Reference = asset;
                UnityEditor.EditorUtility.SetDirty(obj);
                UnityEditor.AssetDatabase.SaveAssets();
            }
            return obj;
        }
#endif // UNITY_EDITOR
    }
}
