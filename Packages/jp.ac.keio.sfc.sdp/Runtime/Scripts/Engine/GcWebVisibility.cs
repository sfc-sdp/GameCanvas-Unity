#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameCanvas
{
    internal static class GcWebVisibility
    {
        private delegate void VisibilityCallback(int owner, int hidden);
        private static readonly VisibilityCallback callback = OnVisibility;
        private static readonly Dictionary<int, Action<bool>> listeners = new();
        private static int nextOwner;

        [DllImport("__Internal")]
        private static extern int GcVisibilityRegister(int owner, VisibilityCallback callback);
        [DllImport("__Internal")]
        private static extern void GcVisibilityUnregister(int owner);

        internal static int Register(Action<bool> listener, out bool hidden)
        {
            int owner = ++nextOwner;
            listeners[owner] = listener;
            hidden = GcVisibilityRegister(owner, callback) != 0;
            return owner;
        }

        internal static void Unregister(int owner)
        {
            GcVisibilityUnregister(owner);
            listeners.Remove(owner);
        }

        [AOT.MonoPInvokeCallback(typeof(VisibilityCallback))]
        private static void OnVisibility(int owner, int hidden)
        {
            if (!listeners.TryGetValue(owner, out var listener)) return;
            try { listener(hidden != 0); }
            catch (Exception exception) { UnityEngine.Debug.LogException(exception); }
        }
    }
}
#endif
