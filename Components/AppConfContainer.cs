using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components
{
    public class AppConfContainer : HeaderedItemsControl
    {
        static AppConfContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(AppConfContainer),
                new FrameworkPropertyMetadata(typeof(AppConfContainer)));
        }
    }
}
