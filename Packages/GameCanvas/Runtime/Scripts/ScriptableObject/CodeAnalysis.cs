#nullable enable

namespace System.Diagnostics.CodeAnalysis
{
    sealed class NotNullWhenAttribute : Attribute
    {
        public bool ReturnValue { get; }

        public NotNullWhenAttribute(bool returnValue)
            => ReturnValue = returnValue;
    }

    sealed class NotNullAttribute : Attribute
    {
    }
}
