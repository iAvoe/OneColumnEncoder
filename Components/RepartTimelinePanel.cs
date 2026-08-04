using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components;

public sealed class RepartTimelinePanel : Panel
{
    public static readonly DependencyProperty WeightProperty = DependencyProperty.RegisterAttached(
        "Weight",
        typeof(double),
        typeof(RepartTimelinePanel),
        new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));

    public static void SetWeight(DependencyObject element, double value) => element.SetValue(WeightProperty, value);
    public static double GetWeight(DependencyObject element) => (double)element.GetValue(WeightProperty);

    protected override Size MeasureOverride(Size availableSize)
    {
        double height = double.IsInfinity(availableSize.Height) ? 54d : availableSize.Height;
        double totalWeight = GetTotalWeight();
        foreach (UIElement child in InternalChildren)
        {
            double width = double.IsInfinity(availableSize.Width)
                ? Math.Max(24d, GetWeight(child))
                : availableSize.Width * GetWeight(child) / totalWeight;
            child.Measure(new Size(Math.Max(1d, width), height));
        }
        return new Size(double.IsInfinity(availableSize.Width) ? DesiredChildrenWidth() : availableSize.Width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double totalWeight = GetTotalWeight();
        double offset = 0d;
        for (int i = 0; i < InternalChildren.Count; i++)
        {
            UIElement child = InternalChildren[i];
            double width = i == InternalChildren.Count - 1
                ? Math.Max(0d, finalSize.Width - offset)
                : finalSize.Width * GetWeight(child) / totalWeight;
            double effectiveWidth = Math.Max(1d, width);
            child.Arrange(new Rect(offset, 0d, effectiveWidth, finalSize.Height));
            offset += effectiveWidth;
        }
        return finalSize;
    }

    private double GetTotalWeight() => Math.Max(1d, InternalChildren.Cast<UIElement>().Sum(child => Math.Max(0d, GetWeight(child))));
    private double DesiredChildrenWidth() => InternalChildren.Cast<UIElement>().Sum(child => Math.Max(24d, child.DesiredSize.Width));
}
