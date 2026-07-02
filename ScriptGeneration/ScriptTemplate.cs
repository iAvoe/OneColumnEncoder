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
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < filePaths.Length; i++)
            {
                string varName = $"v{i + 1}";
                sb.AppendLine($"{varName} = LWLibavVideoSource(\"{filePaths[i].Replace("\"", "\"\"")}\")");
            }
            sb.AppendLine($"src = {string.Join(" ++ ", Enumerable.Range(1, filePaths.Length).Select(i => $"v{i}"))}");
            return sb.ToString();
        }

        public static string BuildConcatVpySourceHeader(string[] filePaths)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("import vapoursynth as vs");
            sb.AppendLine("core = vs.core");
            sb.AppendLine("src = core.std.BlankClip()");
            for (int i = 0; i < filePaths.Length; i++)
            {
                char varName = (char)('a' + i);
                sb.AppendLine($"v{varName} = core.lsmas.LWLibavSource(source=r\"{filePaths[i]}\")");
                if (i == 0)
                    sb.AppendLine($"src = v{varName}");
                else
                    sb.AppendLine($"src = core.std.Splice([src, v{varName}])");
            }
            return sb.ToString();
        }

        public static string BuildConcatFfmpegFileList(string[] filePaths)
        {
            return OneColumnEncoder.ConcatManagement.ConcatFileListGenerator.BuildFileListContent(filePaths);
        }

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
