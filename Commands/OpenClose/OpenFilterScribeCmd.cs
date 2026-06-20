using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.ViewModels.Cards;
using OneColumnEncoder.Views;
using System.Windows;

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
        Func<bool> isOneLineShotSelected,
        Func<bool>? isQueueRoute = null,
        Func<string[]>? getQueueFilePaths = null) : BaseCmd
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
                    UICaptionProviderM.Buttons.OpenScribeSrcScribe,
                    UICaptionProviderM.Hints.FilterScribeDisabled,
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
                isQueueRoute,
                getQueueFilePaths);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.Show();
        }
    }
}
