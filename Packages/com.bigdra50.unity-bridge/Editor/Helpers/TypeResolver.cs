using System;
using System.Linq;
using System.Reflection;

namespace UnityBridge.Helpers
{
    /// <summary>
    /// Finds a Type by full name or short name across all loaded assemblies.
    /// Consolidates duplicate FindType implementations from Asset.cs and Component.cs.
    /// </summary>
    internal static class TypeResolver
    {
        /// <summary>
        /// Resolve a type by name. Tries in order:
        /// 1. Type.GetType (assembly-qualified or mscorlib/executing assembly)
        /// 2. Assembly.GetType for each loaded assembly (full name match)
        /// 3. Short name match (Type.Name or Type.FullName)
        /// </summary>
        public static Type FindType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            // Try Type.GetType first (handles assembly-qualified names)
            var type = Type.GetType(typeName);
            if (type != null) return type;

            // Search all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // Try exact full name match
                    type = assembly.GetType(typeName);
                    if (type != null) return type;

                    // Try short name match
                    type = assembly.GetTypes().FirstOrDefault(t =>
                        t.Name == typeName || t.FullName == typeName);
                    if (type != null) return type;
                }
                catch (ReflectionTypeLoadException ex)
                {
                    BridgeLog.Warn(
                        $"Failed to load types from assembly '{assembly.GetName().Name}': {ex.Message}");
                }
            }

            return null;
        }
    }
}
