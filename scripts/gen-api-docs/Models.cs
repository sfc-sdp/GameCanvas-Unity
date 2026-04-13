namespace GenApiDocs;

public enum ApiKind { Class, Struct, Interface, Enum, Delegate }

public enum MemberKind { Field, Property, Indexer, Method, Constructor, Event, Operator, EnumValue }

public sealed class ApiType
{
    public required string Namespace { get; init; }
    public required string Name { get; init; }              // 表示名 (例: "DictWithLife<TKey, TValue>")
    public required string SimpleName { get; init; }        // "DictWithLife"
    public required string FileName { get; init; }          // "GameCanvas.DictWithLife-2"
    public required ApiKind Kind { get; init; }
    public required string Modifiers { get; init; }         // "public sealed", "public abstract" など
    public required string DeclarationSyntax { get; init; } // "public sealed class Foo : BaseFoo, IFoo"
    public List<string> BaseTypes { get; } = new();
    public XmlDoc Doc { get; set; } = new();
    public List<ApiMember> Members { get; } = new();
    public string FullName => $"{Namespace}.{Name}";
    public string FullNameSimple => $"{Namespace}.{SimpleName}";
}

public sealed class ApiMember
{
    public required MemberKind Kind { get; init; }
    /// <summary>素の識別子 (例: "DrawCircle")。オーバーロードで衝突しうる</summary>
    public required string Name { get; init; }
    /// <summary>
    /// オーバーロード込みで一意なメンバーID。
    /// 形式: "{Kind}.{Name}{ParamTypes}" 例 "Method.DrawCircle(float,float)" / "Property.BackgroundColor"
    /// inheritdoc / cref 解決・見出しアンカー・partial マージで使う。
    /// </summary>
    public required string Id { get; init; }
    /// <summary>見出し表示名。メソッドは引数型付き "DrawCircle(float, float)"、それ以外は Name と同じ</summary>
    public required string DisplayName { get; init; }
    public required string Signature { get; init; }         // "public int Foo(int x, string y)"
    public XmlDoc Doc { get; set; } = new();
    public string? InheritdocCref { get; set; }             // <inheritdoc cref="..."/> 指定
    public bool Inheritdoc { get; set; }                    // <inheritdoc/> のみ (cref 無し)
    /// <summary>表示順用。Kind → DisplayName で並べる</summary>
    public string SortKey => DisplayName;
}

public sealed class XmlDoc
{
    public string? Summary { get; set; }
    public string? Remarks { get; set; }
    public string? Returns { get; set; }
    public string? Value { get; set; }
    public List<(string Name, string Text)> Params { get; } = new();
    public List<(string Name, string Text)> TypeParams { get; } = new();
    public List<(string Cref, string Text)> Exceptions { get; } = new();
    public List<string> Examples { get; } = new();
    public List<string> SeeAlso { get; } = new();
    public bool IsEmpty => Summary == null && Remarks == null && Returns == null && Value == null
        && Params.Count == 0 && TypeParams.Count == 0 && Exceptions.Count == 0
        && Examples.Count == 0 && SeeAlso.Count == 0;
}
