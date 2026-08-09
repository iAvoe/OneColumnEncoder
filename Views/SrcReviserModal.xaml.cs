namespace OneColumnEncoder.Views;

public partial class SrcReviserModal : AdaptiveWindow
{
    public SrcReviserModal()
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
