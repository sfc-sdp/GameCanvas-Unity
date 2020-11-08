/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameCanvas.Editor
{
    public sealed class GcEditorBuilder : EditorWindow
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        const string k_DataPath = "Library/GameCanvasBuilder.json";
        const int k_DataVersion = 2;

        GUIStyle m_LargeText;
        Option m_Option;

        #endregion

        //----------------------------------------------------------
        #region 内部定義
        //----------------------------------------------------------

        /// <summary>
        /// ビルドプラットフォーム
        /// </summary>
        [System.Serializable]
        public enum Platform
        {
            Android = BuildTarget.Android,
            iOS = BuildTarget.iOS
        }

        /// <summary>
        /// ビルドオプション
        /// </summary>
        [System.Serializable]
        public struct Option
        {
            /// <summary>
            /// ビルド後に自動実行するかどうか
            /// </summary>
            public bool mAndRun;

            /// <summary>
            /// バンドルID
            /// </summary>
            public string mApplicationId;

            /// <summary>
            /// バンドルバージョン
            /// </summary>
            public string mBundleVersion;

            /// <summary>
            /// 会社名 or 開発者名
            /// </summary>
            public string mCompanyName;

            /// <summary>
            /// Android 最小SDKバージョン
            /// </summary>
            public AndroidSdkVersions mMinimumSdkVersion;

            /// <summary>
            /// 出力先フォルダ
            /// </summary>
            public string mOutFolderPath;

            /// <summary>
            /// ビルドプラットフォーム
            /// </summary>
            public Platform mPlatform;

            /// <summary>
            /// 製品名
            /// </summary>
            public string mProductName;

            /// <summary>
            /// iOS デバイスかシミュレーターか
            /// </summary>
            public iOSSdkVersion mSdkType;

            /// <summary>
            /// Android ターゲットSDKバージョン
            /// </summary>
            public AndroidSdkVersions mTargetSdkVersion;
        }

        [System.Serializable]
        sealed class Data
        {
            public Option m_Option;
            public int m_Version;

            public Data(Option option)
            {
                m_Option = option;
                m_Version = k_DataVersion;
            }
        }

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static void Build(Option option)
        {
            PlayerSettings.bundleVersion = option.mBundleVersion;
            PlayerSettings.productName = option.mProductName;
            PlayerSettings.companyName = option.mCompanyName;
            EditorUserBuildSettings.connectProfiler = false;
            EditorUserBuildSettings.development = false;

            var buildTarget = (BuildTarget)option.mPlatform;
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            if ((BuildTarget)option.mPlatform != EditorUserBuildSettings.activeBuildTarget)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
            }
            PlayerSettings.SetApplicationIdentifier(buildTargetGroup, option.mApplicationId);

            if (!Directory.Exists(option.mOutFolderPath))
            {
                Directory.CreateDirectory(option.mOutFolderPath);
            }

            var outFilePath = Path.Combine(option.mOutFolderPath, option.mProductName);
            switch (option.mPlatform)
            {
                case Platform.Android:
                    if (Path.GetExtension(outFilePath) != "apk") outFilePath += ".apk";

                    PlayerSettings.Android.minSdkVersion = option.mMinimumSdkVersion;
                    PlayerSettings.Android.targetSdkVersion = option.mTargetSdkVersion;
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                    PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard_2_0);
#if UNITY_ANDROID
                    UnityEditor.Android.UserBuildSettings.symlinkSources = true;
#endif // UNITY_ANDROID

                    // 既に出力ファイルがあれば退避させておく
                    if (File.Exists(outFilePath))
                    {
                        File.Move(outFilePath, outFilePath.Remove(outFilePath.Length - 4) + "." + File.GetLastWriteTime(outFilePath).ToString("MMddHHmmss") + ".apk");
                    }
                    break;

                case Platform.iOS:
                    PlayerSettings.iOS.sdkVersion = option.mSdkType;
                    PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
                    PlayerSettings.iOS.targetOSVersionString = string.Empty;
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
                    PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, (int)AppleMobileArchitecture.ARM64);
                    PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_Standard_2_0);
                    if (string.IsNullOrEmpty(PlayerSettings.iOS.cameraUsageDescription)) PlayerSettings.iOS.cameraUsageDescription = "GameCanvas";
                    if (string.IsNullOrEmpty(PlayerSettings.iOS.locationUsageDescription)) PlayerSettings.iOS.locationUsageDescription = "GameCanvas";
                    if (string.IsNullOrEmpty(PlayerSettings.iOS.microphoneUsageDescription)) PlayerSettings.iOS.microphoneUsageDescription = "GameCanvas";
                    EditorUserBuildSettings.symlinkLibraries = true;
                    break;

                default:
                    return;
            }


            var buildOption = BuildOptions.CompressWithLz4;
            buildOption |= option.mAndRun ? BuildOptions.AutoRunPlayer : BuildOptions.ShowBuiltPlayer;

            // ビルドを実行する
            var report = BuildPipeline.BuildPlayer(
                GetEnabledScenePaths(),
                outFilePath,
                (BuildTarget)option.mPlatform,
                buildOption
            );

            // エラー出力
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
            {
                Debug.LogError("ビルドに失敗しました\n" + report.summary.ToString());
            }
        }

        public static void OpenWindow()
        {
            GetWindow<GcEditorBuilder>(true).Show();
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal static string[] GetEnabledScenePaths()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
        }

        internal static void OnLaunch()
        {
            if (!GcEditorSettings.CurrentSettings.CheckBuildTargetOnLaunchEditor) return;

            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.iOS:
                case BuildTarget.Android:
                    return;
            }

            const string title = "GameCanvas";
            const string message = "ビルドターゲットがスマートデバイス以外に設定されています。切り替えますか？";
            const string button0 = "iOSに設定";
            const string button1 = "このまま";
            const string button2 = "Androidに設定";
            switch (EditorUtility.DisplayDialogComplex(title, message, button0, button1, button2))
            {
                case 0:
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
                    break;

                case 2:
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                    break;
            }
        }

#if UNITY_ANDROID
        internal static void OnPostGenerateGradleAndroidProject(in string path)
        {
            var manifestPath = Path.Combine(path, "src/main/AndroidManifest.xml");

            using (var manifest = AndroidManifest.Load(manifestPath))
            {
                manifest.SkipPermissionsDialog(true);
            }
        }
#endif // UNITY_ANDROID

        System.Enum DrawEnumPopup(string name, System.Enum value, ref bool isChange)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(120));
            var prev = value;
            value = EditorGUILayout.EnumPopup(value, GUILayout.Width(350));
            EditorGUILayout.EndHorizontal();
            isChange |= (prev != value);
            return value;
        }

        bool DrawSaveFolderPath(string name, ref string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(120));
            EditorGUILayout.SelectableLabel(value, GUILayout.Width(296));
            var prev = value;
            if (GUILayout.Button("Select", GUILayout.Width(50)))
            {
                var newValue = EditorUtility.SaveFolderPanel(name, value, "");
                if (!string.IsNullOrEmpty(newValue)) value = newValue;
            }
            EditorGUILayout.EndHorizontal();
            return (prev != value);
        }

        bool DrawTextField(string name, ref string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(120));
            var prev = value;
            value = EditorGUILayout.TextField(value, GUILayout.Width(350));
            EditorGUILayout.EndHorizontal();
            return (prev != value);
        }

        bool DrawToggle(string name, ref bool value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(120));
            var prev = value;
            value = EditorGUILayout.Toggle(value);
            EditorGUILayout.EndHorizontal();
            return (prev != value);
        }

        void InitOption()
        {
            m_Option = default;
            m_Option.mApplicationId = PlayerSettings.applicationIdentifier.Replace(" ", "");
            m_Option.mBundleVersion = PlayerSettings.bundleVersion;
            m_Option.mProductName = PlayerSettings.productName;
            m_Option.mCompanyName = PlayerSettings.companyName;
            m_Option.mOutFolderPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Build"));
            m_Option.mAndRun = false;
            m_Option.mTargetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            m_Option.mMinimumSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
            m_Option.mSdkType = iOSSdkVersion.DeviceSDK;

            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                default:
                case BuildTarget.Android:
                    m_Option.mPlatform = Platform.Android;
                    break;

                case BuildTarget.iOS:
                    m_Option.mPlatform = Platform.iOS;
                    break;
            }
        }

        void LoadData()
        {
            var loaded = false;
            if (File.Exists(k_DataPath))
            {
                var json = File.ReadAllText(k_DataPath);
                var data = JsonUtility.FromJson<Data>(json);
                if (data.m_Version == k_DataVersion)
                {
                    m_Option = data.m_Option;
                    loaded = true;
                }
            }
            if (!loaded) InitOption();
        }

#pragma warning disable IDE0051
        void OnGUI()
        {
            EditorGUI.indentLevel = 1;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("APP BUILDER", m_LargeText, GUILayout.Height(32));

            var isChange = false;
            if (DrawTextField("APPLICATION ID", ref m_Option.mApplicationId))
            {
                isChange |= true;
                m_Option.mApplicationId = m_Option.mApplicationId.Replace(" ", "");
            }
            isChange |= DrawTextField("BUNDLE VERSION", ref m_Option.mBundleVersion);
            isChange |= DrawTextField("PRODUCT NAME", ref m_Option.mProductName);
            isChange |= DrawTextField("COMPANY NAME", ref m_Option.mCompanyName);
            isChange |= DrawSaveFolderPath("OUTPUT FOLDER", ref m_Option.mOutFolderPath);
            m_Option.mPlatform = (Platform)DrawEnumPopup("BUILD TARGET", m_Option.mPlatform, ref isChange);
            switch (m_Option.mPlatform)
            {
                case Platform.Android:
                    m_Option.mTargetSdkVersion = (AndroidSdkVersions)DrawEnumPopup("TARGET SDK", m_Option.mTargetSdkVersion, ref isChange);
                    m_Option.mMinimumSdkVersion = (AndroidSdkVersions)DrawEnumPopup("MINIMUM SDK", m_Option.mMinimumSdkVersion, ref isChange);
                    break;

                case Platform.iOS:
                    m_Option.mSdkType = (iOSSdkVersion)DrawEnumPopup("SDK VERSION", m_Option.mSdkType, ref isChange);
                    break;
            }

            // 保存
            if (isChange) SaveData();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            var button1 = GUILayout.Button("BUILD", GUILayout.Height(25));
            var button2 = m_Option.mPlatform == Platform.Android && GUILayout.Button("BUILD & RUN", GUILayout.Height(25));
            EditorGUILayout.EndHorizontal();

            if (button1)
            {
                m_Option.mAndRun = false;
                Build(m_Option);
                //Close();
            }
            if (button2)
            {
                m_Option.mAndRun = true;
                Build(m_Option);
                //Close();
            }
        }
#pragma warning restore IDE0051

        void SaveData()
        {
            var data = new Data(m_Option);
            var json = JsonUtility.ToJson(data);
            File.WriteAllText(k_DataPath, json);
        }

        new void Show()
        {
            titleContent = new GUIContent("GameCanvas Builder");
            minSize = maxSize = new Vector2(500, 260);

            m_LargeText = new GUIStyle
            {
                fontSize = 18
            };

            LoadData();
            base.Show();
        }
        #endregion
    }
}
