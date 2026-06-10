using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components
{
    public class ChecklistContainer : HeaderedItemsControl
    {
        static ChecklistContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ChecklistContainer),
                new FrameworkPropertyMetadata(typeof(ChecklistContainer)));
        }
    }
}
