using System.Windows;

namespace OneColumnEncoder.Helpers
{
    public class AdaptiveWindow : Window
    {
        public AdaptiveWindow()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            double workAreaHeight = SystemParameters.WorkArea.Height;
            double maxHeight = double.IsInfinity(MaxHeight)
                ? workAreaHeight
                : Math.Min(MaxHeight, workAreaHeight);

            MaxHeight = maxHeight;

            if (Height > maxHeight)
            {
                Height = maxHeight;
            }
        }
    }
}
