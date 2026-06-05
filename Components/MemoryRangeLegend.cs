using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components
{
    /// <summary>
    /// Shows memory range categories used by the proportional memory occupancy bar.
    /// </summary>
    public class MemoryRangeLegend : Control
    {
        static MemoryRangeLegend()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MemoryRangeLegend),
                new FrameworkPropertyMetadata(typeof(MemoryRangeLegend)));
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(MemoryRangeLegend),
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
                typeof(MemoryRangeLegend),
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
                typeof(MemoryRangeLegend),
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
                typeof(MemoryRangeLegend),
                new PropertyMetadata(string.Empty));

        public string AvailableLabel
        {
            get => (string)GetValue(AvailableLabelProperty);
            set => SetValue(AvailableLabelProperty, value);
        }
        public static readonly DependencyProperty AvailableLabelProperty =
            DependencyProperty.Register(
                nameof(AvailableLabel),
                typeof(string),
                typeof(MemoryRangeLegend),
                new PropertyMetadata(string.Empty));
    }
}
