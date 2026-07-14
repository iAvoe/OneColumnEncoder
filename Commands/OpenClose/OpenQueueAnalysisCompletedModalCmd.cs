using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System.Diagnostics;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenQueueAnalysisCompletedModalCmd(
        ModalNavS modalNavS,
        string message,
        string queueJsonPath,
        string? excludedJsonPath) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly string _message = message;
        private readonly string _queueJsonPath = queueJsonPath;
        private readonly string? _excludedJsonPath = excludedJsonPath;

        public override void Execute(object? parameter)
        {
            ConfirmationModal window = new();
            CloseModalCmd closeCmd = new(window.Close);
            ConfirmationVM vm = ConfirmationVM.CreateSuccess(
                UILangProvider.SrcAnalysisWindowTitle,
                _message,
                closeCmd,
                closeCmd);

            AddJsonActions(vm, _queueJsonPath, "SourceQueue.OpenQueueJson", "SourceQueue.CopyQueueJsonPath");
            if (!string.IsNullOrWhiteSpace(_excludedJsonPath))
                AddJsonActions(vm, _excludedJsonPath, "SourceQueue.OpenExcludedJson", "SourceQueue.CopyExcludedJsonPath");

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.ShowDialog();
        }

        private static void AddJsonActions(ConfirmationVM vm, string jsonPath, string openTextKey, string copyTextKey)
        {
            vm.ContextMenuItems.Add(new(
                UILangProvider.Current[openTextKey],
                new ActionCmd(_ => OpenJsonPath(jsonPath))));
            vm.ContextMenuItems.Add(new(
                UILangProvider.Current[copyTextKey],
                new ActionCmd(_ => Clipboard.SetText(jsonPath))));
        }

        private static void OpenJsonPath(string jsonPath) =>
            Process.Start(new ProcessStartInfo(jsonPath) { UseShellExecute = true });
    }
}
