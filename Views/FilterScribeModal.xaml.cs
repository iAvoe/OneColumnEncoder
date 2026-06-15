using OneColumnEncoder.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OneColumnEncoder.Views
{
    public partial class FilterScribeModal : AdaptiveWindow
    {
        public FilterScribeModal()
        {
            InitializeComponent();
        }

        private void UserInput_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;

            var box = (TextBox)sender;
            double newSize = box.FontSize + (e.Delta > 0 ? 1 : -1);
            box.FontSize = double.Clamp(newSize, 8, 48);
            e.Handled = true;
        }

        private void FfmpegFreeText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Return)
                return;

            e.Handled = true;
        }

        private void FfmpegFreeText_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            string? text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string
                ?? e.SourceDataObject.GetData(DataFormats.Text) as string;

            if (text is null)
                return;

            string normalized = text.Replace("\r", string.Empty).Replace("\n", string.Empty);
            if (normalized == text)
                return;

            DataObject dataObject = new DataObject();
            dataObject.SetData(DataFormats.UnicodeText, normalized);
            e.DataObject = dataObject;
        }
    }
}
