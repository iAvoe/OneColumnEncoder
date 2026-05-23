using Microsoft.Win32;
using OneColumnEncoder.Commands;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System;
using System.IO;
using System.Windows;

namespace OneColumnEncoder.Helpers
{
    public enum SourceFileKind
    {
        Video,
        AviSynthScript,
        VapourSynthScript,
        SvfiIni
    }

    public static class SourceFilePickerH
    {
        public static string? GetSource(
            SourceFileKind fileKind,
            string windowTitle,
            ModalNavS? modalNavS = null,
            string? foundPath = null,
            string? currentPath = null,
            string? errorMessage = null)
        {
            string initialDirectory = ResolveInitialDirectory(fileKind, foundPath, currentPath);
            string filter = GetFilter(fileKind);
            string retryMessage = string.IsNullOrWhiteSpace(errorMessage)
                ? GetMissingSelectionMessage()
                : errorMessage;

            while (true)
            {
                string? filePath = SelectFile(windowTitle, filter, initialDirectory);
                if (!string.IsNullOrWhiteSpace(filePath))
                    return filePath;

                if (!ShouldRetrySelection(modalNavS, retryMessage))
                    return null;
            }
        }

        public static string GetPrimaryText(SourceFileKind fileKind, string filePath)
        {
            return fileKind == SourceFileKind.Video
                ? Path.GetFileName(filePath)
                : GetCustomScriptModeText();
        }

        private static string? SelectFile(string title, string filter, string? initialDirectory)
        {
            OpenFileDialog dialog = new()
            {
                Title = title,
                Filter = filter,
                InitialDirectory = NormalizeInitialDirectory(initialDirectory),
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false
            };

            return dialog.ShowDialog(Application.Current.MainWindow) == true
                ? dialog.FileName
                : null;
        }

        private static string ResolveInitialDirectory(SourceFileKind fileKind, string? foundPath, string? currentPath)
        {
            if (!string.IsNullOrWhiteSpace(currentPath))
                return currentPath;

            if (fileKind == SourceFileKind.SvfiIni && !string.IsNullOrWhiteSpace(foundPath))
                return foundPath;

            return string.Empty;
        }

        private static string NormalizeInitialDirectory(string? initialDirectory)
        {
            string fallbackDirectory =
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            if (string.IsNullOrWhiteSpace(initialDirectory))
                return fallbackDirectory;

            if (File.Exists(initialDirectory))
            {
                string? parentDir = Path.GetDirectoryName(initialDirectory);
                return Directory.Exists(parentDir) ? parentDir : fallbackDirectory;
            }

            return Directory.Exists(initialDirectory) ? initialDirectory : fallbackDirectory;
        }

        private static string GetFilter(SourceFileKind fileKind) => fileKind switch
        {
            SourceFileKind.Video =>
                "Video files (*.mkv;*.mp4;*.mov;*.avi;*.m2ts;*.ts;*.webm;*.mxf)|*.mkv;*.mp4;*.mov;*.avi;*.m2ts;*.ts;*.webm;*.mxf|All files (*.*)|*.*",
            SourceFileKind.AviSynthScript =>
                "AviSynth script files (*.avs)|*.avs",
            SourceFileKind.VapourSynthScript =>
                "VapourSynth script files (*.vpy)|*.vpy",
            SourceFileKind.SvfiIni =>
                "SVFI configuration files (*.ini)|*.ini",
            _ =>
                "All files (*.*)|*.*"
        };

        private static bool ShouldRetrySelection(ModalNavS? modalNavS, string message)
        {
            if (modalNavS != null)
                return ShowWarningConfirmation(modalNavS, GetNoFileSelectedTitle(), message);

            string title = GetNoFileSelectedTitle();
            MessageBoxResult result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.Yes);

            return result == MessageBoxResult.Yes;
        }

        private static bool ShowWarningConfirmation(ModalNavS modalNavS, string title, string message)
        {
            bool result = false;

            ConfirmationModal window = new();
            ActionCmd cancelCmd = new(_ => { result = false; window.Close(); });
            ActionCmd confirmCmd = new(_ => { result = true; window.Close(); });
            ConfirmationModalVM vm = ConfirmationModalVM.CreateWarning(title, message, cancelCmd, confirmCmd);

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => modalNavS.Close();
            modalNavS.CurrentModalVM = vm;
            window.ShowDialog();

            return result;
        }

        private static string GetNoFileSelectedTitle() => UILangProviderM.Current.LanguageCode switch
        {
            "zh-cn" => "未选择文件",
            "zh-tw" => "未選擇檔案",
            _ => "No file selected"
        };

        private static string GetMissingSelectionMessage() => UILangProviderM.Current.LanguageCode switch
        {
            "zh-cn" => "未选择文件。选择「是」重试，选择「否」取消。",
            "zh-tw" => "未選擇檔案。選擇「是」重試，選擇「否」取消。",
            _ => "No file selected. Choose Yes to try again, or No to cancel."
        };

        private static string GetCustomScriptModeText() => UILangProviderM.Current.LanguageCode switch
        {
            "zh-cn" => "导入自定义脚本",
            "zh-tw" => "導入自定義腳本",
            _ => "Import custom script"
        };
    }
}
