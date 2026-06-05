using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components
{
    /// <summary>
    /// Show heatmap example from cold to hot, with per-category swatches.
    /// Usage:
    /// <comps:HeatmapLegend
    ///     ColdText="{Binding ColdLabel}"
    ///     HotText="{Binding HotLabel}"
    ///     UpstreamLabel="{Binding UpstreamLabel}"
    ///     DownstreamLabel="{Binding DownstreamLabel}"
    ///     OtherLabel="{Binding OtherLabel}"
    ///     CacheLabel="{Binding CacheLabel}" />
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

        public string UpstreamLabel
        {
            get => (string)GetValue(UpstreamLabelProperty);
            set => SetValue(UpstreamLabelProperty, value);
        }
        public static readonly DependencyProperty UpstreamLabelProperty =
            DependencyProperty.Register(
                nameof(UpstreamLabel),
                typeof(string),
                typeof(HeatmapLegend),
                new PropertyMetadata(string.Empty));

        public string DownstreamLabel
        {
            get => (string)GetValue(DownstreamLabelProperty);
            set => SetValue(DownstreamLabelProperty, value);
        }
        public static readonly DependencyProperty DownstreamLabelProperty =
            DependencyProperty.Register(
                nameof(DownstreamLabel),
                typeof(string),
                typeof(HeatmapLegend),
                new PropertyMetadata(string.Empty));

        public string OtherLabel
        {
            get => (string)GetValue(OtherLabelProperty);
            set => SetValue(OtherLabelProperty, value);
        }
        public static readonly DependencyProperty OtherLabelProperty =
            DependencyProperty.Register(
                nameof(OtherLabel),
                typeof(string),
                typeof(HeatmapLegend),
                new PropertyMetadata(string.Empty));

        public string CacheLabel
        {
            get => (string)GetValue(CacheLabelProperty);
            set => SetValue(CacheLabelProperty, value);
        }
        public static readonly DependencyProperty CacheLabelProperty =
            DependencyProperty.Register(
                nameof(CacheLabel),
                typeof(string),
                typeof(HeatmapLegend),
                new PropertyMetadata(string.Empty));
    }
}
