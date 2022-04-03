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
using UnityEditor;

namespace GameCanvas.Editor
{
    public static class GcRuntimePlatformExtensions
    {
        public static BuildTarget ToBuildTarget(this GcRuntimePlatform self)
        {
            return self switch
            {
                GcRuntimePlatform.Android => BuildTarget.Android,
                GcRuntimePlatform.iOS => BuildTarget.iOS,
                GcRuntimePlatform.WebGL => BuildTarget.WebGL,
                _ => throw new System.NotSupportedException()
            };
        }

        public static BuildTargetGroup ToBuildTargetGroup(this GcRuntimePlatform self)
            => BuildPipeline.GetBuildTargetGroup(self.ToBuildTarget());

        public static GcRuntimePlatform ToRuntimePlatform(this BuildTarget self)
        {
            return self switch
            {
                BuildTarget.Android => GcRuntimePlatform.Android,
                BuildTarget.iOS => GcRuntimePlatform.iOS,
                BuildTarget.WebGL => GcRuntimePlatform.WebGL,
                _ => throw new System.NotSupportedException()
            };
        }
    }
}
