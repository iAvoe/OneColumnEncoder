namespace OneColumnEncoder.Commands.OpenClose
{
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
        Func<long>? getTotalFrames = null) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        public override void Execute(object? parameter)
        {
            if (isOneLineShotSelected())
            {
                if (_modalNavS.IsOpen) _modalNavS.Close();

                ConfirmationModal warnWindow = new();
                CloseModalCmd closeCmd = new(warnWindow.Close);
                ConfirmationVM warnVm = ConfirmationVM.CreateWarning(
                    UICaptionProvider.Buttons.OpenScribeSrcScribe,
                    UICaptionProvider.Hints.FilterScribeDisabled,
                    closeCmd, closeCmd);

                warnWindow.DataContext = warnVm;
                warnWindow.Owner = Application.Current.MainWindow;
                warnWindow.Closed += (_, _) => _modalNavS.Close();
                _modalNavS.CurrentModalVM = warnVm;
                warnWindow.ShowDialog();
                return;
            }

            var existingWindow = Application.Current.Windows
                .OfType<FilterScribeModal>()
                .FirstOrDefault();

            if (existingWindow != null)
            {
                existingWindow.Activate();
                return;
            }

            if (_modalNavS.IsOpen) _modalNavS.Close();

            FilterScribeModal window = new();
            FilterScribeVM vm = new(
                _modalNavS,
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
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            window.Show();
            _modalNavS.CurrentModalVM = vm;
        }
    }
}
