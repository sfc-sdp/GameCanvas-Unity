using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GenApiDocs;

public sealed class SourceParser
{
    readonly List<ApiType> _types = new();
    readonly string _namespaceRoot;

    public SourceParser(string namespaceRoot)
    {
        _namespaceRoot = namespaceRoot;
    }

    public void AddFile(string path)
    {
        var text = File.ReadAllText(path);
        var tree = CSharpSyntaxTree.ParseText(text, path: path);
        var root = (CompilationUnitSyntax)tree.GetRoot();
        foreach (var nsNode in CollectNamespaceBodies(root))
        {
            var (ns, members) = nsNode;
            foreach (var m in members)
            {
                if (m is BaseTypeDeclarationSyntax t)
                    CollectType(ns, t, outerType: null);
                else if (m is DelegateDeclarationSyntax d)
                    CollectDelegate(ns, d);
            }
        }
    }

    static IEnumerable<(string Namespace, IEnumerable<MemberDeclarationSyntax> Members)> CollectNamespaceBodies(CompilationUnitSyntax root)
    {
        // 1) file-scoped / block-scoped namespaces
        foreach (var ns in root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>())
        {
            // file-scoped namespace は root 直下のみ、block-scoped は入れ子 (稀) がありうる
            // このプロジェクトでは namespace の入れ子は使われていないため、直下のメンバーだけ集める
            yield return (ns.Name.ToString(), ns.Members);
        }
        // 2) 名前空間なしの top-level 型 (念のため)
        var topLevel = root.Members.Where(m => m is not BaseNamespaceDeclarationSyntax);
        if (topLevel.Any())
            yield return (string.Empty, topLevel);
    }

    void CollectType(string ns, BaseTypeDeclarationSyntax t, ApiType? outerType)
    {
        if (!IsPublic(t.Modifiers)) return;
        if (HasExcludingAttribute(t.AttributeLists)) return;

        var simple = t.Identifier.ValueText;
        var typeParams = (t as TypeDeclarationSyntax)?.TypeParameterList;
        var displayName = simple + (typeParams != null ? typeParams.ToString() : "");
        var arity = typeParams?.Parameters.Count ?? 0;

        string effectiveNs = ns;
        if (outerType != null)
            effectiveNs = outerType.FullNameSimple; // ネスト型は外部型を namespace 扱いして "A.B.Inner" として扱う

        var kind = t switch
        {
            InterfaceDeclarationSyntax => ApiKind.Interface,
            ClassDeclarationSyntax => ApiKind.Class,
            StructDeclarationSyntax => ApiKind.Struct,
            RecordDeclarationSyntax r => r.Keyword.ValueText == "struct" ? ApiKind.Struct : ApiKind.Class,
            EnumDeclarationSyntax => ApiKind.Enum,
            _ => ApiKind.Class,
        };

        var modifiers = ModifiersToString(t.Modifiers);
        var decl = BuildTypeDeclaration(t, modifiers, displayName);

        var api = new ApiType
        {
            Namespace = effectiveNs,
            Name = displayName,
            SimpleName = simple,
            FileName = BuildFileName(effectiveNs, simple, arity),
            Kind = kind,
            Modifiers = modifiers,
            DeclarationSyntax = decl,
        };
        api.Doc = XmlDocExtractor.Extract(t);

        if (t is TypeDeclarationSyntax td && td.BaseList != null)
        {
            foreach (var bt in td.BaseList.Types)
                api.BaseTypes.Add(bt.Type.ToString());
        }

        // メンバー収集
        if (t is TypeDeclarationSyntax type)
        {
            foreach (var m in type.Members)
                CollectMember(api, m, effectiveNs, simple);
        }
        else if (t is EnumDeclarationSyntax en)
        {
            foreach (var m in en.Members)
                CollectEnumValue(api, m);
        }

        _types.Add(api);

        // ネスト型の再帰
        if (t is TypeDeclarationSyntax outer)
        {
            foreach (var child in outer.Members.OfType<BaseTypeDeclarationSyntax>())
                CollectType(effectiveNs, child, api);
            foreach (var del in outer.Members.OfType<DelegateDeclarationSyntax>())
                CollectDelegate(effectiveNs, del);
        }
    }

    void CollectDelegate(string ns, DelegateDeclarationSyntax d)
    {
        if (!IsPublic(d.Modifiers)) return;
        if (HasExcludingAttribute(d.AttributeLists)) return;

        var simple = d.Identifier.ValueText;
        var typeParams = d.TypeParameterList;
        var displayName = simple + (typeParams != null ? typeParams.ToString() : "");
        var arity = typeParams?.Parameters.Count ?? 0;

        var modifiers = ModifiersToString(d.Modifiers);
        var api = new ApiType
        {
            Namespace = ns,
            Name = displayName,
            SimpleName = simple,
            FileName = BuildFileName(ns, simple, arity),
            Kind = ApiKind.Delegate,
            Modifiers = modifiers,
            DeclarationSyntax = $"{modifiers} delegate {d.ReturnType} {displayName}{d.ParameterList}",
        };
        api.Doc = XmlDocExtractor.Extract(d);
        _types.Add(api);
    }

    void CollectMember(ApiType api, MemberDeclarationSyntax m, string ns, string outerTypeName)
    {
        if (m is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax) return; // ネストは別経路
        if (!IsPublicOrProtected(m)) return;
        if (HasExcludingAttribute(GetAttributeLists(m))) return;

        ApiMember? member = m switch
        {
            FieldDeclarationSyntax f => BuildField(f),
            PropertyDeclarationSyntax p => BuildProperty(p),
            IndexerDeclarationSyntax idx => BuildIndexer(idx),
            MethodDeclarationSyntax meth => BuildMethod(meth),
            ConstructorDeclarationSyntax ctor => BuildConstructor(ctor, outerTypeName),
            EventDeclarationSyntax ev => BuildEvent(ev),
            EventFieldDeclarationSyntax evf => BuildEventField(evf),
            OperatorDeclarationSyntax op => BuildOperator(op),
            ConversionOperatorDeclarationSyntax cv => BuildConversion(cv),
            _ => null,
        };
        if (member == null) return;
        if (!FilterRule.ShouldIncludeMember(api, member)) return;
        api.Members.Add(member);
    }

    static void CollectEnumValue(ApiType api, EnumMemberDeclarationSyntax m)
    {
        if (HasExcludingAttribute(m.AttributeLists)) return;
        var name = m.Identifier.ValueText;
        var value = m.EqualsValue != null ? $" = {m.EqualsValue.Value}" : "";
        var member = new ApiMember
        {
            Kind = MemberKind.EnumValue,
            Name = name,
            Id = $"Value.{name}",
            DisplayName = name,
            Signature = name + value,
            Doc = XmlDocExtractor.Extract(m),
        };
        api.Members.Add(member);
    }

    static ApiMember BuildField(FieldDeclarationSyntax f)
    {
        var v = f.Declaration.Variables.First();
        var name = v.Identifier.ValueText;
        var mods = ModifiersToString(f.Modifiers);
        var type = f.Declaration.Type;
        var init = v.Initializer != null ? $" = {v.Initializer.Value}" : "";
        var sig = $"{mods} {type} {name}{init}";
        return new ApiMember
        {
            Kind = MemberKind.Field,
            Name = name,
            Id = $"Field.{name}",
            DisplayName = name,
            Signature = sig,
            Doc = XmlDocExtractor.Extract(f),
        };
    }

    static ApiMember BuildProperty(PropertyDeclarationSyntax p)
    {
        var name = p.Identifier.ValueText;
        var mods = ModifiersToString(p.Modifiers);
        var accessors = BuildAccessors(p.AccessorList, p.ExpressionBody);
        var sig = $"{mods} {p.Type} {name} {{ {accessors} }}".Trim();
        var (doc, inheritCref, inherit) = XmlDocExtractor.ExtractWithInheritdoc(p);
        return new ApiMember
        {
            Kind = MemberKind.Property,
            Name = name,
            Id = $"Property.{name}",
            DisplayName = name,
            Signature = sig,
            Doc = doc,
            InheritdocCref = inheritCref,
            Inheritdoc = inherit,
        };
    }

    static ApiMember BuildIndexer(IndexerDeclarationSyntax idx)
    {
        var mods = ModifiersToString(idx.Modifiers);
        var accessors = BuildAccessors(idx.AccessorList, idx.ExpressionBody);
        var sig = $"{mods} {idx.Type} this{idx.ParameterList} {{ {accessors} }}".Trim();
        var paramTypesCompact = ParamTypesCompact(idx.ParameterList);
        var (doc, inheritCref, inherit) = XmlDocExtractor.ExtractWithInheritdoc(idx);
        return new ApiMember
        {
            Kind = MemberKind.Indexer,
            Name = "this",
            Id = $"Indexer[{paramTypesCompact}]",
            DisplayName = $"this[{ParamTypesDisplay(idx.ParameterList)}]",
            Signature = sig,
            Doc = doc,
            InheritdocCref = inheritCref,
            Inheritdoc = inherit,
        };
    }

    static ApiMember BuildMethod(MethodDeclarationSyntax meth)
    {
        var name = meth.Identifier.ValueText;
        var mods = ModifiersToString(meth.Modifiers);
        var typeParams = meth.TypeParameterList?.ToString() ?? "";
        var constraints = meth.ConstraintClauses.Count > 0
            ? " " + string.Join(" ", meth.ConstraintClauses.Select(c => c.ToString()))
            : "";
        var sig = $"{mods} {meth.ReturnType} {name}{typeParams}{meth.ParameterList}{constraints}".Trim();
        var paramTypesCompact = ParamTypesCompact(meth.ParameterList);
        var (doc, inheritCref, inherit) = XmlDocExtractor.ExtractWithInheritdoc(meth);
        return new ApiMember
        {
            Kind = MemberKind.Method,
            Name = name,
            Id = $"Method.{name}{typeParams}({paramTypesCompact})",
            DisplayName = $"{name}{typeParams}({ParamTypesDisplay(meth.ParameterList)})",
            Signature = sig,
            Doc = doc,
            InheritdocCref = inheritCref,
            Inheritdoc = inherit,
        };
    }

    static ApiMember BuildConstructor(ConstructorDeclarationSyntax ctor, string outerTypeName)
    {
        var mods = ModifiersToString(ctor.Modifiers);
        var sig = $"{mods} {outerTypeName}{ctor.ParameterList}".Trim();
        var paramTypesCompact = ParamTypesCompact(ctor.ParameterList);
        return new ApiMember
        {
            Kind = MemberKind.Constructor,
            Name = outerTypeName,
            Id = $"Constructor({paramTypesCompact})",
            DisplayName = $"{outerTypeName}({ParamTypesDisplay(ctor.ParameterList)})",
            Signature = sig,
            Doc = XmlDocExtractor.Extract(ctor),
        };
    }

    static ApiMember BuildEvent(EventDeclarationSyntax ev)
    {
        var name = ev.Identifier.ValueText;
        var mods = ModifiersToString(ev.Modifiers);
        var sig = $"{mods} event {ev.Type} {name}";
        return new ApiMember
        {
            Kind = MemberKind.Event,
            Name = name,
            Id = $"Event.{name}",
            DisplayName = name,
            Signature = sig,
            Doc = XmlDocExtractor.Extract(ev),
        };
    }

    static ApiMember BuildEventField(EventFieldDeclarationSyntax ev)
    {
        var v = ev.Declaration.Variables.First();
        var name = v.Identifier.ValueText;
        var mods = ModifiersToString(ev.Modifiers);
        var sig = $"{mods} event {ev.Declaration.Type} {name}";
        return new ApiMember
        {
            Kind = MemberKind.Event,
            Name = name,
            Id = $"Event.{name}",
            DisplayName = name,
            Signature = sig,
            Doc = XmlDocExtractor.Extract(ev),
        };
    }

    static ApiMember BuildOperator(OperatorDeclarationSyntax op)
    {
        var mods = ModifiersToString(op.Modifiers);
        var opToken = op.OperatorToken.ValueText;
        var displayName = $"operator {opToken}";
        var sig = $"{mods} {op.ReturnType} {displayName}{op.ParameterList}".Trim();
        var paramTypesCompact = ParamTypesCompact(op.ParameterList);
        return new ApiMember
        {
            Kind = MemberKind.Operator,
            Name = displayName,
            Id = $"Operator.{opToken}({paramTypesCompact})",
            DisplayName = $"{displayName}({ParamTypesDisplay(op.ParameterList)})",
            Signature = sig,
            Doc = XmlDocExtractor.Extract(op),
        };
    }

    static ApiMember BuildConversion(ConversionOperatorDeclarationSyntax cv)
    {
        var mods = ModifiersToString(cv.Modifiers);
        var kind = cv.ImplicitOrExplicitKeyword.ValueText;
        var targetType = cv.Type.ToString();
        var displayName = $"{kind} operator {targetType}";
        var sig = $"{mods} {displayName}{cv.ParameterList}".Trim();
        var paramTypesCompact = ParamTypesCompact(cv.ParameterList);
        return new ApiMember
        {
            Kind = MemberKind.Operator,
            Name = displayName,
            Id = $"Conversion.{kind}_{targetType}({paramTypesCompact})",
            DisplayName = $"{displayName}({ParamTypesDisplay(cv.ParameterList)})",
            Signature = sig,
            Doc = XmlDocExtractor.Extract(cv),
        };
    }

    /// <summary>ID 生成用: スペースなしの引数型リスト。ref/in/out などの修飾子は含めない (同じ型・違う修飾子で完全一致するケースは想定しない)</summary>
    static string ParamTypesCompact(BaseParameterListSyntax list) =>
        string.Join(",", list.Parameters.Select(p => p.Type?.ToString() ?? "object"));

    /// <summary>見出し表示用: 空白付きの引数型リスト</summary>
    static string ParamTypesDisplay(BaseParameterListSyntax list) =>
        string.Join(", ", list.Parameters.Select(p => p.Type?.ToString() ?? "object"));

    static string BuildAccessors(AccessorListSyntax? list, ArrowExpressionClauseSyntax? expr)
    {
        if (expr != null) return "get;";
        if (list == null) return "";
        var parts = new List<string>();
        foreach (var a in list.Accessors)
        {
            if (!IsPublicOrProtectedAccessor(a)) continue;
            var keyword = a.Keyword.ValueText;
            var mods = a.Modifiers.Count > 0 ? ModifiersToString(a.Modifiers) + " " : "";
            parts.Add($"{mods}{keyword};");
        }
        return string.Join(" ", parts);
    }

    static bool IsPublicOrProtectedAccessor(AccessorDeclarationSyntax a)
    {
        if (a.Modifiers.Any(m => m.ValueText == "private" || m.ValueText == "internal"))
            return false;
        return true;
    }

    static SyntaxList<AttributeListSyntax> GetAttributeLists(MemberDeclarationSyntax m) => m switch
    {
        BaseFieldDeclarationSyntax f => f.AttributeLists,
        BasePropertyDeclarationSyntax p => p.AttributeLists,
        BaseMethodDeclarationSyntax meth => meth.AttributeLists,
        _ => default,
    };

    static bool IsPublic(SyntaxTokenList mods) => mods.Any(m => m.ValueText == "public");

    static bool IsPublicOrProtected(MemberDeclarationSyntax m)
    {
        var mods = m switch
        {
            BaseFieldDeclarationSyntax f => f.Modifiers,
            BasePropertyDeclarationSyntax p => p.Modifiers,
            BaseMethodDeclarationSyntax meth => meth.Modifiers,
            _ => default,
        };
        if (mods.Any(x => x.ValueText == "private" || x.ValueText == "internal")) return false;
        if (mods.Any(x => x.ValueText == "public" || x.ValueText == "protected")) return true;
        // interface メンバーは修飾子が無くても public 扱い
        return m.Parent is InterfaceDeclarationSyntax;
    }

    static bool HasExcludingAttribute(SyntaxList<AttributeListSyntax> lists)
    {
        foreach (var list in lists)
            foreach (var attr in list.Attributes)
            {
                var name = StripAttributeSuffix(attr.Name.ToString());
                if (name == "Obsolete") return true;
                if (name == "EditorBrowsable")
                {
                    var args = attr.ArgumentList?.Arguments.ToString() ?? "";
                    if (args.Contains("Never")) return true;
                }
            }
        return false;
    }

    static string StripAttributeSuffix(string name)
    {
        // "System.ObsoleteAttribute" / "ObsoleteAttribute" / "Obsolete" → "Obsolete"
        var last = name.Split('.').Last();
        return last.EndsWith("Attribute") ? last[..^"Attribute".Length] : last;
    }

    static string ModifiersToString(SyntaxTokenList mods) =>
        string.Join(" ", mods.Select(t => t.ValueText).Where(v => v != "async" && v != "partial"));

    static string BuildTypeDeclaration(BaseTypeDeclarationSyntax t, string modifiers, string displayName)
    {
        var keyword = t switch
        {
            InterfaceDeclarationSyntax => "interface",
            ClassDeclarationSyntax => "class",
            StructDeclarationSyntax => "struct",
            RecordDeclarationSyntax r => r.Keyword.ValueText == "struct" ? "record struct" : "record",
            EnumDeclarationSyntax => "enum",
            _ => "",
        };
        var baseList = t.BaseList != null ? " : " + string.Join(", ", t.BaseList.Types.Select(bt => bt.Type.ToString())) : "";
        var constraints = t is TypeDeclarationSyntax td && td.ConstraintClauses.Count > 0
            ? " " + string.Join(" ", td.ConstraintClauses.Select(c => c.ToString()))
            : "";
        return $"{modifiers} {keyword} {displayName}{baseList}{constraints}".Trim();
    }

    static string BuildFileName(string ns, string simpleName, int arity)
    {
        // DocFX に合わせて "GameCanvas.GcProxy" や "GameCanvas.DictWithLife-2" 形式にする
        var n = string.IsNullOrEmpty(ns) ? simpleName : $"{ns}.{simpleName}";
        return arity > 0 ? $"{n}-{arity}" : n;
    }

    public IEnumerable<ApiType> GetTypes() => _types;

    /// <summary>&lt;inheritdoc/&gt; を同プロジェクト内の基底型・インターフェイスから解決する</summary>
    public static void ResolveInheritdoc(ApiType[] allTypes)
    {
        var byFullName = allTypes
            .GroupBy(t => t.FullNameSimple)
            .ToDictionary(g => g.Key, g => g.First());

        // 型名 (short) → ApiType  （同名型は先勝ち。衝突は想定しない）
        var bySimple = allTypes
            .GroupBy(t => t.SimpleName)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var t in allTypes)
        {
            foreach (var m in t.Members)
            {
                if (!m.Inheritdoc && m.InheritdocCref == null) continue;

                var resolved = TryResolve(t, m, byFullName, bySimple);
                if (resolved != null && !resolved.IsEmpty)
                    m.Doc = MergeDocs(m.Doc, resolved);
            }
        }
    }

    static XmlDoc? TryResolve(ApiType t, ApiMember m,
        Dictionary<string, ApiType> byFullName,
        Dictionary<string, ApiType> bySimple)
    {
        // cref 指定があれば優先 (メンバー cref は Id で一致させる)
        if (m.InheritdocCref != null)
        {
            var (typeName, memberName) = SplitCref(m.InheritdocCref);
            if (typeName != null && memberName != null)
            {
                var target = ResolveType(typeName, t.Namespace, byFullName, bySimple);
                if (target != null)
                {
                    // cref が引数型付きならそのまま Id 一致、名前のみなら先頭一致のフォールバック
                    var match = target.Members.FirstOrDefault(x => x.Id.EndsWith("." + memberName) || x.Name == memberName);
                    if (match != null) return match.Doc;
                }
            }
        }
        // 基底型/インターフェイス階層を BFS で辿って "完全に同じ Id" のメンバーを探す
        // (オーバーロード同士の誤継承を防ぐ)
        var queue = new Queue<ApiType>();
        var visited = new HashSet<ApiType>();
        EnqueueBases(queue, t, bySimple);
        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (!visited.Add(cur)) continue;
            var match = cur.Members.FirstOrDefault(x => x.Id == m.Id);
            if (match != null && !match.Doc.IsEmpty) return match.Doc;
            EnqueueBases(queue, cur, bySimple);
        }
        return null;
    }

    static void EnqueueBases(Queue<ApiType> queue, ApiType t, Dictionary<string, ApiType> bySimple)
    {
        foreach (var bn in t.BaseTypes)
        {
            var baseSimple = StripGenericArgs(bn).Split('.').Last();
            if (bySimple.TryGetValue(baseSimple, out var bt))
                queue.Enqueue(bt);
        }
    }

    static (string? TypeName, string? MemberName) SplitCref(string cref)
    {
        // "T:GameCanvas.Foo" / "M:GameCanvas.Foo.Bar(System.Int32)" / "P:GameCanvas.Foo.Bar" / "Foo.Bar"
        var stripped = cref.Length >= 2 && cref[1] == ':' ? cref[2..] : cref;
        // 引数部分を除去
        var paren = stripped.IndexOf('(');
        if (paren >= 0) stripped = stripped[..paren];
        var lastDot = stripped.LastIndexOf('.');
        if (lastDot < 0) return (null, stripped);
        return (stripped[..lastDot], stripped[(lastDot + 1)..]);
    }

    static ApiType? ResolveType(string typeName, string currentNamespace,
        Dictionary<string, ApiType> byFullName, Dictionary<string, ApiType> bySimple)
    {
        if (byFullName.TryGetValue(typeName, out var full)) return full;
        if (byFullName.TryGetValue($"{currentNamespace}.{typeName}", out var qualified)) return qualified;
        var simple = typeName.Split('.').Last();
        return bySimple.TryGetValue(simple, out var s) ? s : null;
    }

    static string StripGenericArgs(string name)
    {
        var lt = name.IndexOf('<');
        return lt >= 0 ? name[..lt] : name;
    }

    static XmlDoc MergeDocs(XmlDoc target, XmlDoc source)
    {
        // target が未設定のフィールドだけ source から埋める
        target.Summary ??= source.Summary;
        target.Remarks ??= source.Remarks;
        target.Returns ??= source.Returns;
        target.Value ??= source.Value;
        if (target.Params.Count == 0) target.Params.AddRange(source.Params);
        if (target.TypeParams.Count == 0) target.TypeParams.AddRange(source.TypeParams);
        if (target.Exceptions.Count == 0) target.Exceptions.AddRange(source.Exceptions);
        if (target.Examples.Count == 0) target.Examples.AddRange(source.Examples);
        if (target.SeeAlso.Count == 0) target.SeeAlso.AddRange(source.SeeAlso);
        return target;
    }
}
