#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCanvas.Engine
{
    internal static class GcAndroidPermission
    {
        // フォーカスの復帰だけに依存しない。結果が届かないOS経路も期限で終える。
        internal static IEnumerator Request(string[] permissions, System.Action<bool>? timedOut = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var pending = new HashSet<string>();
            foreach (var permission in permissions)
                if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(permission)) pending.Add(permission);
            if (pending.Count == 0) yield break;
            var callbacks = new UnityEngine.Android.PermissionCallbacks();
            void Complete(string permission) => pending.Remove(permission);
            callbacks.PermissionGranted += Complete;
            callbacks.PermissionDenied += Complete;
            callbacks.PermissionRequestDismissed += Complete;
            try
            {
                UnityEngine.Android.Permission.RequestUserPermissions(permissions, callbacks);
                var deadline = Time.realtimeSinceStartupAsDouble + 30;
                while (pending.Count > 0 && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                timedOut?.Invoke(pending.Count > 0);
            }
            finally
            {
                callbacks.PermissionGranted -= Complete;
                callbacks.PermissionDenied -= Complete;
                callbacks.PermissionRequestDismissed -= Complete;
            }
#else
            yield break;
#endif
        }
    }
}
