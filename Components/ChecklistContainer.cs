namespace OneColumnEncoder.Components;

public class ChecklistContainer : HeaderedItemsControl
{
    static ChecklistContainer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ChecklistContainer),
            new FrameworkPropertyMetadata(typeof(ChecklistContainer)));
    }
}
