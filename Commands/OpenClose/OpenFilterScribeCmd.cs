using OneColumnEncoder.Helpers;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.ViewModels.Cards;
using OneColumnEncoder.Views;
using System;
using System.Linq;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenFilterScribeCmd(
        ModalNavS modalNavS,
        Func<string> getSourcePath,
        ToolItemCardVM avsItem,
        ToolItemCardVM vpyItem,
        Action<ToolItemCardVM, SourceFileKind, string> afterImport, // File save & ItemCard write back
        Action<string?> applyFfmpegFilterArgs,
        Func<string?> getSourceFfprobeJson) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        public override void Execute(object? parameter)
        {
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
                avsItem, vpyItem,
                afterImport,
                applyFfmpegFilterArgs,
                getSourceFfprobeJson());
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.Show();
        }
    }
}
