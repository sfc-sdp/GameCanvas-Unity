/*------------------------------------------------------------*/
/// <summary>GameCanvas for Unity [Post Process Build]</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2016 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

/// <summary>
/// ビルド後処理
/// </summary>
public class PostProcessBuild
{
    [PostProcessBuild(1)]
    static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        DisableAutorotateAssertion(buildTarget, pathToBuiltProject);
    }

    /// <remarks>
    /// community.unity.com/t5/iOS-tvOS/UnityDefaultViewController-should-be-used-only-if-unity-is-set/td-p/2112497
    /// </remarks>
    static void DisableAutorotateAssertion(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.iOS) return;

        var viewControllerFileName = "UnityViewControllerBaseiOS.mm";
            
        var targetString = "\tNSAssert(UnityShouldAutorotate()";
        var filePath = Path.Combine(pathToBuiltProject, "Classes");
        filePath = Path.Combine(filePath, "UI");
        filePath = Path.Combine(filePath, viewControllerFileName);
        if (File.Exists(filePath))
        {
            var classFile = File.ReadAllText(filePath);
            var newClassFile = classFile.Replace(targetString, "\t//NSAssert(UnityShouldAutorotate()");
            if (classFile.Length != newClassFile.Length)
            {
                File.WriteAllText(filePath, newClassFile);
            }
        }
    }
}
