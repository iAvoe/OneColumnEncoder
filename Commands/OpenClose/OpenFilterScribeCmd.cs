namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Opens the filter script scribe modal for editing AVS/Vpy filter scripts.
/// </summary>
public class OpenFilterScribeCmd(
    ModalNavS modalNavS,
    Func<string> getSourcePath,
    Func<ToolItemCardVM> getAvsItem,
    Func<ToolItemCardVM> getVpyItem,
    Func<SourceFileKind?> getPreferredScriptSourceKind,
    Action<ToolItemCardVM, SourceFileKind, string> afterImport, // File save & ItemCard write back
    Action<string?> applyFfmpegFilterArgs,
    Func<bool> hasSourceValidationError,
    Func<bool> hasSarRepairWarning,
    Func<string?> getSourceFfprobeJson,
    Func<SourceRevisionRequest, string?> sourceReviser,
    Func<bool> isOneLineShotSelected,
    Func<bool>? isQueueRoute = null,
    Func<string[]>? getQueueFilePaths = null,
    Func<bool>? isConcatRoute = null,
    Func<string[]>? getConcatFilePaths = null,
    Action<string[]>? applyConcatFilePaths = null,
    Func<bool>? isRepartRoute = null,
    Action<string?, string?>? applyScriptFilters = null,
    Func<RepartPlanM?>? getRepartPlan = null,
    Action<Guid[]>? applyRepartOutputOrder = null,
    string? vspipePath = null,
    string? vspipeY4mArg = null,
    Func<long>? getTotalFrames = null) : OpenCloseBase(modalNavS)
{
    /// <summary>
    /// Shows a warning if a one-line shot is selected; otherwise brings an existing
    /// window to the front or shows the filter scribe modal.
    /// </summary>
    public override void Execute(object? parameter)
    {
        if (isOneLineShotSelected())
        {
            ConfirmationModal warnWindow = new();
            CloseModalCmd closeCmd = new(warnWindow.Close);
            ConfirmationVM warnVm = ConfirmationVM.CreateWarning(
                UICaptionProvider.Buttons.OpenScribeSrcScribe,
                UICaptionProvider.Hints.FilterScribeDisabled,
                closeCmd, closeCmd);

            ShowModal(warnWindow, warnVm, showDialog: true, closeOpenStack: true);
            return;
        }

        if (TryActivateExistingWindow<FilterScribeModal>())
            return;

        FilterScribeModal window = new();
        FilterScribeVM vm = new(
            ModalNavS,
            window.Close,
            getSourcePath,
            getAvsItem(), getVpyItem(),
            getPreferredScriptSourceKind,
            afterImport,
            applyFfmpegFilterArgs,
            hasSourceValidationError,
            hasSarRepairWarning,
            getSourceFfprobeJson(),
            sourceReviser,
            isQueueRoute,
            getQueueFilePaths,
            isConcatRoute,
            getConcatFilePaths,
            applyConcatFilePaths,
            isRepartRoute,
            applyScriptFilters,
            getRepartPlan,
            applyRepartOutputOrder,
            vspipePath,
            vspipeY4mArg,
            getTotalFrames);
        ShowModal(window, vm, closeOpenStack: true);
    }
}
