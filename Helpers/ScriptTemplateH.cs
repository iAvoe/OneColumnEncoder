using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Helpers
{
    public class ScriptTemplateH
    {
        public static string BuildAvsExportScript(string sourcePath, string avsPrefix, string avsPrefix2, string avsSuffix, string userInput = "")
        {
            string content = string.IsNullOrEmpty(userInput)
                ? $"{avsPrefix2}\r\n\r\n{avsSuffix}"
                : $"{avsPrefix}\r\n{userInput}\r\n{avsSuffix}";
            return $"LWLibavVideoSource(\"{sourcePath}\")\r\n{content}";
        }
        public static string BuildVpyExportScript(string sourcePath, string vpyPrefix, string vpyPrefix2, string vpySuffix, string userInput = "")
        {
            string header = $"import vapoursynth as vs\r\ncore = vs.core\r\nsrc = core.lsmas.LWLibavSource(source=r\"{sourcePath}\")";
            string content = string.IsNullOrEmpty(userInput)
                ? $"\r\n{vpyPrefix2}\r\n\r\n{vpySuffix}"
                : $"\r\n{vpyPrefix}\r\n{userInput}\r\n{vpySuffix}";
            return $"{header}{content}";
        }
    }
}
