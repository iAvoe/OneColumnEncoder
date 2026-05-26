namespace OneColumnEncoder.Helpers
{
    public static class ScriptTemplateH
    {
        public static string BuildAvsSourceLine(string sourcePath)
            => $"LWLibavVideoSource(\"{sourcePath}\")";

        public static string BuildVpySourceHeader(string sourcePath)
            => $"import vapoursynth as vs\r\ncore = vs.core\r\nsrc = core.lsmas.LWLibavSource(source=r\"{sourcePath}\")";

        public static string BuildAvsExportScript(string sourcePath, string avsPrefix, string avsPrefix2, string avsSuffix, string userInput = "")
        {
            string content = string.IsNullOrEmpty(userInput)
                ? $"{avsPrefix2}\r\n\r\n{avsSuffix}"
                : $"{avsPrefix}\r\n{userInput}\r\n{avsSuffix}";
            return $"{BuildAvsSourceLine(sourcePath)}\r\n{content}";
        }

        public static string BuildVpyExportScript(string sourcePath, string vpyPrefix, string vpyPrefix2, string vpySuffix, string userInput = "")
        {
            string header = BuildVpySourceHeader(sourcePath);
            string content = string.IsNullOrEmpty(userInput)
                ? $"\r\n{vpyPrefix2}\r\n\r\n{vpySuffix}"
                : $"\r\n{vpyPrefix}\r\n{userInput}\r\n{vpySuffix}";
            return $"{header}{content}";
        }

        public static string BuildAvsInOutSection(string sourcePath, string avsPrefix2, string avsSuffix)
            => $"{BuildAvsSourceLine(sourcePath)}\r\n{avsPrefix2}\r\n\r\n{avsSuffix}";

        public static string BuildVpyInOutSection(string sourcePath, string vpyPrefix2, string vpySuffix)
            => $"{BuildVpySourceHeader(sourcePath)}\r\n{vpyPrefix2}\r\n\r\n{vpySuffix}";

        public static string BuildAvsEditorScript(string sourcePath, string avsPrefix2, string userInput)
            => $"{BuildAvsSourceLine(sourcePath)}\r\n{avsPrefix2}\r\n{userInput}";

        public static string BuildVpyEditorScript(string sourcePath, string vpyPrefix2, string vpySuffix, string userInput)
            => $"{BuildVpySourceHeader(sourcePath)}\r\n{vpyPrefix2}\r\n{userInput}\r\n{vpySuffix}";
    }
}
