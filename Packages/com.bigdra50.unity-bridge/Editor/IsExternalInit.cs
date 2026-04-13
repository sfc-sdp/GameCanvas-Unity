// Polyfill for C# 9 record types in Unity (requires .NET Standard 2.1 or .NET 5+)
// This enables 'record' and 'init' keywords in older runtimes.

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
