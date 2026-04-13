using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UnityBridge.Helpers
{
    internal static class ApiSafetyGuard
    {
        private static readonly string[] AllowedNamespacePrefixes = { "UnityEngine", "UnityEditor" };

        public static bool IsNamespaceAllowed(Type type)
        {
            var fullName = type.FullName;
            if (string.IsNullOrEmpty(fullName)) return false;
            return AllowedNamespacePrefixes.Any(prefix => fullName.StartsWith(prefix, StringComparison.Ordinal));
        }

        public static bool IsObsolete(MemberInfo member)
        {
            return member.GetCustomAttribute<ObsoleteAttribute>() != null;
        }

        public static bool IsSupportedParam(ParameterInfo param)
        {
            if (param.IsOut || param.ParameterType.IsByRef || param.ParameterType.IsPointer)
                return false;

            return IsSupportedType(param.ParameterType);
        }

        public static bool HasAllSupportedParams(MethodInfo method)
        {
            return method.GetParameters().All(IsSupportedParam);
        }

        public static object DeserializeArg(JToken token, Type targetType)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                    throw new ArgumentException($"Cannot pass null to value type {targetType.Name}");
                return null;
            }

            // Primitives
            if (targetType == typeof(int)) return token.Value<int>();
            if (targetType == typeof(long)) return token.Value<long>();
            if (targetType == typeof(float)) return token.Value<float>();
            if (targetType == typeof(double)) return token.Value<double>();
            if (targetType == typeof(bool)) return token.Value<bool>();
            if (targetType == typeof(decimal)) return token.Value<decimal>();
            if (targetType == typeof(string)) return token.Value<string>();

            // Enum: accept name (string) or integer value
            if (targetType.IsEnum)
            {
                if (token.Type == JTokenType.Integer)
                    return Enum.ToObject(targetType, token.Value<int>());
                if (token.Type == JTokenType.String)
                {
                    var name = token.Value<string>();
                    if (Enum.TryParse(targetType, name, true, out var result))
                        return result;
                    var validNames = string.Join(", ", Enum.GetNames(targetType));
                    throw new ArgumentException(
                        $"Invalid enum value '{name}' for {targetType.Name}. Valid: {validNames}");
                }
            }

            // Primitive arrays
            if (targetType == typeof(int[])) return token.ToObject<int[]>();
            if (targetType == typeof(float[])) return token.ToObject<float[]>();
            if (targetType == typeof(double[])) return token.ToObject<double[]>();
            if (targetType == typeof(bool[])) return token.ToObject<bool[]>();
            if (targetType == typeof(string[])) return token.ToObject<string[]>();

            throw new ArgumentException($"Unsupported parameter type: {targetType.FullName}");
        }

        public static object[] DeserializeArgs(MethodInfo method, JArray args)
        {
            var parameters = method.GetParameters();
            var result = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                if (args != null && i < args.Count)
                {
                    result[i] = DeserializeArg(args[i], parameters[i].ParameterType);
                }
                else if (parameters[i].HasDefaultValue)
                {
                    result[i] = parameters[i].DefaultValue;
                }
                else
                {
                    throw new ArgumentException(
                        $"Missing required parameter '{parameters[i].Name}' ({parameters[i].ParameterType.Name})");
                }
            }

            return result;
        }

        public static JToken SerializeReturnValue(object value, Type type)
        {
            if (value == null) return JValue.CreateNull();

            if (type == typeof(void)) return JValue.CreateNull();

            // Primitives & string
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
                return JToken.FromObject(value);

            // Enum
            if (type.IsEnum) return value.ToString();

            // Unity value types
            if (type == typeof(Vector2))
            {
                var v = (Vector2)value;
                return new JObject { ["x"] = v.x, ["y"] = v.y };
            }
            if (type == typeof(Vector3))
            {
                var v = (Vector3)value;
                return new JObject { ["x"] = v.x, ["y"] = v.y, ["z"] = v.z };
            }
            if (type == typeof(Vector4))
            {
                var v = (Vector4)value;
                return new JObject { ["x"] = v.x, ["y"] = v.y, ["z"] = v.z, ["w"] = v.w };
            }
            if (type == typeof(Quaternion))
            {
                var q = (Quaternion)value;
                var e = q.eulerAngles;
                return new JObject { ["x"] = e.x, ["y"] = e.y, ["z"] = e.z };
            }
            if (type == typeof(Color))
            {
                var c = (Color)value;
                return new JObject { ["r"] = c.r, ["g"] = c.g, ["b"] = c.b, ["a"] = c.a };
            }
            if (type == typeof(Rect))
            {
                var r = (Rect)value;
                return new JObject { ["x"] = r.x, ["y"] = r.y, ["width"] = r.width, ["height"] = r.height };
            }
            if (type == typeof(Bounds))
            {
                var b = (Bounds)value;
                return new JObject
                {
                    ["center"] = new JObject { ["x"] = b.center.x, ["y"] = b.center.y, ["z"] = b.center.z },
                    ["size"] = new JObject { ["x"] = b.size.x, ["y"] = b.size.y, ["z"] = b.size.z }
                };
            }

            // UnityEngine.Object references
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                var obj = value as UnityEngine.Object;
                if (obj == null) return JValue.CreateNull();
                return new JObject
                {
                    ["name"] = obj.name,
                    ["instanceID"] = obj.GetInstanceID(),
                    ["type"] = obj.GetType().Name
                };
            }

            // Primitive arrays
            if (type.IsArray && type.GetElementType() is { } elem && (elem.IsPrimitive || elem == typeof(string)))
                return JArray.FromObject(value);

            // Fallback
            return new JObject
            {
                ["_type"] = type.Name,
                ["_toString"] = value.ToString()
            };
        }

        public static string FormatSignature(MethodInfo method)
        {
            var paramStrs = method.GetParameters()
                .Select(p => $"{p.ParameterType.Name} {p.Name}{(p.HasDefaultValue ? " = " + (p.DefaultValue?.ToString() ?? "null") : "")}");
            return $"{method.ReturnType.Name} {method.DeclaringType?.FullName}.{method.Name}({string.Join(", ", paramStrs)})";
        }

        private static bool IsSupportedType(Type type)
        {
            if (type.IsPrimitive) return true;
            if (type == typeof(string)) return true;
            if (type == typeof(decimal)) return true;
            if (type.IsEnum) return true;

            // Primitive/string/decimal arrays
            if (type.IsArray)
            {
                var elem = type.GetElementType();
                return elem != null && (elem.IsPrimitive || elem == typeof(string) || elem == typeof(decimal));
            }

            return false;
        }
    }
}
