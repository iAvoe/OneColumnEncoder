using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace OneColumnEncoder.Components
{
    public partial class IntegerSlider : UserControl
    {
        private double _dragRatio;
        private int _dragValue;
        private double _dragPointerOffset;
        internal const double ThumbWidth = 14d;

        public IntegerSlider()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(IntegerSlider), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(int), typeof(IntegerSlider), new PropertyMetadata(0));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(int), typeof(IntegerSlider), new PropertyMetadata(7, OnRangeChanged));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(int), typeof(IntegerSlider), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IsLogarithmicProperty = DependencyProperty.Register(
            nameof(IsLogarithmic), typeof(bool), typeof(IntegerSlider), new PropertyMetadata(false));

        public static readonly DependencyProperty TickLabelsProperty = DependencyProperty.Register(
            nameof(TickLabels), typeof(IEnumerable<string>), typeof(IntegerSlider), new PropertyMetadata(null));

        public static readonly DependencyProperty TickCountProperty = DependencyProperty.Register(
            nameof(TickCount), typeof(int), typeof(IntegerSlider), new PropertyMetadata(8, OnRangeChanged));

        public static readonly DependencyProperty SnapToTicksProperty = DependencyProperty.Register(
            nameof(SnapToTicks), typeof(bool), typeof(IntegerSlider), new PropertyMetadata(true));

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
            nameof(Step), typeof(int), typeof(IntegerSlider), new PropertyMetadata(1));

        public static readonly DependencyProperty SliderMaxWidthProperty = DependencyProperty.Register(
            nameof(SliderMaxWidth), typeof(double), typeof(IntegerSlider), new PropertyMetadata(double.PositiveInfinity));

        public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(
            nameof(LabelWidth), typeof(double), typeof(IntegerSlider), new PropertyMetadata(double.NaN));

        public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
        public int Minimum { get => (int)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
        public int Maximum { get => (int)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
        public int Value { get => (int)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
        public bool IsLogarithmic { get => (bool)GetValue(IsLogarithmicProperty); set => SetValue(IsLogarithmicProperty, value); }
        public IEnumerable<string> TickLabels { get => (IEnumerable<string>)GetValue(TickLabelsProperty); set => SetValue(TickLabelsProperty, value); }
        public int TickCount { get => (int)GetValue(TickCountProperty); set => SetValue(TickCountProperty, value); }
        public bool SnapToTicks { get => (bool)GetValue(SnapToTicksProperty); set => SetValue(SnapToTicksProperty, value); }
        public int Step { get => (int)GetValue(StepProperty); set => SetValue(StepProperty, value); }
        public double SliderMaxWidth { get => (double)GetValue(SliderMaxWidthProperty); set => SetValue(SliderMaxWidthProperty, value); }
        public double LabelWidth { get => (double)GetValue(LabelWidthProperty); set => SetValue(LabelWidthProperty, value); }

        internal double MeasuredLabelWidth => LabelTextBlock.ActualWidth;

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            _dragRatio = GetCurrentRatio();
            _dragValue = Value;
            double travelWidth = GetTravelWidth();
            double thumbCenter = _dragRatio * travelWidth + ThumbWidth / 2d;
            _dragPointerOffset = Mouse.GetPosition(TrackHost).X - thumbCenter;
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            UpdateValueFromPointer();
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (SnapToTicks)
            {
                Value = Math.Max(Minimum, Math.Min(Maximum, Value));
            }
        }

        private void UpdateValueFromPointer()
        {
            if (TrackHost.ActualWidth <= 0 || Maximum <= Minimum)
            {
                return;
            }

            double travelWidth = GetTravelWidth();
            double pointerX = Mouse.GetPosition(TrackHost).X - _dragPointerOffset;
            double thumbLeft = pointerX - ThumbWidth / 2d;
            _dragRatio = Math.Max(0d, Math.Min(1d, thumbLeft / travelWidth));

            int nextValue = RatioToValue(_dragRatio);
            if (nextValue != _dragValue)
            {
                _dragValue = nextValue;
                Value = nextValue;
            }
        }

        private double GetCurrentRatio()
        {
            if (Maximum <= Minimum)
            {
                return 0d;
            }

            return Math.Max(0d, Math.Min(1d, (Value - Minimum) / (double)(Maximum - Minimum)));
        }

        private double GetTravelWidth()
        {
            return Math.Max(1d, TrackHost.ActualWidth - ThumbWidth);
        }

        private int RatioToValue(double ratio)
        {
            double next = IsLogarithmic ? FromLogRatio(ratio) : Minimum + ratio * (Maximum - Minimum);
            int step = Math.Max(1, Step);
            if (step > 1)
            {
                int offset = (int)Math.Round((next - Minimum) / (double)step) * step + Minimum;
                return Math.Max(Minimum, Math.Min(Maximum, offset));
            }
            int snapped = SnapToTicks ? (int)Math.Round(next) : (int)next;
            return Math.Max(Minimum, Math.Min(Maximum, snapped));
        }

        private double FromLogRatio(double ratio)
        {
            var min = Math.Max(1, Minimum);
            var max = Math.Max(min + 1, Maximum);
            return min * Math.Pow(max / (double)min, ratio);
        }

        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (IntegerSlider)d;
            if (control.Value < control.Minimum) control.Value = control.Minimum;
            if (control.Value > control.Maximum) control.Value = control.Maximum;
        }
    }
}
