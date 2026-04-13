using System;
using System.IO;
using UnityEngine;

namespace UnityBridge.Tools
{
    internal static class CaptureHelpers
    {
        public static void ValidateFormat(string format)
        {
            if (format != "png" && format != "jpg")
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Unknown format: {format}. Valid formats: png, jpg");
            }
        }

        public static Camera FindCamera(string cameraName)
        {
            Camera camera = null;
            if (!string.IsNullOrEmpty(cameraName))
            {
                var cameraGo = GameObject.Find(cameraName);
                if (cameraGo != null)
                {
                    camera = cameraGo.GetComponent<Camera>();
                }
            }

            camera ??= Camera.main;

            if (camera == null)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "No camera found. Specify camera name or ensure a Main Camera exists.");
            }

            return camera;
        }

        public static string ResolveOutputDir(string outputDir, string prefix)
        {
            if (string.IsNullOrEmpty(outputDir))
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var projectPath = Directory.GetCurrentDirectory();
                outputDir = Path.Combine(projectPath, $"Screenshots/{prefix}_{timestamp}");
            }

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            return outputDir;
        }

        public static string ResolveOutputPath(string path, string prefix, string ext)
        {
            if (string.IsNullOrEmpty(path))
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var projectPath = Directory.GetCurrentDirectory();
                path = Path.Combine(projectPath, $"Screenshots/{prefix}_{timestamp}{ext}");
            }

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return path;
        }

        public static void ClampDimensions(ref int width, ref int height)
        {
            width = Mathf.Clamp(width, 1, ToolConstants.MaxCaptureWidth);
            height = Mathf.Clamp(height, 1, ToolConstants.MaxCaptureHeight);
        }

        public static string GetExtension(string format)
        {
            return format == "jpg" ? ".jpg" : ".png";
        }
    }
}
