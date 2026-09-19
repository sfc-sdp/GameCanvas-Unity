#nullable enable
using System;
using System.IO;
using GameCanvas.Diagnostics;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameCanvas.Editor
{
    public static class GcValidationBuild
    {
        const string ScenePath = "Assets/GameCanvas/Validation/DeviceProbe.unity";
        public static void Prepare()
        {
            RequireVersion();
            GcAssetCatalogBuilder.Refresh();
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath)!);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var probe = new GameObject("GameCanvas device probe").AddComponent<GcDeviceProbe>();
            probe.UiFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/GameCanvas/DefaultFont.ttf");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }
        public static void Android() => Build(BuildTarget.Android, "Build/Validation/GameCanvasProbe.apk");
        public static void IOS() => Build(BuildTarget.iOS, "Build/Validation/iOS");
        public static void IOSSimulator() => Build(BuildTarget.iOS, "Build/Validation/iOSSimulator", true);
        public static void Web() => Build(BuildTarget.WebGL, "Build/Validation/Web");

        static void Build(BuildTarget target, string path, bool simulator = false)
        {
            RequireVersion();
            if (EditorUserBuildSettings.activeBuildTarget != target)
                throw new BuildFailedException("起動引数の -buildTarget を対象に合わせてください。");
            if (!File.Exists(ScenePath)) throw new BuildFailedException("先に GcValidationBuild.Prepare を別プロセスで実行してください。");
            var named = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target));
            var originalName = PlayerSettings.productName;
            var originalSdk = PlayerSettings.iOS.sdkVersion;
            var originalSimulatorArchitecture = PlayerSettings.iOS.simulatorSdkArchitecture;
            var originalIdentifier = PlayerSettings.GetApplicationIdentifier(named);
            try
            {
                PlayerSettings.SetApplicationIdentifier(named, "jp.ac.keio.sfc.gamecanvas.probe");
                PlayerSettings.productName = "GameCanvas Probe";
                PlayerSettings.SetScriptingBackend(named, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
                PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.iOS.targetOSVersionString = "15.0";
                if (target == BuildTarget.iOS)
                {
                    PlayerSettings.iOS.sdkVersion = simulator ? iOSSdkVersion.SimulatorSDK : iOSSdkVersion.DeviceSDK;
                    if (simulator) PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.ARM64;
                }
                PlayerSettings.iOS.cameraUsageDescription = "授業の検証でカメラ映像を画面に表示します。";
                PlayerSettings.iOS.locationUsageDescription = "授業の検証で現在地の取得と精度を確認します。";
                PlayerSettings.iOS.microphoneUsageDescription = "";
                PlayerSettings.insecureHttpOption = InsecureHttpOption.NotAllowed;
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                EditorUserBuildSettings.buildAppBundle = false;
                Directory.CreateDirectory("Build/Validation");
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath }, locationPathName = path,
                    target = target, options = BuildOptions.Development
                });
                var summary = report.summary;
                File.WriteAllText($"Build/Validation/build-{(simulator ? "iOSSimulator" : target.ToString())}.json", JsonUtility.ToJson(new Result
                {
                    unity = Application.unityVersion, platform = simulator ? "iOSSimulator" : target.ToString(), result = summary.result.ToString(),
                    errors = summary.totalErrors, seconds = summary.totalTime.TotalSeconds,
                    deviceVerified = false
                }, true));
                if (summary.result != BuildResult.Succeeded) throw new BuildFailedException(summary.result.ToString());
            }
            finally
            {
                PlayerSettings.productName = originalName;
                PlayerSettings.iOS.sdkVersion = originalSdk;
                PlayerSettings.iOS.simulatorSdkArchitecture = originalSimulatorArchitecture;
                PlayerSettings.SetApplicationIdentifier(named, originalIdentifier);
            }
        }
        static void RequireVersion()
        {
            if (Application.unityVersion != "6000.6.2f1") throw new BuildFailedException("Unity 6000.6.2f1で実行してください。");
        }
        [Serializable] sealed class Result
        {
            public string unity = "", platform = "", result = "";
            public int errors; public double seconds; public bool deviceVerified;
        }
    }
}
