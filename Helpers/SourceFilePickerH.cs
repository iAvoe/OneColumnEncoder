using Microsoft.Win32;
using OneColumnEncoder.Commands;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System.IO;
using System.Runtime.InteropServices;
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
        private static SourceFilePickerLangProviderM Lang =>
            new(UILangProviderM.Current.LanguageCode);

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

        public static string[] GetVideoFilesInFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                return [];

            string[] extensions = SourceFilePickerLangProviderM.VideoExtensions
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(extension => extension.TrimStart('*').ToLowerInvariant())
                .ToArray();

            return [.. Directory.EnumerateFiles(folderPath)
                .Where(filePath => extensions.Contains(Path.GetExtension(filePath).ToLowerInvariant()))
                .OrderBy(filePath => filePath, NaturalFilePathComparer.Instance)];
        }

        private sealed class NaturalFilePathComparer : IComparer<string>
        {
            public static NaturalFilePathComparer Instance { get; } = new();

            public int Compare(string? x, string? y)
            {
                string xName = Path.GetFileName(x ?? string.Empty);
                string yName = Path.GetFileName(y ?? string.Empty);
                int result = StrCmpLogicalW(xName, yName);
                return result != 0
                    ? result
                    : StringComparer.OrdinalIgnoreCase.Compare(x, y);
            }

            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            private static extern int StrCmpLogicalW(string x, string y);
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

            if (!string.IsNullOrWhiteSpace(foundPath))
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

        private static string GetFilter(SourceFileKind fileKind)
        {
            SourceFilePickerLangProviderM lang = Lang;

            return fileKind switch
            {
                SourceFileKind.Video => lang.VideoFilter,
                SourceFileKind.AviSynthScript => lang.AviSynthScriptFilter,
                SourceFileKind.VapourSynthScript => lang.VapourSynthScriptFilter,
                SourceFileKind.SvfiIni => lang.SvfiIniFilter,
                _ => lang.AllFilesFilter
            };
        }

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
            ConfirmationVM vm = ConfirmationVM.CreateWarning(title, message, cancelCmd, confirmCmd);

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => modalNavS.Close();
            modalNavS.CurrentModalVM = vm;
            window.ShowDialog();

            return result;
        }

        private static string GetNoFileSelectedTitle() => Lang.NoFileSelectedTitle;

        private static string GetMissingSelectionMessage() => Lang.MissingSelectionMessage;

        private static string GetCustomScriptModeText() => Lang.CustomScriptModeText;
    }
}
