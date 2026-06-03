using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace OneColumnEncoder.Components
{
    public partial class ClipRangeSelector : UserControl
    {
        private const double MinRange = 0.01;

        private bool _ignoreSelectionUpdate;
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

        public static readonly DependencyProperty AxisColumnCountProperty = DependencyProperty.Register(
            nameof(AxisColumnCount),
            typeof(int),
            typeof(ClipRangeSelector),
            new PropertyMetadata(0));

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

        public int AxisColumnCount
        {
            get => (int)GetValue(AxisColumnCountProperty);
            private set => SetValue(AxisColumnCountProperty, value);
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
            int count = AxisLabels?.Count() ?? 0;
            AxisColumnCount = count;
            AxisLabelsHost.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateLayoutMetrics()
        {
            double width = Math.Max(0d, TrackHost.ActualWidth);
            if (width <= 0d)
            {
                UpdateSelectionVisuals();
                return;
            }

            double nextTrackHeight = Clamp(width * 0.05d, 18d, 28d);
            double nextThumbWidth = Clamp(width * 0.028d, 14d, 26d);
            double nextThumbHeight = Clamp(nextTrackHeight * 1.9d, 24d, 48d);
            double nextLabelMargin = Clamp(width * 0.012d, 4d, 12d);

            TrackHeight = nextTrackHeight;
            ThumbWidth = nextThumbWidth;
            ThumbHeight = nextThumbHeight;
            LabelMarginTop = nextLabelMargin;

            TrackHost.Height = nextThumbHeight;
            TrackBackground.Height = nextTrackHeight;
            SelectionRange.Height = nextTrackHeight;
            LeftThumb.Width = nextThumbWidth;
            LeftThumb.Height = nextThumbHeight;
            RightThumb.Width = nextThumbWidth;
            RightThumb.Height = nextThumbHeight;
            AxisLabelsHost.Margin = new Thickness(0, nextLabelMargin, 0, 0);

            UpdateSelectionVisuals();
        }

        private void UpdateSelectionVisuals()
        {
            double width = Math.Max(0d, TrackHost.ActualWidth);
            if (width <= 0d)
                return;

            double travelWidth = Math.Max(1d, width - ThumbWidth);
            double start = Clamp(SelectionStart, 0d, 1d);
            double end = Clamp(SelectionEnd, 0d, 1d);
            if (end < start)
                (start, end) = (end, start);

            double startLeft = start * travelWidth;
            double endLeft = end * travelWidth;
            double selectionLeft = startLeft + ThumbWidth / 2d;
            double selectionWidth = Math.Max(0d, endLeft - startLeft);

            Canvas.SetLeft(SelectionRange, selectionLeft);
            Canvas.SetLeft(LeftThumb, startLeft);
            Canvas.SetLeft(RightThumb, endLeft);

            Canvas.SetTop(SelectionRange, (TrackHost.ActualHeight - TrackHeight) / 2d);
            Canvas.SetTop(LeftThumb, (TrackHost.ActualHeight - ThumbHeight) / 2d);
            Canvas.SetTop(RightThumb, (TrackHost.ActualHeight - ThumbHeight) / 2d);

            SelectionRange.Width = Math.Max(0d, selectionWidth);
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
            UpdateLayoutMetrics();
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            UpdateSelectionVisuals();
        }

        private void LeftThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            MoveSelection(startDelta: e.HorizontalChange, endDelta: 0d, dragLeft: true);
        }

        private void RightThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            MoveSelection(startDelta: 0d, endDelta: e.HorizontalChange, dragLeft: false);
        }

        private void MoveSelection(double startDelta, double endDelta, bool dragLeft)
        {
            double width = Math.Max(0d, TrackHost.ActualWidth);
            if (width <= 0d)
                return;

            double travelWidth = Math.Max(1d, width - ThumbWidth);
            double delta = (dragLeft ? startDelta : endDelta) / travelWidth;

            double start = Clamp(SelectionStart, 0d, 1d);
            double end = Clamp(SelectionEnd, 0d, 1d);
            if (end < start)
                (start, end) = (end, start);

            if (dragLeft)
            {
                start = Clamp(start + delta, 0d, Math.Max(0d, end - MinRange));
                if (end - start < MinRange)
                    start = Math.Max(0d, end - MinRange);
            }
            else
            {
                end = Clamp(end + delta, Math.Min(1d, start + MinRange), 1d);
                if (end - start < MinRange)
                    end = Math.Min(1d, start + MinRange);
            }

            _ignoreSelectionUpdate = true;
            SelectionStart = start;
            SelectionEnd = end;
            _ignoreSelectionUpdate = false;
            UpdateSelectionVisuals();
        }

        private static double Clamp(double value, double min, double max) =>
            Math.Max(min, Math.Min(max, value));

        private static bool AreClose(double a, double b) => Math.Abs(a - b) < 0.0001d;
    }
}
