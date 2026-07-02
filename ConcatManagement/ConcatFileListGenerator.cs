using System.IO;
using System.Text;

namespace OneColumnEncoder.ConcatManagement
{
    public static class ConcatFileListGenerator
    {
        public static string GenerateFileList(string[] filePaths, string outputPath)
        {
            var sb = new StringBuilder();
            foreach (string path in filePaths)
            {
                sb.AppendLine($"file '{EscapePath(path)}'");
            }

            string? directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
            return outputPath;
        }

        public static string BuildFileListContent(string[] filePaths)
        {
            var sb = new StringBuilder();
            foreach (string path in filePaths)
                sb.AppendLine($"file '{EscapePath(path)}'");
            return sb.ToString();
        }

        private static string EscapePath(string path) =>
            Path.GetFullPath(path)
                .Replace('\\', '/')
                .Replace("'", "'\\''");
    }
}
