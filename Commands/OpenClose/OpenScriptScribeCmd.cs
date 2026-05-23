using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System;
using System.Linq;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenScriptScribeCmd(ModalNavS modalNavS, Func<string> getSourcePath) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        public override void Execute(object? parameter)
        {
            var existingWindow = Application.Current.Windows
                .OfType<ScriptSrcScribeModal>()
                .FirstOrDefault();

            if (existingWindow != null)
            {
                existingWindow.Activate();
                return;
            }

            if (_modalNavS.IsOpen)
                _modalNavS.Close();

            ScriptSrcScribeModal window = new();
            ScriptSrcScribeModalVM vm = new(_modalNavS, window.Close, getSourcePath);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.Show();
        }
    }
}
