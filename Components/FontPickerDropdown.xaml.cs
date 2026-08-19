namespace OneColumnEncoder.Components;

/// <summary>
/// A dropdown that lets the user pick one font from a supplied list of
/// <see cref="FontFamily"/> values. Each item is rendered using its own font.
/// The selected value is exposed as the family display name (string).
/// </summary>
public partial class FontPickerDropdown : UserControl
{
    private const double DefaultDropDownWidth = 200d;

    public static readonly DependencyProperty SystemFamiliesProperty =
        DependencyProperty.Register(
            nameof(SystemFamilies),
            typeof(IEnumerable<FontFamily>),
            typeof(FontPickerDropdown),
            new PropertyMetadata(null, OnFamiliesChanged));

    public static readonly DependencyProperty CustomFamiliesProperty =
        DependencyProperty.Register(
            nameof(CustomFamilies),
            typeof(IEnumerable<FontFamily>),
            typeof(FontPickerDropdown),
            new PropertyMetadata(null, OnFamiliesChanged));

    public static readonly DependencyProperty SelectedFontFamilyProperty =
        DependencyProperty.Register(
            nameof(SelectedFontFamily),
            typeof(string),
            typeof(FontPickerDropdown),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedFontFamilyChanged));

    public static readonly DependencyProperty DropDownWidthProperty =
        DependencyProperty.Register(
            nameof(DropDownWidth),
            typeof(double),
            typeof(FontPickerDropdown),
            new PropertyMetadata(DefaultDropDownWidth));

    private readonly ObservableCollection<DropdownItemM> _items = [];
    private bool _syncing;

    public IEnumerable<FontFamily>? SystemFamilies
    {
        get => (IEnumerable<FontFamily>?)GetValue(SystemFamiliesProperty);
        set => SetValue(SystemFamiliesProperty, value);
    }

    public IEnumerable<FontFamily>? CustomFamilies
    {
        get => (IEnumerable<FontFamily>?)GetValue(CustomFamiliesProperty);
        set => SetValue(CustomFamiliesProperty, value);
    }

    public string SelectedFontFamily
    {
        get => (string)GetValue(SelectedFontFamilyProperty);
        set => SetValue(SelectedFontFamilyProperty, value);
    }

    public double DropDownWidth
    {
        get => (double)GetValue(DropDownWidthProperty);
        private set => SetValue(DropDownWidthProperty, value);
    }

    public FontPickerDropdown()
    {
        InitializeComponent();
        FontComboBox.ItemsSource = _items;
    }

    private static void OnFamiliesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((FontPickerDropdown)d).RebuildItems();

    private static void OnSelectedFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((FontPickerDropdown)d).SyncSelection();

    private void RebuildItems()
    {
        _items.Clear();
        _items.Add(new DropdownItemM(AppConfLangProvider.Current["Setting.Font.Default"]) { IsPlaceholder = true });

        List<DropdownItemM> systemItems = BuildItems(SystemFamilies);
        List<DropdownItemM> customItems = BuildItems(CustomFamilies);

        UpdateDropDownWidth(systemItems, customItems);

        foreach (DropdownItemM item in customItems)
            _items.Add(item);

        if (systemItems.Count > 0 && customItems.Count > 0)
            _items.Add(new DropdownItemM(string.Empty, isSeparator: true));

        foreach (DropdownItemM item in systemItems)
            _items.Add(item);

        SyncSelection();
    }

    private static List<DropdownItemM> BuildItems(IEnumerable<FontFamily>? families)
    {
        if (families is null) return [];

        List<DropdownItemM> items = [];
        foreach (FontFamily family in families)
        {
            string title = AppFontProvider.GetFontDisplayName(family);
            if (string.IsNullOrWhiteSpace(title)) continue;

            items.Add(new DropdownItemM(title) { Tag = family });
        }

        return items;
    }

    private void UpdateDropDownWidth(List<DropdownItemM> systemItems, List<DropdownItemM> customItems)
    {
        double widest = MeasureMenuWidth(new DropdownItemM(AppConfLangProvider.Current["Setting.Font.Default"]) { IsPlaceholder = true });

        foreach (DropdownItemM item in customItems)
            widest = Math.Max(widest, MeasureMenuWidth(item));

        foreach (DropdownItemM item in systemItems)
            widest = Math.Max(widest, MeasureMenuWidth(item));

        DropDownWidth = Math.Max(DefaultDropDownWidth, Math.Ceiling(widest));
    }

    private double MeasureMenuWidth(DropdownItemM item)
    {
        if (string.IsNullOrWhiteSpace(item.Title))
            return 0d;

        FontFamily family = item.Tag as FontFamily ?? FontComboBox.FontFamily;
        double fontSize = FontComboBox.FontSize > 0 ? FontComboBox.FontSize : FontSize;
        double pixelsPerDip = 1d;

        if (PresentationSource.FromVisual(FontComboBox) is { CompositionTarget: not null } source)
            pixelsPerDip = source.CompositionTarget.TransformToDevice.M11;

        Typeface typeface = new(family, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        FormattedText formattedText = new(
            item.Title,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            Brushes.Black,
            pixelsPerDip);

        // Match the dropdown item padding from App.xaml.
        return formattedText.WidthIncludingTrailingWhitespace + 40d;
    }

    private void SyncSelection()
    {
        if (_syncing || FontComboBox == null) return;

        _syncing = true;
        try
        {
            string current = SelectedFontFamily ?? string.Empty;
            DropdownItemM? placeholder = _items.FirstOrDefault(i => i.IsPlaceholder);
            if (string.IsNullOrWhiteSpace(current))
            {
                FontComboBox.SelectedItem = placeholder;
            }
            else
            {
                FontComboBox.SelectedItem = _items.FirstOrDefault(
                    i => !i.IsPlaceholder && !i.IsSeparator && string.Equals(i.Title, current, System.StringComparison.OrdinalIgnoreCase))
                    ?? placeholder;
            }
        }
        finally
        {
            _syncing = false;
        }
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncing) return;
        if (FontComboBox.SelectedItem is not DropdownItemM item) return;

        if (item.IsPlaceholder)
            SelectedFontFamily = string.Empty;
        else if (item.IsSeparator)
            return;
        else if (item.Tag is FontFamily family)
            SelectedFontFamily = AppFontProvider.GetFontDisplayName(family);
    }
}
