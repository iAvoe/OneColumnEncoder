using System.IO;
using System.Linq;
using System.Text;

namespace OneColumnEncoder.ConcatManagement
{
    public static class ConcatFileListGenerator
    {
        public static string GenerateFileList(string[] filePaths, string outputPath)
        {
            string? directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(outputPath, BuildFileListContent(filePaths), new UTF8Encoding(false));
            return outputPath;
        }

        public static string BuildFileListContent(string[] filePaths) =>
            string.Join("\r\n", filePaths.Select(path => $"file '{EscapePath(path)}'"));

        private static string EscapePath(string path) =>
            Path.GetFullPath(path)
                .Replace('\\', '/')
                .Replace("'", "'\\''");
    }
}
