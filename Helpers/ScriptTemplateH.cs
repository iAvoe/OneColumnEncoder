namespace OneColumnEncoder.Helpers
{
    public static class ScriptTemplateH
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
    }
}
