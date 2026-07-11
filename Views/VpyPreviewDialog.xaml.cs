using OneColumnEncoder.Stores;
using OneColumnEncoder.UI;
using OneColumnEncoder.ViewModels;
using System.Windows;

namespace OneColumnEncoder.Views
{
    public partial class VpyPreviewDialog : AdaptiveWindow
    {
        public VpyPreviewDialog(VspipePreviewVM vm, ModalNavS modalNavS)
        {
            InitializeComponent();
            DataContext = vm;
            Owner = Application.Current.MainWindow;

            Closed += (_, _) =>
            {
                vm.Dispose();
                modalNavS.Close();
            };

            Loaded += (_, _) =>
            {
                modalNavS.CurrentModalVM = vm;
            };
        }
    }
}
