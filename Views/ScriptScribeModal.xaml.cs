using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OneColumnEncoder.Helpers;

namespace OneColumnEncoder.Views
{
    public partial class ScriptScribeModal : AdaptiveWindow
    {
        public ScriptScribeModal()
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
    }
}
