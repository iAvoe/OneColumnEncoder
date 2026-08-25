namespace OneColumnEncoder.Views;

public partial class FilterScribeModal : AdaptiveWindow
{
    private const int DefaultWidth = 740;
    private const int DefaultMinWidth = 640;
    private const int SidebarWidth = 310;
    private const int SidebarGapWidth = 10;
    private const int DefaultWidthWithSidebar = DefaultWidth + SidebarWidth + SidebarGapWidth;
    private const int MinWidthWithSidebar = DefaultMinWidth + SidebarWidth + SidebarGapWidth;

    private const int DefaultHeight = 1010;
    private const int DefaultHeightWithSidebar = DefaultHeight + 50;
    private FilterScribeVM? _subscribedVm;

    public FilterScribeModal()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not FilterScribeVM vm)
            return;

        if (_subscribedVm != vm)
        {
            DetachViewModelEvents();
            _subscribedVm = vm;
            vm.PropertyChanged += OnViewModelPropertyChanged;
        }

        SyncSidebarWidth(vm.IsConcatMode);
        ApplyTextBoxContextMenus();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        DetachViewModelEvents();
        Loaded -= OnLoaded;
        Closed -= OnClosed;
    }

    private void DetachViewModelEvents()
    {
        if (_subscribedVm != null)
            _subscribedVm.PropertyChanged -= OnViewModelPropertyChanged;

        _subscribedVm = null;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(FilterScribeVM.IsConcatMode)) return;

        if (sender is FilterScribeVM vm)
            SyncSidebarWidth(vm.IsConcatMode);
    }

    private void SyncSidebarWidth(bool isConcatMode)
    {
        if (isConcatMode)
            ExpandForSidebar();
        else
            CollapseSidebar();
    }

    private void ExpandForSidebar()
    {
        MinWidth = MinWidthWithSidebar;
        if (Width < DefaultWidthWithSidebar)
            Width = DefaultWidthWithSidebar;
        // There is no minimal height: !MinHeightDefault = !MinHeightWithSidebar;
        if (Height < DefaultHeightWithSidebar)
            Height = DefaultHeightWithSidebar;
    }

    private void CollapseSidebar()
    {
        MinWidth = DefaultMinWidth;
        if (Width > DefaultWidth)
            Width = DefaultWidth;
        // There is no minimal height: !MinHeightDefault;
        if (Height < DefaultHeight)
            Height = DefaultHeight;
    }

    private void UserInput_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control) return;

        var box = (TextBox)sender;
        double newSize = box.FontSize + (e.Delta > 0 ? 1 : -1);
        box.FontSize = double.Clamp(newSize, 8, 48);
        e.Handled = true;
    }

    private void ApplyTextBoxContextMenus()
    {
        foreach (TextBox textBox in EnumerateDescendants<TextBox>(this))
            textBox.ContextMenu = CreateTextBoxContextMenu();
    }

    private static ContextMenu CreateTextBoxContextMenu()
    {
        ContextMenu menu = new();

        menu.Items.Add(CreateMenuItem("Lang.UndoText", ApplicationCommands.Undo));
        menu.Items.Add(CreateMenuItem("Lang.RedoText", ApplicationCommands.Redo));
        menu.Items.Add(new Separator());
        menu.Items.Add(CreateMenuItem("Lang.CutText", ApplicationCommands.Cut));
        menu.Items.Add(CreateMenuItem("Lang.CopyText", ApplicationCommands.Copy));
        menu.Items.Add(CreateMenuItem("Lang.PasteText", ApplicationCommands.Paste));
        menu.Items.Add(CreateMenuItem("Lang.DeleteText", ApplicationCommands.Delete));
        menu.Items.Add(new Separator());
        menu.Items.Add(CreateMenuItem("Lang.SelectAllText", ApplicationCommands.SelectAll));

        return menu;
    }

    private static MenuItem CreateMenuItem(string headerProperty, RoutedUICommand command)
    {
        MenuItem item = new() { Command = command };

        BindingOperations.SetBinding(item, HeaderedItemsControl.HeaderProperty, new Binding($"PlacementTarget.DataContext.{headerProperty}")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ContextMenu), 1)
        });

        BindingOperations.SetBinding(item, MenuItem.CommandTargetProperty, new Binding("PlacementTarget")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ContextMenu), 1)
        });

        return item;
    }

    private static IEnumerable<T> EnumerateDescendants<T>(DependencyObject root) where T : DependencyObject
    {
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, i);
            if (child is T typed)
                yield return typed;

            foreach (T descendant in EnumerateDescendants<T>(child))
                yield return descendant;
        }
    }

    private void FfmpegFreeText_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter && e.Key != Key.Return)
            return;

        e.Handled = true;
    }

    private void FfmpegFreeText_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        string? text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string
            ?? e.SourceDataObject.GetData(DataFormats.Text) as string;

        if (text is null) return;

        string normalized = text.Replace("\r", string.Empty).Replace("\n", string.Empty);
        if (normalized == text) return;

        DataObject dataObject = new();
        dataObject.SetData(DataFormats.UnicodeText, normalized);
        e.DataObject = dataObject;
    }
}
