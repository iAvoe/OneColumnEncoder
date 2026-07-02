namespace OneColumnEncoder.ScriptGeneration
{
    public static class ScriptTemplate
    {
        public static string BuildAvsSourceLine(string sourcePath, int fpsnum = 0, int fpsden = 0)
        {
            string line = $"LWLibavVideoSource(\"{sourcePath}\")";
            if (fpsnum > 0 && fpsden > 0)
                line = $"LWLibavVideoSource(\"{sourcePath}\", fpsnum={fpsnum}, fpsden={fpsden})";
            return line;
        }

        public static string BuildVpySourceHeader(string sourcePath, int fpsnum = 0, int fpsden = 0)
        {
            string header = $"import vapoursynth as vs\r\ncore = vs.core\r\nsrc = core.lsmas.LWLibavSource(source=r\"{sourcePath}\")";
            if (fpsnum > 0 && fpsden > 0)
                header = $"import vapoursynth as vs\r\ncore = vs.core\r\nsrc = core.lsmas.LWLibavSource(source=r\"{sourcePath}\", fpsnum={fpsnum}, fpsden={fpsden})";
            return header;
        }

        public static string BuildAvsExportScript(string sourcePath, string avsPrefix2, string avsSuffix, string userInput = "", int fpsnum = 0, int fpsden = 0)
        {
            string content = string.IsNullOrEmpty(userInput)
                ? $"{avsPrefix2}\r\n\r\n{avsSuffix}"
                : $"{avsPrefix2}\r\n{userInput}\r\n{avsSuffix}";
            return $"{BuildAvsSourceLine(sourcePath, fpsnum, fpsden)}\r\n{content}";
        }

        public static string BuildVpyExportScript(string sourcePath, string vpyPrefix2, string vpySuffix, string userInput = "", int fpsnum = 0, int fpsden = 0)
        {
            string content = string.IsNullOrEmpty(userInput)
                ? $"\r\n{vpyPrefix2}\r\n\r\n{vpySuffix}"
                : $"\r\n{vpyPrefix2}\r\n{userInput}\r\n{vpySuffix}";
            return $"{BuildVpySourceHeader(sourcePath, fpsnum, fpsden)}{content}";
        }

        public static string BuildAvsInOutSection(string sourcePath, string avsPrefix2, string avsSuffix, int fpsnum = 0, int fpsden = 0)
            => $"{BuildAvsSourceLine(sourcePath, fpsnum, fpsden)}\r\n{avsPrefix2}\r\n\r\n{avsSuffix}";

        public static string BuildVpyInOutSection(string sourcePath, string vpyPrefix2, string vpySuffix, int fpsnum = 0, int fpsden = 0)
            => $"{BuildVpySourceHeader(sourcePath, fpsnum, fpsden)}\r\n{vpyPrefix2}\r\n\r\n{vpySuffix}";

        public static string BuildAvsEditorScript(string sourcePath, string avsPrefix2, string userInput, int fpsnum = 0, int fpsden = 0)
            => $"{BuildAvsSourceLine(sourcePath, fpsnum, fpsden)}\r\n{avsPrefix2}\r\n{userInput}";

        public static string BuildVpyEditorScript(string sourcePath, string vpyPrefix2, string vpySuffix, string userInput, int fpsnum = 0, int fpsden = 0)
            => $"{BuildVpySourceHeader(sourcePath, fpsnum, fpsden)}\r\n{vpyPrefix2}\r\n{userInput}\r\n{vpySuffix}";

        public static string BuildConcatAvsSourceHeader(string[] filePaths)
        {
            List<string> lines = [];
            for (int i = 0; i < filePaths.Length; i++)
            {
                string varName = $"v{i + 1}";
                lines.Add($"{varName} = LWLibavVideoSource(\"{filePaths[i].Replace("\"", "\"\"")}\")");
            }
            lines.Add($"src = {string.Join(" ++ ", Enumerable.Range(1, filePaths.Length).Select(i => $"v{i}"))}");
            return string.Join("\r\n", lines);
        }

        public static string BuildConcatVpySourceHeader(string[] filePaths)
        {
            List<string> lines =
            [
                "import vapoursynth as vs",
                "core = vs.core",
                "video_files = ["
            ];

            for (int i = 0; i < filePaths.Length; i++)
            {
                string suffix = i == filePaths.Length - 1 ? "]" : ",";
                lines.Add($"    r\"{filePaths[i]}\"{suffix}");
            }

            lines.Add("clips = [core.lsmas.LWLibavSource(source=f) for f in video_files]");
            lines.Add("src = core.std.Splice(clips=clips)");
            return string.Join("\r\n", lines);
        }

        public static string BuildConcatFfmpegFileList(string[] filePaths) =>
            OneColumnEncoder.ConcatManagement.ConcatFileListGenerator.BuildFileListContent(filePaths);

        public static string BuildConcatAvsExportScript(string[] filePaths, string avsPrefix2, string avsSuffix, string userInput = "")
        {
            string content = string.IsNullOrEmpty(userInput)
                ? $"{avsPrefix2}\r\n\r\n{avsSuffix}"
                : $"{avsPrefix2}\r\n{userInput}\r\n{avsSuffix}";
            return $"{BuildConcatAvsSourceHeader(filePaths)}\r\n{content}";
        }

        public static string BuildConcatVpyExportScript(string[] filePaths, string vpyPrefix2, string vpySuffix, string userInput = "")
        {
            string content = string.IsNullOrEmpty(userInput)
                ? $"\r\n{vpyPrefix2}\r\n\r\n{vpySuffix}"
                : $"\r\n{vpyPrefix2}\r\n{userInput}\r\n{vpySuffix}";
            return $"{BuildConcatVpySourceHeader(filePaths)}{content}";
        }
    }
}
