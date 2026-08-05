using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components;

public sealed class RepartDividerPanel : Panel
{
    public static readonly DependencyProperty PositionProperty = DependencyProperty.RegisterAttached(
        "Position",
        typeof(double),
        typeof(RepartDividerPanel),
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsParentArrange));

    public static void SetPosition(DependencyObject element, double value) => element.SetValue(PositionProperty, value);
    public static double GetPosition(DependencyObject element) => (double)element.GetValue(PositionProperty);

    protected override Size MeasureOverride(Size availableSize)
    {
        double height = double.IsInfinity(availableSize.Height) ? 32d : availableSize.Height;
        foreach (UIElement child in InternalChildren)
            child.Measure(new Size(44d, height));
        return new Size(double.IsInfinity(availableSize.Width) ? DesiredChildrenWidth() : availableSize.Width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (UIElement child in InternalChildren)
        {
            double position = Math.Max(0d, Math.Min(1d, GetPosition(child)));
            double width = child.DesiredSize.Width;
            double left = position * finalSize.Width - width / 2d;
            left = Math.Max(0d, Math.Min(Math.Max(0d, finalSize.Width - width), left));
            child.Arrange(new Rect(left, 0d, width, finalSize.Height));
        }
        return finalSize;
    }

    private double DesiredChildrenWidth() => InternalChildren.Cast<UIElement>().Sum(child => Math.Max(44d, child.DesiredSize.Width));
}
