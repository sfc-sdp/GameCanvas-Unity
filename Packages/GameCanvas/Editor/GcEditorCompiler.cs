/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2022 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor.Build.Player;

namespace GameCanvas.Editor
{
    public static class GcEditorCompiler
    {
        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static bool TryCompile(in GcRuntimePlatform target, bool developmentBuild)
        {
            const string k_DllOutputPath = "Temp/PlayerScriptOutput";

            var options = developmentBuild
                ? ScriptCompilationOptions.DevelopmentBuild
                : ScriptCompilationOptions.None;
            var settings = new ScriptCompilationSettings()
            {
                target = target.ToBuildTarget(),
                group = target.ToBuildTargetGroup(),
                options = options,
                extraScriptingDefines = System.Array.Empty<string>()
            };
            DeleteFolderIfExists();
            var result = PlayerBuildInterface.CompilePlayerScripts(settings, k_DllOutputPath);
            DeleteFolderIfExists();
            return (result.typeDB != null && result.assemblies.Count > 0);

            static void DeleteFolderIfExists()
            {
                if (Directory.Exists(k_DllOutputPath))
                {
                    Directory.Delete(k_DllOutputPath, true);
                }
            }
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal static string OnGeneratedCSProject(string _, string content)
        {
            var document = XDocument.Parse(content);
            ConvertToModernMSBuild(document);
            AddNullable(document);
            RemoveBooLangRef(document);
            return document.Root.ToString();
        }

        static void ConvertToModernMSBuild(in XDocument document)
        {
#if false
            document.Descendants()
                .Attributes()
                .Where(x => x.IsNamespaceDeclaration)
                .Remove();
            foreach (var elem in document.Descendants())
            {
                elem.Name = elem.Name.LocalName;
            }

            var root = document.Root;
            root.RemoveAttributes();
            root.SetAttributeValue("Sdk", "Microsoft.NET.Sdk");

            root.Descendants()
                .Where(IsRemoveNode)
                .Remove();

            var urs = root.Descendants()
                .Where(IsUpdateRefNode);
            foreach (var ur in urs)
            {
                ur.SetAttributeValue("Update", (string)ur.Attribute("Include"));
                ur.SetAttributeValue("Include", null);
            }

            var pg = root.Descendants()
                .Where(x => x.Name.LocalName == "PropertyGroup")
                .First();
            pg.AddFirst(new XElement("AssetTargetFallback", "net472;net471;net40"));
            pg.AddFirst(new XElement("TargetFramework", "netstandard2.0"));
            pg.Add(new XElement("GenerateAssemblyInfo", false));
            pg.Add(new XElement("EnableDefaultItems", false));

            var prs = root.Descendants()
                .Where(x => x.Name.LocalName == "ProjectReference");
            foreach (var pr in prs) pr.RemoveNodes();

            static bool IsRemoveNode(XElement elem)
            {
                switch (elem.Name.LocalName)
                {
                    case "Configuration":
                    case "Platform":
                    case "ProductVersion":
                    case "SchemaVersion":
                    case "ProjectGuid":
                    case "AppDesignerFolder":
                    case "AssemblyName":
                    case "TargetFrameworkVersion":
                    case "FileAlignment":
                    case "DebugSymbols":
                    case "DebugType":
                    case "Optimize":
                    case "ErrorReport":
                    case "AddAdditionalExplicitAssemblyReferences":
                    case "ProjectTypeGuids":
                    case "Import":
                        return true;
                }

                if (elem.Name.LocalName == "WarningLevel")
                {
                    return elem.Value == "4";
                }
                else if (elem.Name.LocalName == "AllowUnsafeBlocks")
                {
                    return elem.Value == "False";
                }

                return false;
            }

            static bool IsUpdateRefNode(XElement elem)
            {
                if (elem.Name.LocalName != "Reference") return false;

                switch ((string)elem.Attribute("Include"))
                {
                    case "System":
                    case "System.Core":
                    case "System.Data":
                    case "System.Drawing":
                    case "System.IO.Compression.FileSystem":
                    case "System.Numerics":
                    case "System.Runtime.Serialization":
                    case "System.Xml":
                    case "System.Xml.Linq":
                        return true;
                }
                return false;
            }
#endif // false
        }

        static void AddNullable(in XDocument document)
        {
#if false
            var lv = document.Root.Descendants()
                .Where(x => x.Name.LocalName == "LangVersion")
                .Single();
            lv.AddAfterSelf(new XElement(lv.Name.Namespace + "Nullable", "enable"));
#endif // false
        }

        static void RemoveBooLangRef(in XDocument document)
        {
            document.Root.Descendants()
                .Where(x => x.Name.LocalName == "Reference")
                .Where(x => (string)x.Attribute("Include") == "Boo.Lang")
                .Remove();
        }
        #endregion
    }
}
