using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components
{
    /// <summary>
    /// Show heatmap example from cold to hot, with per-category swatches.
    /// Usage:
    /// <comps:HeatmapLegend
    ///     ColdToHotText="{Binding ColdToHotLabel}"
    ///     UpstreamLabel="{Binding UpstreamLabel}"
    ///     DownstreamLabel="{Binding DownstreamLabel}"
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

        public string ColdToHotText
        {
            get => (string)GetValue(ColdToHotTextProperty);
            set => SetValue(ColdToHotTextProperty, value);
        }
        public static readonly DependencyProperty ColdToHotTextProperty =
            DependencyProperty.Register(
                nameof(ColdToHotText),
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
