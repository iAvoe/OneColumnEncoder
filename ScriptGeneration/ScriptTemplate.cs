namespace OneColumnEncoder.ScriptGeneration;

public static class ScriptTemplate
{
    private static string? BuildAvsCacheDirArgument()
    {
        string? cacheDirectory = LwiCacheDirectoryProvider.EnsureDirectory();
        return cacheDirectory is null
            ? null
            : $", cachedir=\"{cacheDirectory.Replace("\"", "\"\"")}\"";
    }

    private static string? BuildVpyCacheDirArgument()
    {
        string? cacheDirectory = LwiCacheDirectoryProvider.EnsureDirectory();
        return cacheDirectory is null
            ? null
            : $", cachedir=r\"{cacheDirectory}\"";
    }

    public static string BuildAvsSourceLine(string srcPath, int fpsnum = 0, int fpsden = 0)
    {
        string arguments = $"\"{srcPath}\"{BuildAvsCacheDirArgument()}";
        if (fpsnum > 0 && fpsden > 0)
            arguments += $", fpsnum={fpsnum}, fpsden={fpsden}";
        return LsmashPluginPathProvider.WithAvsLoadPlugin($"LWLibavVideoSource({arguments})");
    }

    public static string BuildVpySourceHeader(string srcPath, int fpsnum = 0, int fpsden = 0)
    {
        string arguments = $"source=r\"{srcPath}\"{BuildVpyCacheDirArgument()}";
        if (fpsnum > 0 && fpsden > 0)
            arguments += $", fpsnum={fpsnum}, fpsden={fpsden}";
        return $"import vapoursynth as vs\r\ncore = vs.core\r\n{LsmashPluginPathProvider.BuildVpyLoadPluginSection()}src = core.lsmas.LWLibavSource({arguments})";
    }

    public static string BuildAvsExportScript(string srcPath, string avsPrefix2, string avsSuffix, string userInput = "", int fpsnum = 0, int fpsden = 0)
    {
        string content = string.IsNullOrEmpty(userInput)
            ? $"{avsPrefix2}\r\n\r\n{avsSuffix}"
            : $"{avsPrefix2}\r\n{userInput}\r\n{avsSuffix}";
        return $"{BuildAvsSourceLine(srcPath, fpsnum, fpsden)}\r\n{content}";
    }

    public static string BuildVpyExportScript(string srcPath, string vpyPrefix2, string vpySuffix, string userInput = "", int fpsnum = 0, int fpsden = 0)
    {
        string content = string.IsNullOrEmpty(userInput)
            ? $"\r\n{vpyPrefix2}\r\n\r\n{vpySuffix}"
            : $"\r\n{vpyPrefix2}\r\n{userInput}\r\n{vpySuffix}";
        return $"{BuildVpySourceHeader(srcPath, fpsnum, fpsden)}{content}";
    }

    public static string BuildAvsInOutSection(string srcPath, string avsPrefix2, string avsSuffix, int fpsnum = 0, int fpsden = 0)
        => $"{BuildAvsSourceLine(srcPath, fpsnum, fpsden)}\r\n{avsPrefix2}\r\n\r\n{avsSuffix}";

    public static string BuildVpyInOutSection(string srcPath, string vpyPrefix2, string vpySuffix, int fpsnum = 0, int fpsden = 0)
        => $"{BuildVpySourceHeader(srcPath, fpsnum, fpsden)}\r\n{vpyPrefix2}\r\n\r\n{vpySuffix}";

    public static string BuildAvsEditorScript(string srcPath, string avsPrefix2, string userInput, int fpsnum = 0, int fpsden = 0)
        => $"{BuildAvsSourceLine(srcPath, fpsnum, fpsden)}\r\n{avsPrefix2}\r\n{userInput}";

    public static string BuildVpyEditorScript(string srcPath, string vpyPrefix2, string vpySuffix, string userInput, int fpsnum = 0, int fpsden = 0)
        => $"{BuildVpySourceHeader(srcPath, fpsnum, fpsden)}\r\n{vpyPrefix2}\r\n{userInput}\r\n{vpySuffix}";

    public static string BuildConcatAvsSourceHeader(string[] filePaths, int fpsnum = 0, int fpsden = 0)
    {
        if (filePaths.Length < 2)
            throw new ArgumentException("Concat script generation requires at least two source paths.", nameof(filePaths));

        // Build an AviSynth header that assigns each concat segment to its own source variable.
        string BuildSource(string path)
        {
            string arguments = $"\"{path.Replace("\"", "\"\"")}\"{BuildAvsCacheDirArgument()}";
            if (fpsnum > 0 && fpsden > 0)
                arguments += $", fpsnum={fpsnum}, fpsden={fpsden}";
            return $"LWLibavVideoSource({arguments})";
        }

        List<string> lines =
        [
            LsmashPluginPathProvider.BuildAvsLoadPluginLine(),
            $"v1 = {BuildSource(filePaths[0])}",
            $"v2 = {BuildSource(filePaths[1])}"
        ];

        for (int i = 2; i < filePaths.Length; i++)
            lines.Add($"v{i + 1} = {BuildSource(filePaths[i])}");

        // Stitch the individual segment variables into one concat source.
        // AVS doesn't have the "src = " prefix, do not add it
        lines.Add($"{string.Join(" ++ ", Enumerable.Range(1, filePaths.Length).Select(i => $"v{i}"))}");
        return string.Join("\r\n", lines);
    }

    public static string BuildConcatVpySourceHeader(string[] filePaths, int fpsnum = 0, int fpsden = 0)
    {
        if (filePaths.Length < 2)
            throw new ArgumentException("Concat script generation requires at least two source paths.", nameof(filePaths));

        List<string> lines =
        [
            "import vapoursynth as vs",
            "core = vs.core"
        ];

        if (!LsmashPluginPathProvider.VapourSynthHasLsmashPlugin)
            lines.Add(LsmashPluginPathProvider.BuildVpyLoadPluginLine());

        lines.Add("video_files = [");

        lines.Add($"    r\"{filePaths[0]}\",");
        lines.Add($"    r\"{filePaths[1]}\"{(filePaths.Length == 2 ? "]" : ",")}");

        for (int i = 2; i < filePaths.Length; i++)
            lines.Add($"    r\"{filePaths[i]}\"{(i == filePaths.Length - 1 ? "]" : ",")}");

        string lwlibavCall = $"core.lsmas.LWLibavSource(source=f{BuildVpyCacheDirArgument()}"
            + (fpsnum > 0 && fpsden > 0 ? $", fpsnum={fpsnum}, fpsden={fpsden})" : ")");
        lines.Add($"clips = [{lwlibavCall} for f in video_files]");
        lines.Add("src = core.std.Splice(clips=clips)");
        return string.Join("\r\n", lines);
    }

    public static string BuildConcatFfmpegFileList(string[] filePaths) =>
        OneColumnEncoder.ConcatManagement.ConcatFileListGenerator.BuildFileListContent(filePaths);

    public static string BuildConcatAvsExportScript(string[] filePaths, string avsPrefix2, string avsSuffix, string userInput = "", int fpsnum = 0, int fpsden = 0)
    {
        // Reuse the concat AviSynth source header, then append the user script body.
        string content = string.IsNullOrEmpty(userInput)
            ? $"{avsPrefix2}\r\n\r\n{avsSuffix}"
            : $"{avsPrefix2}\r\n{userInput}\r\n{avsSuffix}";
        return $"{BuildConcatAvsSourceHeader(filePaths, fpsnum, fpsden)}\r\n{content}";
    }

    public static string BuildConcatVpyExportScript(string[] filePaths, string vpyPrefix2, string vpySuffix, string userInput = "", int fpsnum = 0, int fpsden = 0)
    {
        string content = string.IsNullOrEmpty(userInput)
            ? $"\r\n{vpyPrefix2}\r\n\r\n{vpySuffix}"
            : $"\r\n{vpyPrefix2}\r\n{userInput}\r\n{vpySuffix}";
        return $"{BuildConcatVpySourceHeader(filePaths, fpsnum, fpsden)}{content}";
    }

    public static string BuildVpyPreviewScript(string srcPath, string userInput, int fpsnum = 0, int fpsden = 0)
    {
        string header = BuildVpySourceHeader(srcPath, fpsnum, fpsden);
        string previewInsert = "\r\nref = src\r\nsrc.set_output(0)";
        string suffix = "\r\n# ...force src as the B preview output, so A/B preview can display output 1...\r\nsrc.set_output(1)";

        string content = string.IsNullOrWhiteSpace(userInput)
            ? $"{previewInsert}\r\n# (no user filters)\r\n{suffix}"
            : $"{previewInsert}\r\n{userInput.Trim()}\r\n{suffix}";

        return $"{header}{content}";
    }

    public static string BuildAvsPreviewScript(string srcPath, string userInput, int fpsnum = 0, int fpsden = 0)
    {
        string source = BuildAvsSourceLine(srcPath, fpsnum, fpsden);
        return string.IsNullOrWhiteSpace(userInput)
            ? $"{source}\r\n# (no user filters)"
            : $"{source}\r\n{userInput.Trim()}";
    }
}
