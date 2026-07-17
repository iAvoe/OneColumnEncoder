using OneColumnEncoder.UI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OneColumnEncoder.Views
{
    public partial class SourceReviserModal : AdaptiveWindow
    {
        public SourceReviserModal()
        {
            InitializeComponent();
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void NumericTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            string? pastedText = e.DataObject.GetData(DataFormats.Text) as string;
            if (!string.IsNullOrEmpty(pastedText) && !pastedText.All(char.IsDigit))
                e.CancelCommand();
        }
    }
}
