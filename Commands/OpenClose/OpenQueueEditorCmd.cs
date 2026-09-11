namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Opens the shared source or Repart-output queue editor. Confirm applies the displayed
/// order through the callback; Cancel leaves the caller's current queue unchanged.
/// </summary>
public sealed class OpenQueueEditorCmd(
    ModalNavS modalNavS,
    Action<string[]>? applyEditedPaths = null,
    int minimumItemCount = 0,
    bool disableSortButtons = false,
    Action<Guid[]>? applyEditedOutputIds = null,
    int outputFrameRateNumerator = 0,
    int outputFrameRateDenominator = 1) : OpenCloseBase(modalNavS)
{
    private readonly Action<string[]>? _applyEditedPaths = applyEditedPaths;
    private readonly int _minimumItemCount = minimumItemCount;
    private readonly bool _disableSortButtons = disableSortButtons;
    private readonly Action<Guid[]>? _applyEditedOutputIds = applyEditedOutputIds;
    private readonly int _outputFrameRateNumerator = outputFrameRateNumerator;
    private readonly int _outputFrameRateDenominator = outputFrameRateDenominator;

    public bool IsOutputMode => _applyEditedOutputIds != null;

    public static string[] EditFilePaths(
        ModalNavS modalNavS,
        IEnumerable<string> filePaths,
        int minimumItemCount = 0,
        bool disableSortButtons = false)
    {
        string[] editedFilePaths = [.. filePaths];
        new OpenQueueEditorCmd(modalNavS, paths => editedFilePaths = paths, minimumItemCount, disableSortButtons)
            .Execute(editedFilePaths);
        return editedFilePaths;
    }

    public static void EditOutputOrder(
        ModalNavS modalNavS,
        IEnumerable<RepartOutputSegmentM> outputSegments,
        int frameRateNumerator,
        int frameRateDenominator,
        Action<Guid[]> applyEditedOutputIds)
    {
        RepartOutputSegmentM[] segments = [.. outputSegments];
        new OpenQueueEditorCmd(
            modalNavS,
            applyEditedOutputIds: applyEditedOutputIds,
            outputFrameRateNumerator: frameRateNumerator,
            outputFrameRateDenominator: frameRateDenominator)
            .Execute(segments);
    }

    public override void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<QueueEditorModal>())
            return;

        QueueEditorModal window = new();
        QueueEditorVM vm;
        if (IsOutputMode)
        {
            if (parameter is not RepartOutputSegmentM[] outputSegments || outputSegments.Length == 0)
                return;
            vm = new QueueEditorVM(
                window.Close,
                outputSegments,
                _outputFrameRateNumerator,
                _outputFrameRateDenominator,
                _applyEditedOutputIds!);
        }
        else
        {
            if (parameter is not string[] filePaths || filePaths.Length == 0)
                return;
            vm = new QueueEditorVM(window.Close, filePaths, _applyEditedPaths!, _minimumItemCount, _disableSortButtons);
        }
        ShowModal(window, vm, showDialog: true);
    }
}
