using OneColumnEncoder.ConcatManagement;
using System.IO;

namespace OneColumnEncoder.Commands.Browse;

public class BrowseSrcConcatCmd(
    ToolItemCardVM item,
    ModalNavS modalNavS,
    Func<string> getFfprobePath,
    Func<bool>? isSvtav1SelectedFunc = null, // SVT-AV1 does not support 12bit, check needed
    AppDataM? appDataM = null,
    string? browseKey = null,
    Action<ToolItemCardVM, string[]>? afterImport = null) : BrowseCmdBase(item)
{
    private const int MinimumConcatSourceCount = 2;
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
            : FileManagementProviderM.GetInitialDirectory(_item.P2TextData);

        OpenFileDialog dialog = new()
        {
            Title = UILangProvider.Current["SourceConcat.SelectFilesTitle"],
            Multiselect = true,
            Filter = new SrcFilePickerLangProvider(UILangProvider.Current.LanguageCode).VideoFilter,
            InitialDirectory = initialDirectory
        };

        if (ShowDialog(dialog) != true) return;

        string[] filePaths = dialog.FileNames;
        if (filePaths.Length == 0) return;

        // Rejection is ffprobe-based: files that ffprobe cannot read are excluded
        // by the compatibility analysis below, no extension pre-filter runs here.

        // Concat mode needs at least two sources to build a meaningful file list.
        if (filePaths.Length < MinimumConcatSourceCount)
        {
            new OpenErrModalCmd(
                _modalNavS,
                FilterScribeModalLangProvider.Current["SrcScribe.ConcatNeedMultipleSourcesTitle"],
                FilterScribeModalLangProvider.Current["SrcScribe.ConcatNeedMultipleSources"]).Execute(null);
            ActivateMainWindow();
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
            ActivateMainWindow();
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
            ActivateMainWindow();
            return;
        }

        if (analysisResult is not null && analysisResult.HasResolutionMismatch)
        {
            new OpenErrModalCmd(
                _modalNavS,
                UICaptionProvider.SourceInspect.ErrorTitle,
                analysisResult.ResolutionMismatchMessage ?? string.Empty).Execute(null);
            ActivateMainWindow();
            return;
        }

        // Compatible part of the selection stays in the import; files that ffprobe
        // could not read are excluded and reported in one combined summary.
        string[] acceptedPaths = analysisResult is not null
            ? [.. analysisResult.RawAnalyses.Select(entry => entry.FilePath)]
            : filePaths;

        List<string> warningSections = [];
        if (analysisResult is not null && analysisResult.ProbeFailures.Count > 0)
            warningSections.Add(SrcProbeGate.FormatFailures(analysisResult.ProbeFailures));
        if (analysisResult is not null && analysisResult.VariableFrameRateWarnings.Count > 0)
            warningSections.Add(string.Join(
                Environment.NewLine + Environment.NewLine,
                analysisResult.VariableFrameRateWarnings));

        if (warningSections.Count > 0)
        {
            new OpenWarnModalCmd(
                _modalNavS,
                UICaptionProvider.SourceInspect.WarnTitle,
                string.Join(Environment.NewLine + Environment.NewLine, warningSections)).Execute(null);
            ActivateMainWindow();
        }

        // Store the parent folder of the accepted files and show a compact, ordered
        // summary in the import card, so the card never points at an excluded file.
        string parentDir = Path.GetDirectoryName(acceptedPaths[0]) ?? string.Empty;
        SetQueueCardText(parentDir, acceptedPaths);
        Remember(_appDataM, _browseKey, acceptedPaths[0]);
        _afterImport?.Invoke(_item, acceptedPaths);
        ActivateMainWindow();
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

    private static string FormatExtension(string extension) =>
        string.IsNullOrWhiteSpace(extension) ? "(none)" : extension;
}
