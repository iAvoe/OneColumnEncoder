using OneColumnEncoder.ConcatManagement;
using System.IO;

namespace OneColumnEncoder.Commands;

public class BrowseSrcConcatCmd(
    ToolItemCardVM item,
    ModalNavS modalNavS,
    Func<string> getFfprobePath,
    Func<bool>? isSvtav1SelectedFunc = null, // SVT-AV1 does not support 12bit, check needed
    AppDataM? appDataM = null,
    string? browseKey = null,
    Action<ToolItemCardVM, string[]>? afterImport = null) : BaseCmd
{
    private readonly ToolItemCardVM _item = item;
    private readonly ModalNavS _modalNavS = modalNavS;
    private readonly Func<string> _getFfprobePath = getFfprobePath;
    private readonly Func<bool>? _isSvtav1SelectedFunc = isSvtav1SelectedFunc;
    private readonly AppDataM? _appDataM = appDataM;
    private readonly string? _browseKey = browseKey;
    private readonly Action<ToolItemCardVM, string[]>? _afterImport = afterImport;

    public override async void Execute(object? parameter)
    {
        // Use the shared video source filter so concat import only presents supported media files.
        string initialDirectory = _browseKey != null && _appDataM != null
            ? BrowseHistory.ResolveInitialDirectory(_appDataM, _browseKey, _item.P2TextData)
            : OutputPath.GetInitialDirectory(_item.P2TextData);

        OpenFileDialog dialog = new()
        {
            Title = UILangProvider.Current["SourceConcat.SelectFilesTitle"],
            Multiselect = true,
            Filter = new SrcFilePickerLangProvider(UILangProvider.Current.LanguageCode).VideoFilter,
            InitialDirectory = initialDirectory
        };

        Window? owner = Application.Current.MainWindow;
        bool? result = owner is null
            ? dialog.ShowDialog()
            : dialog.ShowDialog(owner);
        if (result != true) return;

        string[] filePaths = dialog.FileNames;
        if (filePaths.Length == 0) return;

        // Reject unsupported extensions even if the user switches the dialog to "All files".
        string? unsupportedExtensionError = GetUnsupportedExtensionMessage(filePaths);
        if (unsupportedExtensionError != null)
        {
            new OpenErrModalCmd(
                _modalNavS,
                UICaptionProvider.SourceInspect.WarnTitle,
                unsupportedExtensionError).Execute(null);
            Application.Current.MainWindow?.Activate();
            return;
        }

        // Concat mode needs at least two sources to build a meaningful file list.
        if (filePaths.Length < 2)
        {
            new OpenErrModalCmd(
                _modalNavS,
                FilterScribeModalLangProvider.Current["SrcScribe.ConcatNeedMultipleSourcesTitle"],
                FilterScribeModalLangProvider.Current["SrcScribe.ConcatNeedMultipleSources"]).Execute(null);
            Application.Current.MainWindow?.Activate();
            return;
        }

        // Keep all concat inputs on the same container/extension before deeper compatibility checks.
        string? extensionError = GetExtensionMismatchMessage(filePaths);
        if (extensionError != null)
        {
            new OpenErrModalCmd(
                _modalNavS,
                UICaptionProvider.SourceInspect.WarnTitle,
                extensionError).Execute(null);
            Application.Current.MainWindow?.Activate();
            return;
        }

        // Probe the selected files for codec, resolution, frame rate, and SVT-AV1 constraints.
        ConcatCompatibilityAnalysisResult? analysisResult = null;
        try
        {
            analysisResult = await ConcatCompatibilityAnalyzer.AnalyzeAsync(
                _getFfprobePath(),
                filePaths,
                _isSvtav1SelectedFunc);
        }
        catch (Exception ex)
        {
            new OpenErrModalCmd(
                _modalNavS,
                UICaptionProvider.SourceInspect.WarnTitle,
                ex.Message).Execute(null);
            Application.Current.MainWindow?.Activate();
            return;
        }

        if (analysisResult is not null && analysisResult.HasResolutionMismatch)
        {
            new OpenErrModalCmd(
                _modalNavS,
                UICaptionProvider.SourceInspect.ErrorTitle,
                analysisResult.ResolutionMismatchMessage ?? string.Empty).Execute(null);
            Application.Current.MainWindow?.Activate();
            return;
        }

        if (analysisResult is not null && analysisResult.VariableFrameRateWarnings.Count > 0)
        {
            string warningMessage = string.Join(
                Environment.NewLine + Environment.NewLine,
                analysisResult.VariableFrameRateWarnings);
            new OpenWarnModalCmd(
                _modalNavS,
                UICaptionProvider.SourceInspect.WarnTitle,
                warningMessage).Execute(null);
            Application.Current.MainWindow?.Activate();
        }

        // Store the parent folder and show a compact, ordered file summary in the import card.
        string parentDir = Path.GetDirectoryName(filePaths[0]) ?? string.Empty;
        string[] fileNames = [.. filePaths.Select(Path.GetFileName).Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n!)];

        _item.P2TextData = parentDir;
        _item.P1TextData = BrowseSrcQueueCmd.FormatQueueP1Text(fileNames);
        _item.P1TooltipText = BrowseSrcQueueCmd.FormatQueueP1TooltipText(fileNames);
        if (_browseKey != null && _appDataM != null)
            BrowseHistory.Remember(_appDataM, _browseKey, filePaths[0]);
        _afterImport?.Invoke(_item, filePaths);
        Application.Current.MainWindow?.Activate();
    }

    private static string? GetExtensionMismatchMessage(string[] filePaths)
    {
        if (filePaths.Length < 2) return null;

        // The concat demuxer path expects every selected source to use the same extension.
        string expectedExtension = Path.GetExtension(filePaths[0]) ?? string.Empty;
        string[] mismatched = [.. filePaths
            .Where(path => !string.Equals(
                Path.GetExtension(path) ?? string.Empty,
                expectedExtension,
                StringComparison.OrdinalIgnoreCase))];
        if (mismatched.Length == 0) return null;

        string expectedLabel = FormatExtension(expectedExtension);
        string mismatchedList = string.Join(
            Environment.NewLine,
            mismatched.Select(path => $"- {Path.GetFileName(path)} ({FormatExtension(Path.GetExtension(path) ?? string.Empty)})"));
        return string.Format(UILangProvider.Current["SourceConcat.ExtensionMismatch"], expectedLabel, mismatchedList);
    }

    private static string? GetUnsupportedExtensionMessage(string[] filePaths)
    {
        // Normalize picker patterns like "*.mkv" to extensions like ".mkv" for lookup.
        HashSet<string> videoExtensions = [.. SrcFilePickerLangProvider.VideoExtensions
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(extension => extension.TrimStart('*').ToLowerInvariant())];

        string[] unsupported = [.. filePaths
            .Where(path => !videoExtensions.Contains((Path.GetExtension(path) ?? string.Empty).ToLowerInvariant()))];
        if (unsupported.Length == 0) return null;

        string unsupportedList = string.Join(
            Environment.NewLine,
            unsupported.Select(path => $"- {Path.GetFileName(path)} ({FormatExtension(Path.GetExtension(path) ?? string.Empty)})"));
        return string.Format(UILangProvider.Current["SourceConcat.ExtensionMismatch"], SrcFilePickerLangProvider.VideoExtensions, unsupportedList);
    }

    private static string FormatExtension(string extension) =>
        string.IsNullOrWhiteSpace(extension) ? "(none)" : extension;
}
