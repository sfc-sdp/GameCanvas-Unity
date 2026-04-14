namespace GenApiDocs;

public static class Program
{
    public static int Main(string[] args)
    {
        string? source = null;
        string? output = null;
        string namespaceRoot = "GameCanvas";

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--source" when i + 1 < args.Length: source = args[++i]; break;
                case "--output" when i + 1 < args.Length: output = args[++i]; break;
                case "--namespace-root" when i + 1 < args.Length: namespaceRoot = args[++i]; break;
                case "-h":
                case "--help":
                    PrintUsage();
                    return 0;
            }
        }

        if (source == null || output == null)
        {
            PrintUsage();
            return 1;
        }

        if (!Directory.Exists(source))
        {
            Console.Error.WriteLine($"source directory not found: {source}");
            return 1;
        }

        var files = Directory.EnumerateFiles(source, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsEditorOrTestPath(p))
            .ToArray();
        Console.WriteLine($"parsing {files.Length} .cs files under {source}");

        var parser = new SourceParser(namespaceRoot);
        foreach (var f in files)
            parser.AddFile(f);

        var all = parser.GetTypes().Where(FilterRule.ShouldInclude).ToArray();
        SourceParser.ResolveInheritdoc(all);

        // 重複型名 (partial の複数ファイル分) をマージ
        var merged = MergePartials(all);

        // 継承メンバーを各型に展開 (IGameCanvas のような集約インターフェイス向け)
        ResolveInheritedMembers(merged);

        Console.WriteLine($"generating {merged.Length} type pages → {output}");

        // リンク解決用インデックス
        var byFullSimple = merged.GroupBy(t => t.FullNameSimple).ToDictionary(g => g.Key, g => g.First());
        var bySimpleName = merged.GroupBy(t => t.SimpleName).ToDictionary(g => g.Key, g => g.First());

        Directory.CreateDirectory(output);
        foreach (var t in merged)
        {
            var md = MarkdownRenderer.RenderType(t, byFullSimple, bySimpleName);
            File.WriteAllText(Path.Combine(output, $"{t.FileName}.md"), md);
        }
        File.WriteAllText(Path.Combine(output, "index.md"), MarkdownRenderer.RenderIndex(merged, byFullSimple, bySimpleName));
        File.WriteAllText(Path.Combine(output, "toc.json"), MarkdownRenderer.RenderToc(merged));

        Console.WriteLine("done.");
        return 0;
    }

    static bool IsEditorOrTestPath(string path)
    {
        var sep = Path.DirectorySeparatorChar;
        var p = path.Replace('/', sep);
        return p.Contains($"{sep}Editor{sep}") || p.Contains($"{sep}Tests{sep}");
    }

    static ApiType[] MergePartials(IEnumerable<ApiType> all)
    {
        var groups = all.GroupBy(t => t.FullNameSimple + "-" + t.Name);
        var result = new List<ApiType>();
        foreach (var g in groups)
        {
            var first = g.First();
            foreach (var other in g.Skip(1))
            {
                // メンバー重複は Id (Kind + 名前 + 引数型列) で排除。オーバーロードは別 Id なので残る
                foreach (var m in other.Members)
                {
                    if (first.Members.Any(x => x.Id == m.Id)) continue;
                    first.Members.Add(m);
                }
                // 基底型の追加
                foreach (var b in other.BaseTypes)
                    if (!first.BaseTypes.Contains(b)) first.BaseTypes.Add(b);
                // Doc が空なら上書き
                if (first.Doc.IsEmpty && !other.Doc.IsEmpty) first.Doc = other.Doc;
            }
            result.Add(first);
        }
        return result.ToArray();
    }

    /// <summary>
    /// 各型について、基底型・継承インターフェイスを BFS で辿り、直接定義に無いメンバーを
    /// <see cref="ApiType.InheritedMembers"/> に収集する。Id で重複を排除しつつ、最初に到達した
    /// 基底を継承元として記録する (= より浅い継承元が優先される)。
    /// </summary>
    static void ResolveInheritedMembers(ApiType[] all)
    {
        var bySimple = all.GroupBy(t => t.SimpleName).ToDictionary(g => g.Key, g => g.First());

        foreach (var t in all)
        {
            var seenIds = new HashSet<string>(t.Members.Select(m => m.Id));
            var visited = new HashSet<ApiType> { t };
            var queue = new Queue<ApiType>();
            EnqueueBases(queue, t, bySimple);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (!visited.Add(cur)) continue;

                foreach (var m in cur.Members)
                {
                    // コンストラクターは継承されないので展開対象から除外する
                    if (m.Kind == MemberKind.Constructor) continue;
                    if (seenIds.Add(m.Id))
                        t.InheritedMembers.Add((m, cur));
                }
                EnqueueBases(queue, cur, bySimple);
            }
        }
    }

    static void EnqueueBases(Queue<ApiType> queue, ApiType t, Dictionary<string, ApiType> bySimple)
    {
        foreach (var bn in t.BaseTypes)
        {
            var simple = StripGenericArgs(bn).Split('.').Last();
            if (bySimple.TryGetValue(simple, out var bt))
                queue.Enqueue(bt);
        }
    }

    static string StripGenericArgs(string name)
    {
        var lt = name.IndexOf('<');
        return lt >= 0 ? name[..lt] : name;
    }

    static void PrintUsage()
    {
        Console.Error.WriteLine(
            "usage: gen-api-docs --source <dir> --output <dir> [--namespace-root <ns>]\n" +
            "\n" +
            "  --source          C# ソースディレクトリ (再帰)\n" +
            "  --output          Markdown 出力先\n" +
            "  --namespace-root  ルート名前空間 (既定: GameCanvas)");
    }
}
