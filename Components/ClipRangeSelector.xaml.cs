using OneColumnEncoder.ViewModels;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace OneColumnEncoder.Components
{
    public partial class ClipRangeSelector : UserControl
    {
        private const double MinRange = 0.01;

        private bool _ignoreSelectionUpdate;
        private bool _isDraggingSelection;
        private INotifyCollectionChanged? _axisLabelsNotify;

        public ClipRangeSelector()
        {
            InitializeComponent();
            Loaded += (_, _) => UpdateLayoutMetrics();
            SizeChanged += (_, _) => UpdateLayoutMetrics();
        }

        public static readonly DependencyProperty AxisLabelsProperty = DependencyProperty.Register(
            nameof(AxisLabels),
            typeof(IEnumerable<string>),
            typeof(ClipRangeSelector),
            new PropertyMetadata(Array.Empty<string>(), OnAxisLabelsChanged));

        public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(
            nameof(SelectionStart),
            typeof(double),
            typeof(ClipRangeSelector),
            new FrameworkPropertyMetadata(0.15d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectionChanged));

        public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(
            nameof(SelectionEnd),
            typeof(double),
            typeof(ClipRangeSelector),
            new FrameworkPropertyMetadata(0.85d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectionChanged));

        public static readonly DependencyProperty TrackHeightProperty = DependencyProperty.Register(
            nameof(TrackHeight),
            typeof(double),
            typeof(ClipRangeSelector),
            new PropertyMetadata(20d));

        public static readonly DependencyProperty ThumbWidthProperty = DependencyProperty.Register(
            nameof(ThumbWidth),
            typeof(double),
            typeof(ClipRangeSelector),
            new PropertyMetadata(16d));

        public static readonly DependencyProperty ThumbHeightProperty = DependencyProperty.Register(
            nameof(ThumbHeight),
            typeof(double),
            typeof(ClipRangeSelector),
            new PropertyMetadata(28d));

        public static readonly DependencyProperty LabelMarginTopProperty = DependencyProperty.Register(
            nameof(LabelMarginTop),
            typeof(double),
            typeof(ClipRangeSelector),
            new PropertyMetadata(8d));

        public IEnumerable<string> AxisLabels
        {
            get => (IEnumerable<string>)GetValue(AxisLabelsProperty);
            set => SetValue(AxisLabelsProperty, value);
        }

        public double SelectionStart
        {
            get => (double)GetValue(SelectionStartProperty);
            set => SetValue(SelectionStartProperty, value);
        }

        public double SelectionEnd
        {
            get => (double)GetValue(SelectionEndProperty);
            set => SetValue(SelectionEndProperty, value);
        }

        public double TrackHeight
        {
            get => (double)GetValue(TrackHeightProperty);
            private set => SetValue(TrackHeightProperty, value);
        }

        public double ThumbWidth
        {
            get => (double)GetValue(ThumbWidthProperty);
            private set => SetValue(ThumbWidthProperty, value);
        }

        public double ThumbHeight
        {
            get => (double)GetValue(ThumbHeightProperty);
            private set => SetValue(ThumbHeightProperty, value);
        }

        public double LabelMarginTop
        {
            get => (double)GetValue(LabelMarginTopProperty);
            private set => SetValue(LabelMarginTopProperty, value);
        }

        public bool IsDraggingSelection => _isDraggingSelection;

        private static void OnAxisLabelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ClipRangeSelector control = (ClipRangeSelector)d;
            control.UnhookAxisLabels(e.OldValue as IEnumerable<string>);
            control.HookAxisLabels(e.NewValue as IEnumerable<string>);
            control.RefreshAxisLabels();
        }

        private static void OnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ClipRangeSelector control = (ClipRangeSelector)d;
            control.NormalizeSelection();
            control.UpdateSelectionVisuals();
        }

        private void HookAxisLabels(IEnumerable<string>? labels)
        {
            if (labels is INotifyCollectionChanged notify)
            {
                _axisLabelsNotify = notify;
                notify.CollectionChanged += AxisLabels_CollectionChanged;
            }
        }

        private void UnhookAxisLabels(IEnumerable<string>? labels)
        {
            if (_axisLabelsNotify != null)
            {
                _axisLabelsNotify.CollectionChanged -= AxisLabels_CollectionChanged;
                _axisLabelsNotify = null;
            }
        }

        private void AxisLabels_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshAxisLabels();
        }

        private void RefreshAxisLabels()
        {
            AxisLabelsHost.Children.Clear();
            IEnumerable<string>? labels = AxisLabels;
            int count = labels?.Count() ?? 0;
            AxisLabelsHost.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (count <= 0) return;

            foreach (string label in labels!)
            {
                AxisLabelsHost.Children.Add(new TextBlock
                {
                    Text = label,
                    FontSize = 10,
                    Foreground = FindResource("GlobalSecondary") as System.Windows.Media.Brush ?? System.Windows.Media.Brushes.Gray,
                });
            }

            LayoutAxisLabels();
        }

        private void LayoutAxisLabels()
        {
            double width = Math.Max(0d, TrackHost.ActualWidth);
            int count = AxisLabelsHost.Children.Count;
            if (count <= 1 || width <= 0d) return;

            for (int i = 0; i < count; i++)
            {
                if (AxisLabelsHost.Children[i] is not TextBlock tb) continue;
                tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double x = (double)i / (count - 1) * width;
                double halfW = tb.DesiredSize.Width / 2d;
                if (i == 0) halfW = 0d;
                else if (i == count - 1) halfW = tb.DesiredSize.Width;
                Canvas.SetLeft(tb, Math.Max(0d, Math.Min(width - tb.DesiredSize.Width, x - halfW)));
                Canvas.SetTop(tb, 0d);
            }
        }

        private static double Clamp(double value, double min, double max) =>
            Math.Max(min, Math.Min(max, value));

        private void UpdateLayoutMetrics()
        {
            double width = Math.Max(0d, TrackHost.ActualWidth);
            if (width <= 0d)
            {
                UpdateSelectionVisuals();
                return;
            }

            double nextThumbWidth = Clamp(width * 0.05d, 16d, 30d);
            double nextLabelMargin = Clamp(width * 0.012d, 4d, 12d);

            TrackHeight = 50d;
            ThumbWidth = nextThumbWidth;
            ThumbHeight = 48d;
            LabelMarginTop = nextLabelMargin;

            SelectionThumb.Width = nextThumbWidth;
            AxisLabelsHost.Margin = new Thickness(0, nextLabelMargin, 0, 0);

            UpdateSelectionVisuals();
            LayoutAxisLabels();
        }

        private void UpdateSelectionVisuals()
        {
            double width = Math.Max(0d, TrackHost.ActualWidth);
            if (width <= 0d)
                return;

            double start = Clamp(SelectionStart, 0d, 1d);
            double end = Clamp(SelectionEnd, 0d, 1d);
            if (end < start)
                (start, end) = (end, start);

            double actualWidth = Math.Max(MinRange, end - start);
            double maxStart = Math.Max(0d, 1d - actualWidth);
            start = Clamp(start, 0d, maxStart);
            end = start + actualWidth;

            double selectionWidth = Math.Max(8d, actualWidth * width);
            double left = start * width;

            SelectionThumb.Width = selectionWidth;
            Canvas.SetLeft(SelectionThumb, Math.Max(0d, Math.Min(width - selectionWidth, left)));
            Canvas.SetTop(SelectionThumb, (TrackBackground.ActualHeight - SelectionThumb.ActualHeight) / 2d);
        }

        private void NormalizeSelection()
        {
            if (_ignoreSelectionUpdate)
                return;

            double start = Clamp(SelectionStart, 0d, 1d);
            double end = Clamp(SelectionEnd, 0d, 1d);

            if (end < start)
                (start, end) = (end, start);

            if (!AreClose(SelectionStart, start) || !AreClose(SelectionEnd, end))
            {
                _ignoreSelectionUpdate = true;
                SelectionStart = start;
                SelectionEnd = end;
                _ignoreSelectionUpdate = false;
            }
        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            _isDraggingSelection = true;
            (DataContext as SampleClipVM)?.SetDraggingSelection(true);
            UpdateLayoutMetrics();
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isDraggingSelection = false;
            (DataContext as SampleClipVM)?.SetDraggingSelection(false);
            UpdateSelectionVisuals();
        }

        private void SelectionThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double width = Math.Max(0d, TrackHost.ActualWidth);
            if (width <= 0d)
                return;

            double delta = e.HorizontalChange / width;

            double start = Clamp(SelectionStart, 0d, 1d);
            double end = Clamp(SelectionEnd, 0d, 1d);
            if (end < start)
                (start, end) = (end, start);

            double span = Math.Max(MinRange, end - start);
            double nextStart = Clamp(start + delta, 0d, Math.Max(0d, 1d - span));
            double nextEnd = nextStart + span;

            _ignoreSelectionUpdate = true;
            SelectionStart = nextStart;
            SelectionEnd = nextEnd;
            _ignoreSelectionUpdate = false;
            UpdateSelectionVisuals();
        }

        private static bool AreClose(double a, double b) => Math.Abs(a - b) < 0.0001d;
    }
}
