/**
 * GameCanvas for Unity [Post ProcessBuild]
 * 
 * Copyright (c) 2015-2016 Seibe TAKAHASHI
 * 
 * This software is released under the MIT License.
 * http://opensource.org/licenses/mit-license.php
 */
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

/// <summary>
/// ビルド後処理
/// </summary>
public class PostProcessBuild
{
    [PostProcessBuild]
    static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
#if UNITY_IOS
        DisableAutorotateAssertion(buildTarget, pathToBuiltProject);
#endif
    }

    /// <remarks>
    /// community.unity.com/t5/iOS-tvOS/UnityDefaultViewController-should-be-used-only-if-unity-is-set/td-p/2112497
    /// </remarks>
    static void DisableAutorotateAssertion(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
#if UNITY_5_3_OR_NEWER && !UNITY_5_3_0
            var viewControllerFileName = "UnityViewControllerBaseiOS.mm";
#else
            var viewControllerFileName = "UnityViewControllerBase.mm";
#endif
            
            var targetString = "\tNSAssert(UnityShouldAutorotate()";
            var filePath = Path.Combine(pathToBuiltProject, "Classes");
            filePath = Path.Combine(filePath, "UI");
            filePath = Path.Combine(filePath, viewControllerFileName);
            if (File.Exists(filePath))
            {
                string classFile = File.ReadAllText(filePath);
                string newClassFile = classFile.Replace(targetString, "\t//NSAssert(UnityShouldAutorotate()");
                if (classFile.Length != newClassFile.Length)
                {
                    File.WriteAllText(filePath, newClassFile);
                }
            }
        }
    }
}
