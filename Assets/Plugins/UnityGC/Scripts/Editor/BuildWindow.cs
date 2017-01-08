/*------------------------------------------------------------*/
/// <summary>GameCanvas for Unity [Build Window]</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2017 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/
using UnityEditor;
using UnityEngine;

namespace GameCanvas.Editor
{
    /// <summary>
    /// ビルドウィンドウ
    /// </summary>
    public class BuildWindow : EditorWindow
    {
        [SerializeField]
        static BuildOption option;
        static BuildWindow window;

        private GUIStyle largeText;

        [MenuItem("Window/GameCanvas Builder")]
        static public void Open()
        {
            if (window == null)
            {
                window = CreateInstance<BuildWindow>();
                window.Init();
            }
            
            window.ShowUtility();
        }

        private void Init()
        {
            option = new BuildOption();

            titleContent = new GUIContent("GameCanvas Builder");
            minSize = maxSize = new Vector2(500, 278);

            largeText = new GUIStyle();
            largeText.fontSize = 18;
        }

        void OnGUI()
        {
            if (option == null) Init();

            EditorGUI.indentLevel = 1;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("APP BUILDER", largeText, GUILayout.Height(32));

            DrawTextField("BUNDLE ID", ref option.bundleIdentifier);
            DrawTextField("BUNDLE VERSION", ref option.bundleVersion);
            DrawTextField("PRODUCT NAME", ref option.productName);
            DrawTextField("COMPANY NAME", ref option.companyName);
            DrawSaveFolderPath("OUTPUT FOLDER", ref option.outFolderPath);
            var target = (MobilePlatform)DrawEnumPopup("BUILD TARGET", option.target);
            if (target != option.target)
            {
                option.target = target;
                EditorUserBuildSettings.SwitchActiveBuildTarget((BuildTarget)option.target);
            }
            switch (option.target)
            {
                case MobilePlatform.Android:
                    option.minAndroidSdkVersion = (AndroidSdkVersions)DrawEnumPopup("SDK VERSION", option.minAndroidSdkVersion);
                    DrawToggle("USE IL2CPP", ref option.il2cpp);
                    break;

                case MobilePlatform.iOS:
                    option.iOSSdkVersion = (iOSSdkVersion)DrawEnumPopup("SDK VERSION", option.iOSSdkVersion);
                    break;
            }
            DrawToggle("DEVELOPMENT", ref option.isDevelopment);
            DrawToggle("PROFILING", ref option.connectProfiler);

            EditorGUILayout.Space();
            if (GUILayout.Button("BUILD", GUILayout.Height(25)))
            {
                Builder.Run(option);
                //Close();
            }
        }

        void DrawToggle(string name, ref bool value)
        {
            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(name, GUILayout.Width(120));
                value = EditorGUILayout.Toggle(value);
            EditorGUILayout.EndHorizontal();
        }

        void DrawTextField(string name, ref string value)
        {
            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(name, GUILayout.Width(120));
                value = EditorGUILayout.TextField(value, GUILayout.Width(350));
            EditorGUILayout.EndHorizontal();
        }

        void DrawSaveFolderPath(string name, ref string value)
        {
            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(name, GUILayout.Width(120));
                EditorGUILayout.SelectableLabel(value, GUILayout.Width(296));
                if (GUILayout.Button("Select", GUILayout.Width(50)))
                {
                    value = EditorUtility.SaveFolderPanel(name, value, "");
                }
            EditorGUILayout.EndHorizontal();
        }

        System.Enum DrawEnumPopup(string name, System.Enum value)
        {
            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(name, GUILayout.Width(120));
                value = (MobilePlatform)EditorGUILayout.EnumPopup(value, GUILayout.Width(350));
            EditorGUILayout.EndHorizontal();

            return value;
        }
    }
}
