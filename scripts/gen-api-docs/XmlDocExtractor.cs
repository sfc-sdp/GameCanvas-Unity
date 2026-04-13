using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GenApiDocs;

public static class XmlDocExtractor
{
    public static XmlDoc Extract(SyntaxNode node)
    {
        var (doc, _, _) = ExtractWithInheritdoc(node);
        return doc;
    }

    public static (XmlDoc Doc, string? InheritdocCref, bool Inheritdoc) ExtractWithInheritdoc(SyntaxNode node)
    {
        var doc = new XmlDoc();
        string? inheritCref = null;
        bool inherit = false;

        var comment = node.GetLeadingTrivia()
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();
        if (comment == null) return (doc, null, false);

        foreach (var child in comment.Content)
        {
            switch (child)
            {
                case XmlElementSyntax el:
                    HandleElement(el, doc, ref inherit, ref inheritCref);
                    break;
                case XmlEmptyElementSyntax empty:
                    HandleEmptyElement(empty, doc, ref inherit, ref inheritCref);
                    break;
            }
        }
        return (doc, inheritCref, inherit);
    }

    static void HandleElement(XmlElementSyntax el, XmlDoc doc, ref bool inherit, ref string? inheritCref)
    {
        var name = el.StartTag.Name.LocalName.ValueText;
        var content = RenderContent(el.Content);
        switch (name)
        {
            case "summary": doc.Summary = content; break;
            case "remarks": doc.Remarks = content; break;
            case "returns": doc.Returns = content; break;
            case "value": doc.Value = content; break;
            case "example": doc.Examples.Add(content); break;
            case "param":
                doc.Params.Add((GetAttr(el.StartTag.Attributes, "name") ?? "", content));
                break;
            case "typeparam":
                doc.TypeParams.Add((GetAttr(el.StartTag.Attributes, "name") ?? "", content));
                break;
            case "exception":
                doc.Exceptions.Add((GetAttr(el.StartTag.Attributes, "cref") ?? "", content));
                break;
            case "seealso":
                {
                    var c = GetAttr(el.StartTag.Attributes, "cref");
                    if (c != null) doc.SeeAlso.Add(c);
                    break;
                }
            case "inheritdoc":
                inherit = true;
                inheritCref = GetAttr(el.StartTag.Attributes, "cref");
                break;
        }
    }

    static void HandleEmptyElement(XmlEmptyElementSyntax el, XmlDoc doc, ref bool inherit, ref string? inheritCref)
    {
        var name = el.Name.LocalName.ValueText;
        switch (name)
        {
            case "inheritdoc":
                inherit = true;
                inheritCref = GetAttr(el.Attributes, "cref");
                break;
            case "seealso":
                {
                    var c = GetAttr(el.Attributes, "cref");
                    if (c != null) doc.SeeAlso.Add(c);
                    break;
                }
        }
    }

    static string RenderContent(SyntaxList<XmlNodeSyntax> content)
    {
        var sb = new StringBuilder();
        foreach (var node in content)
            sb.Append(RenderNode(node));
        return Cleanup(sb.ToString());
    }

    static string RenderNode(XmlNodeSyntax node) => node switch
    {
        XmlTextSyntax text => text.ToFullString(),
        XmlElementSyntax el => RenderInlineElement(el),
        XmlEmptyElementSyntax e => RenderInlineEmpty(e),
        _ => node.ToString(),
    };

    static string RenderInlineElement(XmlElementSyntax el)
    {
        var name = el.StartTag.Name.LocalName.ValueText;
        var inner = string.Concat(el.Content.Select(RenderNode));
        return name switch
        {
            "c" or "code" => $"`{Cleanup(inner)}`",
            "paramref" or "typeparamref" => $"`{GetAttr(el.StartTag.Attributes, "name") ?? Cleanup(inner)}`",
            "see" => RenderSee(el.StartTag.Attributes, Cleanup(inner)),
            "para" => "\n\n" + Cleanup(inner) + "\n\n",
            _ => Cleanup(inner),
        };
    }

    static string RenderInlineEmpty(XmlEmptyElementSyntax el)
    {
        var name = el.Name.LocalName.ValueText;
        return name switch
        {
            "see" => RenderSee(el.Attributes, null),
            "paramref" or "typeparamref" => $"`{GetAttr(el.Attributes, "name") ?? ""}`",
            _ => "",
        };
    }

    /// <summary>cref を一時マーカー `{@see:xxx}` に変換。後工程 (Markdown レンダラ) で実リンクに置換する</summary>
    static string RenderSee(SyntaxList<XmlAttributeSyntax> attrs, string? fallback)
    {
        var cref = GetAttr(attrs, "cref");
        if (cref != null)
        {
            var stripped = cref.Length >= 2 && cref[1] == ':' ? cref[2..] : cref;
            return $"{{@see:{stripped}}}";
        }
        var langword = GetAttr(attrs, "langword");
        if (langword != null) return $"`{langword}`";
        var href = GetAttr(attrs, "href");
        if (href != null) return $"[{fallback ?? href}]({href})";
        return fallback ?? "";
    }

    static string? GetAttr(SyntaxList<XmlAttributeSyntax> attrs, string name)
    {
        foreach (var a in attrs)
        {
            if (a.Name.LocalName.ValueText != name) continue;
            return a switch
            {
                XmlCrefAttributeSyntax cr => cr.Cref.ToString(),
                XmlNameAttributeSyntax na => na.Identifier.Identifier.ValueText,
                XmlTextAttributeSyntax tx => string.Concat(tx.TextTokens.Select(t => t.ValueText)),
                _ => a.ToString(),
            };
        }
        return null;
    }

    /// <summary>
    /// XML コメントから抽出した生テキストの正規化:
    ///   - 各行先頭の `///` (+続く 1 スペース) を除去
    ///   - 連続する空行を 1 つに
    ///   - 前後空白を除去
    /// </summary>
    static string Cleanup(string text)
    {
        var lines = text.Split('\n').Select(l =>
        {
            l = l.TrimEnd('\r');
            var idx = l.IndexOf("///", StringComparison.Ordinal);
            if (idx >= 0)
            {
                var after = l[(idx + 3)..];
                if (after.StartsWith(' ')) after = after[1..];
                return after;
            }
            return l;
        });

        var joined = string.Join("\n", lines);
        // 連続空行を 1 つに
        while (joined.Contains("\n\n\n"))
            joined = joined.Replace("\n\n\n", "\n\n");
        return joined.Trim();
    }
}
