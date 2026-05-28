using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace OneColumnEncoder.Components
{
    public partial class IntegerSlider : UserControl
    {
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

        public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
        public int Minimum { get => (int)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
        public int Maximum { get => (int)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
        public int Value { get => (int)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
        public bool IsLogarithmic { get => (bool)GetValue(IsLogarithmicProperty); set => SetValue(IsLogarithmicProperty, value); }
        public IEnumerable<string> TickLabels { get => (IEnumerable<string>)GetValue(TickLabelsProperty); set => SetValue(TickLabelsProperty, value); }
        public int TickCount { get => (int)GetValue(TickCountProperty); set => SetValue(TickCountProperty, value); }
        public bool SnapToTicks { get => (bool)GetValue(SnapToTicksProperty); set => SetValue(SnapToTicksProperty, value); }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            UpdateValueFromPosition(e.HorizontalChange);
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (SnapToTicks)
            {
                Value = Math.Max(Minimum, Math.Min(Maximum, Value));
            }
        }

        private void UpdateValueFromPosition(double deltaX)
        {
            if (TrackHost.ActualWidth <= 0 || Maximum <= Minimum)
            {
                return;
            }

            var ratio = Math.Max(0d, Math.Min(1d, (double)(Value - Minimum) / (Maximum - Minimum)));
            ratio = Math.Max(0d, Math.Min(1d, ratio + deltaX / TrackHost.ActualWidth));

            var next = IsLogarithmic ? FromLogRatio(ratio) : Minimum + ratio * (Maximum - Minimum);
            Value = SnapToTicks ? SnapValue((int)Math.Round(next)) : (int)next;
        }

        private int SnapValue(int value)
        {
            if (TickCount <= 1)
            {
                return Math.Max(Minimum, Math.Min(Maximum, value));
            }

            var step = (Maximum - Minimum) / (double)(TickCount - 1);
            var index = Math.Round((value - Minimum) / step);
            return Math.Max(Minimum, Math.Min(Maximum, (int)Math.Round(Minimum + index * step)));
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
