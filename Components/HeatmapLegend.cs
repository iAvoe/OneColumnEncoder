using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components
{
    /// <summary>
    /// Show heatmap example from cold to hot
    /// Usage:
    /// <comps:HeatmapLegend ColdText="{Binding ColdLabel}" HotText="{Binding HotLabel}" />
    /// </summary>
    public class HeatmapLegend : Control
    {
        static HeatmapLegend()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HeatmapLegend),
                new FrameworkPropertyMetadata(typeof(HeatmapLegend)));
        }

        public string ColdText
        {
            get => (string)GetValue(ColdTextProperty);
            set => SetValue(ColdTextProperty, value);
        }
        public static readonly DependencyProperty ColdTextProperty =
            DependencyProperty.Register(
                nameof(ColdText),
                typeof(string),
                typeof(HeatmapLegend),
                new PropertyMetadata(string.Empty));

        public string HotText
        {
            get => (string)GetValue(HotTextProperty);
            set => SetValue(HotTextProperty, value);
        }
        public static readonly DependencyProperty HotTextProperty =
            DependencyProperty.Register(
                nameof(HotText),
                typeof(string),
                typeof(HeatmapLegend),
                new PropertyMetadata(string.Empty));
    }
}
