/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2019 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

#if UNITY_EDITOR
namespace GameCanvas.Editor
{
    using System.IO;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public sealed class Builder : EditorWindow
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private const string cDataPath = "Library/GameCanvasBuilder.json";
        private const int cDataVersion = 1;

        private GUIStyle mLargeText;
        private Option mOption;

        #endregion

        //----------------------------------------------------------
        #region 定義
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
            /// バンドルID
            /// </summary>
            public string mApplicationId;
            /// <summary>
            /// バンドルバージョン
            /// </summary>
            public string mBundleVersion;
            /// <summary>
            /// 製品名
            /// </summary>
            public string mProductName;
            /// <summary>
            /// 会社名 or 開発者名
            /// </summary>
            public string mCompanyName;
            /// <summary>
            /// 出力先フォルダ
            /// </summary>
            public string mOutFolderPath;
            /// <summary>
            /// ビルドプラットフォーム
            /// </summary>
            public Platform mPlatform;
            /// <summary>
            /// Android ターゲットSDKバージョン
            /// </summary>
            public AndroidSdkVersions mTargetSdkVersion;
            /// <summary>
            /// Android 最小SDKバージョン
            /// </summary>
            public AndroidSdkVersions mMinimumSdkVersion;
            /// <summary>
            /// iOS デバイスかシミュレーターか
            /// </summary>
            public iOSSdkVersion mSdkType;
            /// <summary>
            /// IL2CPPビルドかどうか
            /// </summary>
            public bool mIsIl2cpp;
            /// <summary>
            /// 開発ビルドかどうか
            /// </summary>
            public bool mIsDevelopment;
            /// <summary>
            /// プロファイラに自動接続するかどうか
            /// </summary>
            public bool mIsConnectProfiler;
            /// <summary>
            /// ビルド後に自動実行するかどうか
            /// </summary>
            public bool mAndRun;
        }

        [System.Serializable]
        private sealed class Data
        {
            public Option mOption;
            public int mVersion;

            public Data(Option option)
            {
                mOption = option;
                mVersion = cDataVersion;
            }
        }

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        [MenuItem("GameCanvas/アプリをビルドする")]
        public static void OpenWindow()
        {
            GetWindow<Builder>(true).Show();
        }

        public static void Build(Option option)
        {
            PlayerSettings.bundleVersion = option.mBundleVersion;
            PlayerSettings.productName = option.mProductName;
            PlayerSettings.companyName = option.mCompanyName;
            EditorUserBuildSettings.connectProfiler = option.mIsConnectProfiler;
            EditorUserBuildSettings.development = option.mIsDevelopment;

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
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, option.mIsIl2cpp ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);

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
                    break;

                default:
                    return;
            }


            var buildOption = BuildOptions.CompressWithLz4;
            if (option.mIsConnectProfiler) buildOption |= BuildOptions.ConnectWithProfiler;
            buildOption |= option.mAndRun ? BuildOptions.AutoRunPlayer : BuildOptions.ShowBuiltPlayer;

            // ビルドを実行する
            var report = BuildPipeline.BuildPlayer(
                getEnabledScenePaths(),
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

        #endregion

        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        private new void Show()
        {
            titleContent = new GUIContent("GameCanvas Builder");
            minSize = maxSize = new Vector2(500, 278);

            mLargeText = new GUIStyle();
            mLargeText.fontSize = 18;

            loadData();
            base.Show();
        }

        private void OnGUI()
        {
            EditorGUI.indentLevel = 1;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("APP BUILDER", mLargeText, GUILayout.Height(32));

            var isChange = false;
            if (drawTextField("APPLICATION ID", ref mOption.mApplicationId))
            {
                isChange |= true;
                mOption.mApplicationId = mOption.mApplicationId.Replace(" ", "");
            }
            isChange |= drawTextField("BUNDLE VERSION", ref mOption.mBundleVersion);
            isChange |= drawTextField("PRODUCT NAME", ref mOption.mProductName);
            isChange |= drawTextField("COMPANY NAME", ref mOption.mCompanyName);
            isChange |= drawSaveFolderPath("OUTPUT FOLDER", ref mOption.mOutFolderPath);
            mOption.mPlatform = (Platform)drawEnumPopup("BUILD TARGET", mOption.mPlatform, ref isChange);
            switch (mOption.mPlatform)
            {
                case Platform.Android:
                    mOption.mTargetSdkVersion = (AndroidSdkVersions)drawEnumPopup("TARGET SDK", mOption.mTargetSdkVersion, ref isChange);
                    mOption.mMinimumSdkVersion = (AndroidSdkVersions)drawEnumPopup("MINIMUM SDK", mOption.mMinimumSdkVersion, ref isChange);
                    break;

                case Platform.iOS:
                    mOption.mSdkType = (iOSSdkVersion)drawEnumPopup("SDK VERSION", mOption.mSdkType, ref isChange);
                    break;
            }
            if (drawToggle("DEVELOPMENT", ref mOption.mIsDevelopment))
            {
                isChange |= true;
                mOption.mIsIl2cpp = !mOption.mIsDevelopment;
            }
            EditorGUI.BeginDisabledGroup(!mOption.mIsDevelopment);
            isChange |= drawToggle("PROFILING", ref mOption.mIsConnectProfiler);
            EditorGUI.EndDisabledGroup();

            // 保存
            if (isChange) saveData();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            var button1 = GUILayout.Button("BUILD", GUILayout.Height(25));
            var button2 = mOption.mPlatform == Platform.Android && GUILayout.Button("BUILD & RUN", GUILayout.Height(25));
            EditorGUILayout.EndHorizontal();

            if (button1)
            {
                mOption.mAndRun = false;
                Build(mOption);
                //Close();
            }
            if (button2)
            {
                mOption.mAndRun = true;
                Build(mOption);
                //Close();
            }
        }

        private bool drawToggle(string name, ref bool value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(120));
            var prev = value;
            value = EditorGUILayout.Toggle(value);
            EditorGUILayout.EndHorizontal();
            return (prev != value);
        }

        private bool drawTextField(string name, ref string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(120));
            var prev = value;
            value = EditorGUILayout.TextField(value, GUILayout.Width(350));
            EditorGUILayout.EndHorizontal();
            return (prev != value);
        }

        private bool drawSaveFolderPath(string name, ref string value)
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

        private System.Enum drawEnumPopup(string name, System.Enum value, ref bool isChange)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(120));
            var prev = value;
            value = EditorGUILayout.EnumPopup(value, GUILayout.Width(350));
            EditorGUILayout.EndHorizontal();
            isChange |= (prev != value);
            return value;
        }

        private void loadData()
        {
            var loaded = false;
            if (File.Exists(cDataPath))
            {
                var json = File.ReadAllText(cDataPath);
                var data = JsonUtility.FromJson<Data>(json);
                if (data.mVersion == cDataVersion)
                {
                    mOption = data.mOption;
                    loaded = true;
                }
            }
            if (!loaded) initOption();
        }

        private void saveData()
        {
            var data = new Data(mOption);
            var json = JsonUtility.ToJson(data);
            File.WriteAllText(cDataPath, json);
        }

        private void initOption()
        {
            mOption = default(Option);
            mOption.mApplicationId = PlayerSettings.applicationIdentifier.Replace(" ", "");
            mOption.mBundleVersion = PlayerSettings.bundleVersion;
            mOption.mProductName = PlayerSettings.productName;
            mOption.mCompanyName = PlayerSettings.companyName;
            mOption.mOutFolderPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Build"));
            mOption.mIsIl2cpp = false;
            mOption.mIsDevelopment = false;
            mOption.mIsConnectProfiler = false;
            mOption.mAndRun = false;
            mOption.mTargetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            mOption.mMinimumSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
            mOption.mSdkType = iOSSdkVersion.DeviceSDK;

            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                default:
                case BuildTarget.Android:
                    mOption.mPlatform = Platform.Android;
                    break;

                case BuildTarget.iOS:
                    mOption.mPlatform = Platform.iOS;
                    break;
            }
        }

        private void switchPlatform(Platform platform)
        {
            // TODO
        }

        /// <summary>
        /// 現在有効なシーンの一覧を取得します
        /// </summary>
        /// <returns></returns>
        internal static string[] getEnabledScenePaths()
        {
            var scenePathList = new List<string>();

            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled) scenePathList.Add(scene.path);
            }

            return scenePathList.ToArray();
        }

        #endregion
    }
}
#endif //UNITY_EDITOR
