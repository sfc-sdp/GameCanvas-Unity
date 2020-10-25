/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Text;
using System.Xml;

namespace GameCanvas.Editor
{
    /// <summary>
    /// AndroidManifest
    /// </summary>
    public sealed class AndroidManifest : XmlDocument, System.IDisposable
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        const string k_XmlNamespaceAndroid = "http://schemas.android.com/apk/res/android";
        const string k_XmlNamespaceTools = "http://schemas.android.com/tools";

        readonly XmlNamespaceManager m_NamespaceManager;
        readonly string m_Path;
        XmlElement m_ApplicationElement;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public AndroidManifest(in string path)
        {
            m_Path = path;

            m_NamespaceManager = new XmlNamespaceManager(NameTable);
            m_NamespaceManager.AddNamespace("android", k_XmlNamespaceAndroid);
            m_NamespaceManager.AddNamespace("tools", k_XmlNamespaceTools);
        }

        public bool SaveOnDispose { get; set; } = true;

        public static AndroidManifest Load(in string path)
        {
            var manifest = new AndroidManifest(path);
            manifest.Load();
            return manifest;
        }

        public void Dispose()
        {
            if (SaveOnDispose) Save();
        }

        public void Load()
        {
            using (var reader = new XmlTextReader(m_Path))
            {
                reader.Read();
                Load(reader);
            }
            m_ApplicationElement = SelectSingleNode("/manifest/application") as XmlElement;
        }

        public void Save()
        {
            using (var writer = new XmlTextWriter(m_Path, new UTF8Encoding(false)))
            {
                writer.Formatting = Formatting.Indented;
                Save(writer);
            }
        }

        public void SkipPermissionsDialog(in bool skip)
        {
            SetMetaData(m_ApplicationElement, "unityplayer.SkipPermissionsDialog", skip ? "true" : "false");
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        void SetMetaData(in XmlElement target, in string key, in string value, string prefix = "android", in string ns = k_XmlNamespaceAndroid)
        {
            if (TryGetMetaDataElement(target, prefix, key, out var elem))
            {
                var attr = elem.GetAttributeNode("value");
                attr.Value = value;
            }
            else
            {
                var attr1 = CreateAttribute(prefix, "name", ns);
                attr1.Value = key;
                var attr2 = CreateAttribute(prefix, "value", ns);
                attr2.Value = value;
                elem = CreateElement("meta-data");
                elem.SetAttributeNode(attr1);
                elem.SetAttributeNode(attr2);
                target.AppendChild(elem);
            }
        }

        bool TryGetMetaDataElement(in XmlElement parent, in string prefix, in string name, out XmlElement elem)
        {
            elem = parent.SelectSingleNode($"/meta-data[@{prefix}:name='{name}']", m_NamespaceManager) as XmlElement;
            return (elem != null);
        }
        #endregion
    }
}
