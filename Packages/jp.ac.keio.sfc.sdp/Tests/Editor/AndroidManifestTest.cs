#nullable enable
using System.IO;
using NUnit.Framework;
namespace GameCanvas.Editor.Tests
{
    public class AndroidManifestTest
    {
        [Test] public void PermissionMetadataUpdatesExistingNamespacedValueWithoutDuplicates()
        {
            var path=Path.GetTempFileName();
            try
            {
                File.WriteAllText(path,"<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\"><application><meta-data android:name=\"unityplayer.SkipPermissionsDialog\" android:value=\"false\"/></application></manifest>");
                using(var manifest=AndroidManifest.Load(path)){manifest.SkipPermissionsDialog(true);manifest.SkipPermissionsDialog(true);}
                using(var manifest=AndroidManifest.Load(path))
                {
                    Assert.That(manifest.SelectNodes("/manifest/application/meta-data")!.Count,Is.EqualTo(1));
                    Assert.That(manifest.SelectSingleNode("/manifest/application/meta-data")!.Attributes!["value","http://schemas.android.com/apk/res/android"]!.Value,Is.EqualTo("true"));
                    manifest.SkipPermissionsDialog(false);
                }
                Assert.That(File.ReadAllText(path),Does.Contain("android:value=\"false\""));
            }
            finally {File.Delete(path);}
        }
    }
}
