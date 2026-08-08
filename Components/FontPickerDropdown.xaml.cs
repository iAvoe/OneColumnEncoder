namespace OneColumnEncoder.Components
{
    /// <summary>
    /// A dropdown that lets the user pick one font from a supplied list of
    /// <see cref="FontFamily"/> values. Each item is rendered using its own font.
    /// The selected value is exposed as the family display name (string).
    /// </summary>
    public partial class FontPickerDropdown : UserControl
    {
        public static readonly DependencyProperty FamiliesProperty =
            DependencyProperty.Register(
                nameof(Families),
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

        private readonly ObservableCollection<DropdownItemM> _items = [];
        private bool _syncing;

        public IEnumerable<FontFamily>? Families
        {
            get => (IEnumerable<FontFamily>?)GetValue(FamiliesProperty);
            set => SetValue(FamiliesProperty, value);
        }

        public string SelectedFontFamily
        {
            get => (string)GetValue(SelectedFontFamilyProperty);
            set => SetValue(SelectedFontFamilyProperty, value);
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
            if (Families != null)
            {
                foreach (FontFamily family in Families)
                    _items.Add(new DropdownItemM(AppFontProvider.GetFontDisplayName(family)) { Tag = family });
            }

            SyncSelection();
        }

        private void SyncSelection()
        {
            if (_syncing || FontComboBox == null) return;

            _syncing = true;
            try
            {
                string current = SelectedFontFamily ?? string.Empty;
                if (string.IsNullOrWhiteSpace(current))
                {
                    FontComboBox.SelectedItem = _items.FirstOrDefault(i => i.IsPlaceholder);
                }
                else
                {
                    FontComboBox.SelectedItem = _items.FirstOrDefault(
                        i => !i.IsPlaceholder && string.Equals(i.Title, current, System.StringComparison.OrdinalIgnoreCase));
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
            else if (item.Tag is FontFamily family)
                SelectedFontFamily = AppFontProvider.GetFontDisplayName(family);
        }
    }
}
