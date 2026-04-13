using System;
using System.Reflection;
using UnityBridge.Helpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityBridge
{
    internal static class BridgeToolbarHelper
    {
        internal static readonly Color DisconnectedColor = new(0.5f, 0.5f, 0.5f);
        internal static readonly Color ConnectingColor = new(0.9f, 0.7f, 0.2f);
        internal static readonly Color ConnectedColor = new(0.3f, 0.8f, 0.3f);

        private static VisualElement _button;
        private static ConnectionStatus _lastStatus = ConnectionStatus.Disconnected;

        internal static VisualElement CreateButton()
        {
            var button = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingLeft = 4,
                    paddingRight = 4,
                    marginLeft = 2,
                    marginRight = 2
                }
            };
            button.AddToClassList("unity-toolbar-button");

            var indicator = new VisualElement
            {
                name = "bridge-indicator",
                style =
                {
                    width = 8,
                    height = 8,
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    marginRight = 4,
                    backgroundColor = DisconnectedColor
                }
            };

            var label = new Label("Bridge")
            {
                name = "bridge-label",
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    fontSize = 11
                }
            };

            button.Add(indicator);
            button.Add(label);

            button.RegisterCallback<MouseEnterEvent>(_ =>
                button.style.backgroundColor = new Color(1f, 1f, 1f, 0.08f));
            button.RegisterCallback<MouseLeaveEvent>(_ =>
                button.style.backgroundColor = StyleKeyword.Null);

            return button;
        }

        internal static void Register(VisualElement button)
        {
            _button = button;
            _lastStatus = GetCurrentStatus();
            ApplyButtonState(_button, _lastStatus);
            EditorApplication.update += PollStatus;
        }

        private static void PollStatus()
        {
            if (_button?.panel == null)
            {
                EditorApplication.update -= PollStatus;
                _button = null;
                return;
            }

            var current = GetCurrentStatus();
            if (current == _lastStatus) return;

            _lastStatus = current;
            ApplyButtonState(_button, current);
        }

        private static ConnectionStatus GetCurrentStatus()
        {
            return BridgeManager.Instance.Client?.Status ?? ConnectionStatus.Disconnected;
        }

        private static void ApplyButtonState(VisualElement button, ConnectionStatus status)
        {
            if (button?.panel == null) return;

            var indicator = button.Q("bridge-indicator");
            if (indicator == null) return;

            var (color, tooltip, enabled) = status switch
            {
                ConnectionStatus.Disconnected => (
                    DisconnectedColor,
                    "Unity Bridge: Disconnected\nClick to connect",
                    true),
                ConnectionStatus.Connecting => (
                    ConnectingColor,
                    "Unity Bridge: Connecting...",
                    false),
                ConnectionStatus.Connected => (
                    ConnectedColor,
                    $"Unity Bridge: Connected ({BridgeManager.Instance.Host}:{BridgeManager.Instance.Port})\nClick to disconnect",
                    true),
                ConnectionStatus.Reloading => (
                    ConnectingColor,
                    "Unity Bridge: Reloading...",
                    false),
                _ => (DisconnectedColor, "Unity Bridge", true)
            };

            indicator.style.backgroundColor = color;
            button.tooltip = tooltip;
            button.SetEnabled(enabled);
        }

        internal static async void ToggleConnection()
        {
            var manager = BridgeManager.Instance;
            try
            {
                if (manager.IsConnected)
                {
                    await manager.DisconnectAsync();
                }
                else
                {
                    await manager.ConnectAsync(manager.Host, manager.Port);
                }
            }
            catch (ObjectDisposedException)
            {
                // Already disconnected
            }
            catch (Exception ex)
            {
                BridgeLog.Warn($"Toolbar toggle connection error: {ex.Message}");
            }
        }
    }

    [InitializeOnLoad]
    internal static class BridgeToolbarInjector
    {
        private static readonly FieldInfo GetField;
        private static readonly PropertyInfo VisualTreeProp;

        static BridgeToolbarInjector()
        {
            var editorAssembly = typeof(Editor).Assembly;
            var sToolbarType = editorAssembly.GetType("UnityEditor.Toolbar");
            var guiViewType = editorAssembly.GetType("UnityEditor.GUIView");

            // Unity 6000.3+ changed "get" field - try multiple approaches
            GetField = sToolbarType?.GetField("get", BindingFlags.Public | BindingFlags.Static)
                       ?? sToolbarType?.GetField("get", BindingFlags.NonPublic | BindingFlags.Static);

            VisualTreeProp = guiViewType?.GetProperty("visualTree",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (sToolbarType == null || GetField == null || VisualTreeProp == null)
            {
                BridgeLog.Warn("Toolbar injection: required types/members not found");
                return;
            }

            EditorApplication.update += WaitForToolbar;
        }

        private static void WaitForToolbar()
        {
            var toolbar = GetField.GetValue(null);
            if (toolbar == null) return;

            EditorApplication.update -= WaitForToolbar;

            if (VisualTreeProp?.GetValue(toolbar) is not VisualElement root) return;

            // Unity 6000.3+: find the main toolbar container
            var overlayContainer = root.Query<VisualElement>()
                .Where(e => e.GetType().Name == "MainToolbarOverlayContainer").First();

            // Find the right-most ContainerSection (after Account/Services)
            VisualElement rightZone = null;
            if (overlayContainer != null)
            {
                var sections = overlayContainer.Query<VisualElement>()
                    .Where(e => e.GetType().Name == "ContainerSection").ToList();
                rightZone = sections.Count > 0 ? sections[^1] : null;
            }

            // Fallback for older Unity versions
            rightZone ??= root.Q("ToolbarZoneRightAlign");

            if (rightZone == null)
            {
                BridgeLog.Warn("Toolbar injection: right zone not found");
                return;
            }

            var button = BridgeToolbarHelper.CreateButton();
            button.RegisterCallback<ClickEvent>(_ => BridgeToolbarHelper.ToggleConnection());
            rightZone.Insert(0, button);
            BridgeToolbarHelper.Register(button);
        }
    }
}
