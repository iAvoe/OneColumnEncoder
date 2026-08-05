using OneColumnEncoder.UI;
using OneColumnEncoder.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace OneColumnEncoder.Views;

public partial class RepartConfModal : AdaptiveWindow
{
    private double _dividerDragPointerOffset;

    public RepartConfModal() => InitializeComponent();

    private void OutputListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is RepartConfVM vm && sender is ListBox listBox)
            vm.SetSelectedOutputs(listBox.SelectedItems.Cast<RepartOutputItemVM>());
    }

    private void DividerThumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is RepartConfVM vm && sender is FrameworkElement { DataContext: RepartDividerItemVM item })
            vm.SelectDividerForInteraction(item);
    }

    private void DividerThumb_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is RepartConfVM vm && sender is FrameworkElement { DataContext: RepartDividerItemVM item })
            vm.SelectDividerForInteraction(item);
    }

    private void TimelineTrack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2 || sender is not FrameworkElement track || DataContext is not RepartConfVM vm || !vm.CanAddEpisode)
            return;

        if (track.ActualWidth <= 0d)
            return;

        double position = Math.Max(0d, Math.Min(1d, e.GetPosition(track).X / track.ActualWidth));
        vm.AddDividerAtPosition(position);
        e.Handled = true;
    }

    private void DividerThumb_DragStarted(object sender, DragStartedEventArgs e)
    {
        _dividerDragPointerOffset = 0d;
        if (DataContext is not RepartConfVM vm
            || sender is not FrameworkElement { DataContext: RepartDividerItemVM item } element
            || FindAncestor<ItemsControl>(element) is not { ActualWidth: > 0d } host)
        {
            return;
        }

        vm.SelectDividerForInteraction(item);
        double dividerX = item.Position * host.ActualWidth;
        _dividerDragPointerOffset = Mouse.GetPosition(host).X - dividerX;
    }

    private void DividerThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (DataContext is not RepartConfVM vm
            || sender is not FrameworkElement { DataContext: RepartDividerItemVM item } element
            || FindAncestor<ItemsControl>(element) is not { ActualWidth: > 0d } host)
        {
            return;
        }

        double pointerX = Mouse.GetPosition(host).X - _dividerDragPointerOffset;
        double position = Math.Max(0d, Math.Min(1d, pointerX / host.ActualWidth));
        vm.MoveDividerToPosition(item, position);
    }

    private void DividerThumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        _dividerDragPointerOffset = 0d;
    }

    private static T? FindAncestor<T>(DependencyObject element) where T : DependencyObject
    {
        DependencyObject? current = element;
        while (current != null)
        {
            if (current is T match) return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
