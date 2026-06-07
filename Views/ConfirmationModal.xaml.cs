using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Helpers;

namespace OneColumnEncoder.Views
{
    /// <summary>
    /// Interaction logic for WarnErrModal.xaml
    /// </summary>
    public partial class ConfirmationModal : AdaptiveWindow
    {
        public ConfirmationModal()
        {
            InitializeComponent();
        }

        private void CopyMessage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext is ConfirmationVM vm
                && !string.IsNullOrWhiteSpace(vm.P1Text);
        }

        private void CopyMessage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is ConfirmationVM vm)
                Clipboard.SetText(vm.P1Text);
        }
    }
}
