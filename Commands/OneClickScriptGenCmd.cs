using OneColumnEncoder.Models;
using System;
using System.Windows;

namespace OneColumnEncoder.Commands
{
    public class OneClickScriptGenCmd(Func<string> getSourcePath) : BaseCmd
    {
        private readonly Func<string> _getSourcePath = getSourcePath;
        public override void Execute(object? parameter)
        {
            string sourcePath = _getSourcePath();
            string script = string.IsNullOrWhiteSpace(sourcePath)
                ? "LWLibavVideoSource(\"C:\\path\\to\\video.mkv\")\r\n" +
                  "# 请先在主界面导入视频源文件，或手动修改路径"
                : $"LWLibavVideoSource(\"{sourcePath}\")\r\n";

            Clipboard.SetText(script);
            MessageBox.Show(
                UILangProviderM.Current["SrcScribe.CopiedFull"],
                UILangProviderM.Current["SrcScribe.Title"],
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
