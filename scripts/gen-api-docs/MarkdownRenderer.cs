using System.Text;
using System.Text.RegularExpressions;

namespace GenApiDocs;

public static class MarkdownRenderer
{
    static readonly Regex SeeMarker = new(@"\{@see:([^}]+)\}", RegexOptions.Compiled);

    public static string RenderType(ApiType t, IReadOnlyDictionary<string, ApiType> byFullSimple,
        IReadOnlyDictionary<string, ApiType> bySimpleName)
    {
        var sb = new StringBuilder();

        // frontmatter (ジェネリック型引数の `<` が YAML / Vue で HTML tag と誤認されるため quote)
        sb.AppendLine("---");
        sb.AppendLine($"title: {YamlStr(t.Name)}");
        sb.AppendLine($"outline: [2, 3]");
        sb.AppendLine("---");
        sb.AppendLine();

        // 見出し + シグネチャ
        sb.AppendLine($"# {KindLabel(t.Kind)} {EscapeHeading(t.Name)}");
        sb.AppendLine();
        sb.AppendLine($"**Namespace:** `{t.Namespace}`");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine(t.DeclarationSyntax);
        sb.AppendLine("```");
        sb.AppendLine();

        RenderDocBody(sb, t.Doc, byFullSimple, bySimpleName);

        // 継承元一覧 (IGameCanvas のように多数の interface を束ねる型向け)
        if (t.BaseTypes.Count > 0)
        {
            sb.AppendLine("**継承元**");
            sb.AppendLine();
            foreach (var bn in t.BaseTypes)
                sb.AppendLine($"- {FormatTypeRef(bn, byFullSimple, bySimpleName)}");
            sb.AppendLine();
        }

        // 直接メンバー + 継承メンバーをまとめてカテゴリ別に表示
        var all = t.Members.Select(m => (Member: m, Origin: (ApiType?)null))
            .Concat(t.InheritedMembers.Select(im => (im.Member, (ApiType?)im.Origin)))
            .ToArray();

        var groups = new (string Title, MemberKind[] Kinds)[]
        {
            ("列挙値", new[] { MemberKind.EnumValue }),
            ("コンストラクター", new[] { MemberKind.Constructor }),
            ("フィールド", new[] { MemberKind.Field }),
            ("プロパティ", new[] { MemberKind.Property, MemberKind.Indexer }),
            ("メソッド", new[] { MemberKind.Method }),
            ("演算子", new[] { MemberKind.Operator }),
            ("イベント", new[] { MemberKind.Event }),
        };

        foreach (var (title, kinds) in groups)
        {
            var ms = all.Where(x => kinds.Contains(x.Member.Kind))
                .OrderBy(x => x.Member.SortKey).ToArray();
            if (ms.Length == 0) continue;
            sb.AppendLine($"## {title}");
            sb.AppendLine();
            foreach (var (m, origin) in ms)
                RenderMember(sb, m, origin, byFullSimple, bySimpleName);
        }

        return sb.ToString();
    }

    static string FormatTypeRef(string typeRef,
        IReadOnlyDictionary<string, ApiType> byFullSimple,
        IReadOnlyDictionary<string, ApiType> bySimpleName)
    {
        var simple = typeRef;
        var lt = simple.IndexOf('<');
        if (lt >= 0) simple = simple[..lt];
        simple = simple.Split('.').Last();
        if (bySimpleName.TryGetValue(simple, out var t))
            return $"[`{typeRef}`](./{t.FileName}.md)";
        return $"`{typeRef}`";
    }

    public static string RenderIndex(ApiType[] types,
        IReadOnlyDictionary<string, ApiType> byFullSimple,
        IReadOnlyDictionary<string, ApiType> bySimpleName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.AppendLine("title: API リファレンス");
        sb.AppendLine("outline: [2, 3]");
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("# API リファレンス");
        sb.AppendLine();
        sb.AppendLine("GameCanvas の公開 API 一覧です。`Obsolete` とマークされた API および内部実装用の型は除外しています。");
        sb.AppendLine();

        var byNs = types.GroupBy(t => t.Namespace).OrderBy(g => g.Key);
        foreach (var g in byNs)
        {
            sb.AppendLine($"## {g.Key}");
            sb.AppendLine();
            var byKind = g.GroupBy(t => t.Kind).OrderBy(x => (int)x.Key);
            foreach (var kg in byKind)
            {
                sb.AppendLine($"### {KindLabelPlural(kg.Key)}");
                sb.AppendLine();
                foreach (var t in kg.OrderBy(x => x.SimpleName))
                {
                    var summary = FirstLine(t.Doc.Summary);
                    var resolved = string.IsNullOrEmpty(summary) ? "" : ResolveSee(summary, byFullSimple, bySimpleName);
                    sb.AppendLine($"- [`{t.Name}`](./{t.FileName}.md){(string.IsNullOrEmpty(resolved) ? "" : $" — {resolved}")}");
                }
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }

    public static string RenderToc(ApiType[] types)
    {
        // VitePress config.mts から import 可能な JSON
        var sb = new StringBuilder();
        sb.AppendLine("[");
        var byNs = types.GroupBy(t => t.Namespace).OrderBy(g => g.Key).ToArray();
        for (int i = 0; i < byNs.Length; i++)
        {
            var g = byNs[i];
            sb.AppendLine("  {");
            sb.AppendLine($"    \"text\": {JsonStr(g.Key)},");
            sb.AppendLine("    \"collapsed\": true,");
            sb.AppendLine("    \"items\": [");
            var items = g.OrderBy(t => t.SimpleName).ToArray();
            for (int j = 0; j < items.Length; j++)
            {
                var t = items[j];
                sb.Append($"      {{ \"text\": {JsonStr(t.Name)}, \"link\": \"/api/{t.FileName}\" }}");
                sb.AppendLine(j == items.Length - 1 ? "" : ",");
            }
            sb.AppendLine("    ]");
            sb.Append("  }");
            sb.AppendLine(i == byNs.Length - 1 ? "" : ",");
        }
        sb.AppendLine("]");
        return sb.ToString();
    }

    static void RenderMember(StringBuilder sb, ApiMember m, ApiType? origin,
        IReadOnlyDictionary<string, ApiType> byFullSimple,
        IReadOnlyDictionary<string, ApiType> bySimpleName)
    {
        // オーバーロード同士でアンカーが衝突しないように DisplayName (引数型付き) で見出しを立てる
        sb.AppendLine($"### {EscapeHeading(m.DisplayName)}");
        sb.AppendLine();
        if (origin != null)
        {
            sb.AppendLine($"*継承元: [`{origin.Name}`](./{origin.FileName}.md)*");
            sb.AppendLine();
        }
        sb.AppendLine("```csharp");
        sb.AppendLine(m.Signature);
        sb.AppendLine("```");
        sb.AppendLine();
        RenderDocBody(sb, m.Doc, byFullSimple, bySimpleName);
    }

    static void RenderDocBody(StringBuilder sb, XmlDoc doc,
        IReadOnlyDictionary<string, ApiType> byFullSimple,
        IReadOnlyDictionary<string, ApiType> bySimpleName)
    {
        if (doc.Summary != null)
        {
            sb.AppendLine(ResolveSee(doc.Summary, byFullSimple, bySimpleName));
            sb.AppendLine();
        }
        if (doc.Remarks != null)
        {
            sb.AppendLine("**備考**");
            sb.AppendLine();
            sb.AppendLine(ResolveSee(doc.Remarks, byFullSimple, bySimpleName));
            sb.AppendLine();
        }
        if (doc.Value != null)
        {
            sb.AppendLine("**値**");
            sb.AppendLine();
            sb.AppendLine(ResolveSee(doc.Value, byFullSimple, bySimpleName));
            sb.AppendLine();
        }
        if (doc.TypeParams.Count > 0)
        {
            sb.AppendLine("**型パラメーター**");
            sb.AppendLine();
            foreach (var (name, text) in doc.TypeParams)
                sb.AppendLine($"- `{name}` — {ResolveSee(text, byFullSimple, bySimpleName)}");
            sb.AppendLine();
        }
        if (doc.Params.Count > 0)
        {
            sb.AppendLine("**パラメーター**");
            sb.AppendLine();
            foreach (var (name, text) in doc.Params)
                sb.AppendLine($"- `{name}` — {ResolveSee(text, byFullSimple, bySimpleName)}");
            sb.AppendLine();
        }
        if (doc.Returns != null)
        {
            sb.AppendLine("**戻り値**");
            sb.AppendLine();
            sb.AppendLine(ResolveSee(doc.Returns, byFullSimple, bySimpleName));
            sb.AppendLine();
        }
        if (doc.Exceptions.Count > 0)
        {
            sb.AppendLine("**例外**");
            sb.AppendLine();
            foreach (var (cref, text) in doc.Exceptions)
                sb.AppendLine($"- `{cref}` — {ResolveSee(text, byFullSimple, bySimpleName)}");
            sb.AppendLine();
        }
        if (doc.Examples.Count > 0)
        {
            sb.AppendLine("**例**");
            sb.AppendLine();
            foreach (var ex in doc.Examples)
            {
                sb.AppendLine(ResolveSee(ex, byFullSimple, bySimpleName));
                sb.AppendLine();
            }
        }
        if (doc.SeeAlso.Count > 0)
        {
            sb.AppendLine("**関連項目**");
            sb.AppendLine();
            foreach (var c in doc.SeeAlso)
                sb.AppendLine($"- {ResolveSee("{@see:" + c + "}", byFullSimple, bySimpleName)}");
            sb.AppendLine();
        }
    }

    static string ResolveSee(string text,
        IReadOnlyDictionary<string, ApiType> byFullSimple,
        IReadOnlyDictionary<string, ApiType> bySimpleName)
    {
        return SeeMarker.Replace(text, match =>
        {
            var cref = match.Groups[1].Value;

            // 1. 引数部分を除いた文字列を取り、まず型として解決を試みる
            var paren = cref.IndexOf('(');
            var stripped = paren >= 0 ? cref[..paren] : cref;
            if (TryResolve(stripped, byFullSimple, bySimpleName, out var typeOnly, out var typeFile))
                return $"[`{typeOnly!.Name}`](./{typeFile}.md)";

            // 2. 最後のドットで分割し、型 + メンバーとして解決
            var lastDot = stripped.LastIndexOf('.');
            if (lastDot > 0)
            {
                var typeName = stripped[..lastDot];
                var memberName = stripped[(lastDot + 1)..];
                if (TryResolve(typeName, byFullSimple, bySimpleName, out var target, out var filename))
                {
                    var anchor = ResolveMemberAnchor(target!, memberName);
                    return $"[`{target!.SimpleName}.{memberName}`](./{filename}.md#{anchor})";
                }
            }

            // 3. 解決できないときは inline code
            return $"`{cref}`";
        });
    }

    static string ResolveMemberAnchor(ApiType type, string memberName)
    {
        // オーバーロードが複数ある場合: 最初のマッチ (VitePress 流儀では後続は -1, -2 サフィックスだが cref からは区別不可)
        var m = type.Members.FirstOrDefault(x => x.Name == memberName || x.DisplayName.StartsWith(memberName + "("));
        // markdown-it-anchor の slugify は HTML エスケープ後の見出しテキストを入力とするため、
        // C# 側でも EscapeHeading を通してから slugify する (`<T>` → `&lt;T&gt;` → `lt-t-gt`)
        var display = m?.DisplayName ?? memberName;
        return SlugifyHeading(EscapeHeading(display));
    }

    static bool TryResolve(string typeName,
        IReadOnlyDictionary<string, ApiType> byFullSimple,
        IReadOnlyDictionary<string, ApiType> bySimpleName,
        out ApiType? target, out string? filename)
    {
        if (byFullSimple.TryGetValue(typeName, out var t1))
        {
            target = t1; filename = t1.FileName; return true;
        }
        var simple = typeName.Split('.').Last();
        if (bySimpleName.TryGetValue(simple, out var t2))
        {
            target = t2; filename = t2.FileName; return true;
        }
        target = null; filename = null; return false;
    }

    /// <summary>
    /// VitePress (markdown-it-anchor) に渡す独自 slugify と同じロジック。
    /// config.mts 側の <c>markdown.anchor.slugify</c> にも同じ実装を入れているので双方の出力が一致する:
    ///   1) 小文字化
    ///   2) 英数・_・-・非 ASCII (日本語等) は残す、それ以外は <c>-</c> に置換
    ///   3) 連続する <c>-</c> を 1 つに、前後の <c>-</c> を削除
    /// 例: "DrawCircle(float, float)" → "drawcircle-float-float"
    ///     "UnregisterScene&lt;T&gt;()" → "unregisterscene-t"
    /// </summary>
    static string SlugifyHeading(string heading)
    {
        var lower = heading.ToLowerInvariant();
        var sb = new StringBuilder();
        foreach (var c in lower)
        {
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c > 0x7F)
                sb.Append(c);
            else
                sb.Append('-');
        }
        var s = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), @"-+", "-");
        return s.Trim('-');
    }

    static string KindLabel(ApiKind k) => k switch
    {
        ApiKind.Class => "クラス",
        ApiKind.Struct => "構造体",
        ApiKind.Interface => "インターフェイス",
        ApiKind.Enum => "列挙型",
        ApiKind.Delegate => "デリゲート",
        _ => "",
    };

    static string KindLabelPlural(ApiKind k) => k switch
    {
        ApiKind.Class => "クラス",
        ApiKind.Struct => "構造体",
        ApiKind.Interface => "インターフェイス",
        ApiKind.Enum => "列挙型",
        ApiKind.Delegate => "デリゲート",
        _ => "",
    };

    static string EscapeHeading(string s) => s.Replace("<", "&lt;").Replace(">", "&gt;");

    static string YamlStr(string s) => $"'{s.Replace("'", "''")}'";

    static string FirstLine(string? s)
    {
        if (s == null) return "";
        var nl = s.IndexOf('\n');
        return nl >= 0 ? s[..nl].Trim() : s.Trim();
    }

    static string JsonStr(string s)
    {
        var escaped = s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        return $"\"{escaped}\"";
    }
}
