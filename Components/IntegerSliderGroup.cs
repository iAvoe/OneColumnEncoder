using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace OneColumnEncoder.Components
{
    public class IntegerSliderGroup : StackPanel
    {
        public IntegerSliderGroup()
        {
            Orientation = Orientation.Vertical;
            Loaded += (_, _) => Dispatcher.BeginInvoke(new Action(UpdateSliderLabelWidths), DispatcherPriority.Loaded);
            SizeChanged += (_, _) => UpdateSliderLabelWidths();
        }

        private void UpdateSliderLabelWidths()
        {
            IntegerSlider[] sliders = Children.OfType<IntegerSlider>().ToArray();
            if (sliders.Length == 0)
            {
                return;
            }

            foreach (IntegerSlider slider in sliders)
            {
                slider.LabelWidth = double.NaN;
            }

            double labelWidth = sliders.Max(s => s.MeasuredLabelWidth);
            if (labelWidth <= 0 || double.IsNaN(labelWidth) || double.IsInfinity(labelWidth))
            {
                return;
            }

            labelWidth = Math.Ceiling(labelWidth);
            foreach (IntegerSlider slider in sliders)
            {
                if (!AreClose(slider.LabelWidth, labelWidth))
                {
                    slider.LabelWidth = labelWidth;
                }
            }
        }

        private static bool AreClose(double value1, double value2)
        {
            return Math.Abs(value1 - value2) < 0.1;
        }
    }
}
