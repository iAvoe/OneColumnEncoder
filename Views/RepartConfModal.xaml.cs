using OneColumnEncoder.UI;
using OneColumnEncoder.ViewModels;
using System.Windows.Controls;

namespace OneColumnEncoder.Views;

public partial class RepartConfModal : AdaptiveWindow
{
    public RepartConfModal() => InitializeComponent();

    private void OutputListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is RepartConfVM vm && sender is ListBox listBox)
            vm.SetSelectedOutputs(listBox.SelectedItems.Cast<RepartOutputItemVM>());
    }
}
