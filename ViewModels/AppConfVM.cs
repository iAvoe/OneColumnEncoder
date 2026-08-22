using OneColumnEncoder.Commands.SaveLoad;
using OneColumnEncoder.Components;
using System.Windows.Shapes;

namespace OneColumnEncoder.ViewModels;

public class AppConfVM : BaseVM
{
    private readonly AppConfM _appConfM;
    private readonly ModalNavS _modalNavS;
    private AppConfLangProvider _lang = AppConfLangProvider.Current;

    public AppConfLangProvider Lang
    {
        get => _lang;
        private set => SetProperty(ref _lang, value);
    }

    #region Properties & Commands
    public static string WindowTitle => AppConfLangProvider.WindowTitle;
    public string HeaderText =>
        Lang["AppConfModal.Header"];
    public string ClearOldQueueJsonHint =>
        Lang["Hint.AppConfClearOldQueueJson"];
    public string AudioMuxHint =>
        Lang["Hint.AudioMux"];
    public string AudioMuxHint2 =>
        Lang["Hint.AudioMux2"];

    public CloseModalCmd CloseCmd { get; }
    public SaveAppConfCmd SaveCmd { get; }
    public LoadAppConfCmd LoadCmd { get; }
    public ICommand RefreshFontsCmd { get; }

    public ButtonGroupVM FinishSettingButtons { get; }

    public ClearOldQueueJsonCmd ClearOldQueueJsonCmd { get; }

    public AppConfM.OverwriteSettings Overwrite => _appConfM.Overwrite;

    public ObservableCollection<AppConfContainer> SettingsListing { get; } = [];

    #endregion

    #region Constructor

    public AppConfVM(AppConfM appConfS, ModalNavS modalNavS, Action closeAction)
    {
        _appConfM = appConfS;
        _modalNavS = modalNavS;
        CloseCmd = new CloseModalCmd(closeAction);
        SaveCmd = new SaveAppConfCmd(appConfS, closeAction);
        LoadCmd = new LoadAppConfCmd(appConfS);
        ClearOldQueueJsonCmd = new ClearOldQueueJsonCmd(modalNavS);
        RefreshFontsCmd = new ActionCmd(_ => RefreshFonts());
        FinishSettingButtons = ButtonGroupVM.CreateThreeButton(
            UICaptionProvider.AppConf.Buttons.Cancel,
            UICaptionProvider.AppConf.Buttons.ClearOldQueueJson,
            UICaptionProvider.AppConf.Buttons.Save,
            CloseCmd,
            ClearOldQueueJsonCmd,
            SaveCmd);
        FinishSettingButtons.B3_2Icon = SvgIconProvider.GameDelete;
        FinishSettingButtons.B3_3Icon = SvgIconProvider.GameSave;
        BuildSettingsListing();
        ShowFontLoadWarningIfNeeded();
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    #endregion

    private void BuildSettingsListing()
    {
        Dictionary<string, object> sourceMap = new()
        {
            [UICaptionProvider.AppConf.Groups.Overwrite] = _appConfM.Overwrite,
            [UICaptionProvider.AppConf.Groups.Language] = _appConfM.Lang,
            [UICaptionProvider.AppConf.Groups.InitMode] = _appConfM,
            [UICaptionProvider.AppConf.Groups.Fonts] = _appConfM.Font,
            [UICaptionProvider.AppConf.Groups.Logs] = _appConfM.Logs,
            [UICaptionProvider.AppConf.Groups.AutoMux] = _appConfM.AutoMux,
            [UICaptionProvider.AppConf.Groups.AudioMux] = _appConfM.AudioMux,
        };

        // Note: order of settings item sections are by SettinglistProviderM.GetAllSettings()
        foreach (IGrouping<string, SettingItemDefinitionM> group
            in SettinglistProviderM.GetAllSettings().GroupBy(s => s.GroupName))
        {
            AppConfContainer container = new() { Header = group.Key };
            object source = sourceMap[group.Key];
            foreach (SettingItemDefinitionM setting in group)
            {
                switch (setting.ControlType)
                {
                    case SettingControlType.CheckBox:
                        AddCheckboxItem(container, setting.Label, source, setting.PropertyName);
                        break;
                    case SettingControlType.TextBox:
                        AddTextboxItem(container,
                            setting.Label,
                            source,
                            setting.PropertyName,
                            setting.MinValue,
                            setting.MaxValue);
                        break;
                    case SettingControlType.Dropdown:
                        AddDropdownItem(container,
                            setting.Label,
                            source,
                            setting.PropertyName,
                            setting.Options ?? UICaptionProvider.AppConf.LanguageOptions.Codes,
                            setting.DisplayNameResolver
                            ?? (source is AppConfM.AudioMuxSettings
                                ? UICaptionProvider.AppConf.AudioMuxOptions.GetDisplayName
                                : UICaptionProvider.AppConf.LanguageOptions.GetDisplayName));
                        break;
                    case SettingControlType.Font:
                        AddFontItem(container, setting.Label, source, setting.PropertyName);
                        break;
                    case SettingControlType.AutoMux:
                        AddAutoMuxRow(container, setting.Label, source, setting.CheckboxProperties);
                        break;
                }
            }

            if (group.Key == UICaptionProvider.AppConf.Groups.Fonts)
                AddFontRefreshItem(container);

            SettingsListing.Add(container);
        }
    }

    #region Setting control elements
    private static void AddCheckboxItem(AppConfContainer container, string text, object source, string propertyPath)
    {
        CheckBox cb = new() { HorizontalAlignment = HorizontalAlignment.Right };
        cb.SetBinding(
            CheckBox.IsCheckedProperty,
            new Binding(propertyPath) { Source = source, Mode = BindingMode.TwoWay });
        container.Items.Add(
            new AppConfItem { Text = text, Content = cb });
    }

    private void AddTextboxItem(AppConfContainer container, string text, object source, string propertyPath,
        int? minValue = null, int? maxValue = null)
    {
        TextBox tb = new()
        {
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        Type propertyType = source.GetType().GetProperty(propertyPath)?.PropertyType ?? typeof(string);

        if (propertyType == typeof(int))
        {
            tb.PreviewTextInput += OnNumericTextBoxPreviewTextInput;
            DataObject.AddPastingHandler(tb, OnNumericTextBoxPasting);
        }

        tb.SetBinding(
            TextBox.TextProperty,
            BuildTextBinding(propertyPath, source, propertyType, text, minValue, maxValue));
        container.Items.Add(new AppConfItem { Text = text, Content = tb });
    }

    private static Binding BuildTextBinding(string propertyPath, object source, Type propertyType,
        string propertyLabel, int? minValue, int? maxValue)
    {
        Binding binding = new()
        {
            Path = new PropertyPath(propertyPath),
            Source = source,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
            NotifyOnValidationError = true
        };

        if (propertyType == typeof(int)
            && (minValue is not null || maxValue is not null))
        {
            binding.ValidationRules.Add(new IntTextBindingRule
            {
                MinValue = minValue,
                MaxValue = maxValue,
                FieldName = propertyLabel
            });
        }

        return binding;
    }

    private static void OnNumericTextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !ContainsOnlyDigits(e.Text);
    }

    private void OnNumericTextBoxPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        string? pastedText = e.DataObject.GetData(DataFormats.Text) as string;
        if (!string.IsNullOrEmpty(pastedText) && !ContainsOnlyDigits(pastedText))
        {
            e.CancelCommand();
            textBox.Dispatcher.InvokeAsync(() =>
                new OpenWarnModalCmd(
                    _modalNavS,
                    AppConfLangProvider.Current["AppConf.Validation.InvalidNumericInputTitle"],
                    AppConfLangProvider.Current["AppConf.Validation.InvalidNumericInput"])
                    .Execute(null));
        }
    }

    private static bool ContainsOnlyDigits(string value) =>
        value.All(char.IsDigit);

    private sealed class IntTextBindingRule : ValidationRule
    {
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public string FieldName { get; set; } = "Value";

        // Invoked by the WPF binding engine at runtime when a bound numeric TextBox
        // (added via BuildTextBinding) loses focus and its value must be validated.
        // Static reference counts report 0 because the call comes through the binding infrastructure,
        // not direct code. Several settings rely on it.
        public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
        {
            string text = value?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text))
            {
                return new ValidationResult(false, string.Format(
                    AppConfLangProvider.Current["AppConf.Validation.Required"],
                    FieldName));
            }

            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
            {
                return new ValidationResult(false, string.Format(
                    AppConfLangProvider.Current["AppConf.Validation.IntegerOnly"],
                    FieldName));
            }

            if (MinValue.HasValue && parsed < MinValue.Value)
            {
                return new ValidationResult(false, string.Format(
                    AppConfLangProvider.Current["AppConf.Validation.Min"],
                    FieldName,
                    MinValue.Value));
            }

            if (MaxValue.HasValue && parsed > MaxValue.Value)
            {
                return new ValidationResult(false, string.Format(
                    AppConfLangProvider.Current["AppConf.Validation.Max"],
                    FieldName,
                    MaxValue.Value));
            }

            return ValidationResult.ValidResult;
        }
    }

    private static void AddDropdownItem(AppConfContainer container, string text, object source, string propertyPath,
        string[] options, Func<string, string>? displayNameResolver = null)
    {
        string currentValue = source.GetType().GetProperty(propertyPath)?.GetValue(source) as string ?? options[0];
        List<DropdownItemM> items = [.. options.Select(o => new DropdownItemM(
            displayNameResolver?.Invoke(o) ?? o) { Tag = o })];

        DropdownMenuVM dropdownVM = new();
        foreach (DropdownItemM item in items) dropdownVM.Items.Add(item);
        dropdownVM.SelectedItem =
            items.FirstOrDefault(i => string.Equals(i.Tag as string, currentValue, StringComparison.OrdinalIgnoreCase)) ?? items[0];

        dropdownVM.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DropdownMenuVM.SelectedItem)
                && dropdownVM.SelectedItem is not null)
            {
                System.Reflection.PropertyInfo? prop = source.GetType().GetProperty(propertyPath);
                prop?.SetValue(source, dropdownVM.SelectedItem.Tag as string ?? dropdownVM.SelectedItem.Title);
            }
        };

        DropdownMenu dropdown = new()
        {
            DataContext = dropdownVM,
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        container.Items.Add(new AppConfItem { Text = text, Content = dropdown });
    }

    private static void AddAutoMuxRow(AppConfContainer container, string modeText, object source,
        IReadOnlyList<string>? propertyPaths)
    {
        if (propertyPaths == null || propertyPaths.Count < 3) return;

        AutoMuxRow row = new() { ModeText = modeText };
        row.SetBinding(AutoMuxRow.IsX264Property,
            new Binding(propertyPaths[0]) { Source = source, Mode = BindingMode.TwoWay });
        row.SetBinding(AutoMuxRow.IsX265Property,
            new Binding(propertyPaths[1]) { Source = source, Mode = BindingMode.TwoWay });
        row.SetBinding(AutoMuxRow.IsSvtAv1Property,
            new Binding(propertyPaths[2]) { Source = source, Mode = BindingMode.TwoWay });
        container.Items.Add(row);
    }

    private static void AddFontItem(AppConfContainer container, string text, object source, string propertyPath)
    {
        bool isCodeFont = propertyPath == nameof(AppConfM.FontSettings.CodeFontFamily);
        IEnumerable<FontFamily> systemFamilies = isCodeFont
            ? AppFontProvider.CodeSystemFonts
            : AppFontProvider.UiSystemFonts;
        IEnumerable<FontFamily> customFamilies = isCodeFont
            ? AppFontProvider.CodeCustomFonts
            : AppFontProvider.UiCustomFonts;

        FontPickerDropdown picker = new()
        {
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Right,
            SystemFamilies = systemFamilies,
            CustomFamilies = customFamilies
        };
        picker.SetBinding(
            FontPickerDropdown.SelectedFontFamilyProperty,
            new Binding(propertyPath) { Source = source, Mode = BindingMode.TwoWay });

        container.Items.Add(new AppConfItem { Text = text, Content = picker });
    }

    private void AddFontRefreshItem(AppConfContainer container)
    {
        Button refreshButton = new()
        {
            Content = CreateRefreshButtonContent(),
            Style = (Style)Application.Current.FindResource("NormalButton"),
            // Already "aligns" right: HorizontalAlignment = HorizontalAlignment.Right,
            MinWidth = 200,
            Height = 25,
            Command = RefreshFontsCmd,
        };

        container.Items.Add(new AppConfItem { Text = string.Empty, Content = refreshButton });
    }

    private static StackPanel CreateRefreshButtonContent()
    {
        StackPanel content = new()
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
        };

        Rectangle icon = new()
        {
            Width = 15,
            Height = 15,
            Margin = new Thickness(0, 0, 5, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Fill = Brushes.Transparent
        };
        icon.SetBinding(Rectangle.FillProperty,
            new Binding(nameof(Control.Foreground))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Button), 1)
            });
        icon.OpacityMask = new ImageBrush
        {
            ImageSource = SvgIconProvider.GameRefresh,
            Stretch = Stretch.Uniform
        };
        content.Children.Add(icon);
        content.Children.Add(new TextBlock
        {
            Text = AppConfLangProvider.Current["Refresh"],
            Margin = new Thickness(5, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        });
        return content;
    }

    private void RefreshFonts()
    {
        AppFontProvider.Refresh();
        AppFontProvider.ApplyFrom(_appConfM);
        SettingsListing.Clear();
        BuildSettingsListing();
        ShowFontLoadWarningIfNeeded();
    }

    private void ShowFontLoadWarningIfNeeded()
    {
        if (!AppFontProvider.HasCustomFontLoadIssues) return;

        Application.Current?.Dispatcher.InvokeAsync(() =>
            new OpenWarnModalCmd(
                _modalNavS,
                AppConfLangProvider.WindowTitle,
                AppConfLangProvider.Current["AppConf.Font.UnusableWarning"])
                .Execute(null));
    }
    #endregion

    #region Language Switching

    private void OnLanguageChanged()
    {
        Lang = AppConfLangProvider.Current;
        OnPropertyChanged(nameof(HeaderText));
        OnPropertyChanged(nameof(ClearOldQueueJsonHint));
        OnPropertyChanged(nameof(AudioMuxHint));
        OnPropertyChanged(nameof(AudioMuxHint2));

        FinishSettingButtons.B3_1Text = UICaptionProvider.AppConf.Buttons.Cancel;
        FinishSettingButtons.B3_2Text = UICaptionProvider.AppConf.Buttons.ClearOldQueueJson;
        FinishSettingButtons.B3_3Text = UICaptionProvider.AppConf.Buttons.Save;

        SettingsListing.Clear();
        BuildSettingsListing();
    }

    #endregion

    #region Dispose

    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}
