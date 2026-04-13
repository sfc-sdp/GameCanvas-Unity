namespace GenApiDocs;

/// <summary>旧 DocFX の filter.yml を踏襲した公開 API フィルタ</summary>
public static class FilterRule
{
    static readonly string[] ExcludedNamespaces =
    {
        "GameCanvas.Editor",
        "GameCanvas.Tests",
    };

    static readonly HashSet<string> ExcludedTypes = new()
    {
        "GameCanvas.BehaviourBase",
        "GameCanvas.Resource",
    };

    // GcFont/GcImage/GcSound/GcText の Field は自動生成の定数で量が多いため除外
    static readonly HashSet<string> FieldSuppressedTypes = new()
    {
        "GameCanvas.GcFont",
        "GameCanvas.GcImage",
        "GameCanvas.GcSound",
        "GameCanvas.GcText",
    };

    public static bool ShouldInclude(ApiType t)
    {
        if (ExcludedNamespaces.Any(ns => t.Namespace == ns || t.Namespace.StartsWith(ns + ".")))
            return false;
        if (ExcludedTypes.Contains(t.FullNameSimple)) return false;
        if (t.FullNameSimple.StartsWith("GameCanvas.Resource.")) return false;
        return true;
    }

    public static bool ShouldIncludeMember(ApiType t, ApiMember m)
    {
        if (m.Kind == MemberKind.Field && FieldSuppressedTypes.Contains(t.FullNameSimple))
            return false;
        return true;
    }
}
