using OneColumnEncoder.Commands.SaveLoad;
using OneColumnEncoder.Components;
using System.Collections.ObjectModel;
using System.Globalization;

namespace OneColumnEncoder.ViewModels
{
    public class AppConfVM : BaseVM
    {
        private readonly AppConfM _appConfM;
        private AppConfLangProvider _lang = AppConfLangProvider.Current;

        public AppConfLangProvider Lang
        {
            get => _lang;
            private set => SetProperty(ref _lang, value);
        }

        #region Properties & Commands
        public string WindowTitle => Lang["AppConfModal.Title"];
        public string HeaderText =>
            Lang["AppConfModal.Header"];
        public string NotificationPolicyHint =>
            Lang["Hint.AppConfNotificationPolicy"];
        public string ClearOldQueueJsonHint =>
            Lang["Hint.AppConfClearOldQueueJson"];

        public CloseModalCmd CloseCmd { get; }
        public SaveAppConfCmd SaveCmd { get; }
        public LoadAppConfCmd LoadCmd { get; }

        public ButtonGroupVM FinishSettingButtons { get; }

        public ClearOldQueueJsonCmd ClearOldQueueJsonCmd { get; }

        public AppConfM.OverwriteSettings Overwrite => _appConfM.Overwrite;

        public ObservableCollection<AppConfContainer> SettingsListing { get; } = [];

        #endregion

        #region Constructor

        public AppConfVM(AppConfM appConfS, Action closeAction)
        {
            _appConfM = appConfS;
            CloseCmd = new CloseModalCmd(closeAction);
            SaveCmd = new SaveAppConfCmd(appConfS, closeAction);
            LoadCmd = new LoadAppConfCmd(appConfS);
            ClearOldQueueJsonCmd = new ClearOldQueueJsonCmd();
            FinishSettingButtons = ButtonGroupVM.CreateThreeButton(
                UICaptionProvider.AppConf.Buttons.Cancel,
                UICaptionProvider.AppConf.Buttons.ClearOldQueueJson,
                UICaptionProvider.AppConf.Buttons.Save,
                CloseCmd,
                ClearOldQueueJsonCmd,
                SaveCmd);
            FinishSettingButtons.B3_2Icon = SvgIconProvider.GameDelete;
            FinishSettingButtons.B3_3Icon = SvgIconProvider.GameSave;
            AppFontProvider.Refresh();
            BuildSettingsListing();
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
                [UICaptionProvider.AppConf.Groups.Logs] = _appConfM.Logs
            };

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
                            AddDropdownItem(container, setting.Label, source, setting.PropertyName,
                                UICaptionProvider.AppConf.LanguageOptions.Codes);
                            break;
                        case SettingControlType.Font:
                            AddFontItem(container, setting.Label, source, setting.PropertyName);
                            break;
                    }
                }
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

        private static void AddTextboxItem(AppConfContainer container, string text, object source, string propertyPath,
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

        private static void OnNumericTextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            string? pastedText = e.DataObject.GetData(DataFormats.Text) as string;
            if (!string.IsNullOrEmpty(pastedText) && !ContainsOnlyDigits(pastedText))
            {
                e.CancelCommand();
                textBox.Dispatcher.InvokeAsync(() =>
                    System.Windows.MessageBox.Show(
                        AppConfLangProvider.Current["AppConf.Validation.InvalidNumericInput"],
                        AppConfLangProvider.Current["AppConf.Validation.InvalidNumericInputTitle"],
                    MessageBoxButton.OK, MessageBoxImage.Warning));
            }
        }

        private static bool ContainsOnlyDigits(string value) =>
            value.All(char.IsDigit);

        private sealed class IntTextBindingRule : ValidationRule
        {
            public int? MinValue { get; set; }
            public int? MaxValue { get; set; }
            public string FieldName { get; set; } = "Value";

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

        private static void AddDropdownItem(AppConfContainer container, string text, object source, string propertyPath, string[] options)
        {
            string currentValue = source.GetType().GetProperty(propertyPath)?.GetValue(source) as string ?? options[0];
            List<DropdownItemM> items = [.. options.Select(o => new DropdownItemM(
                UICaptionProvider.AppConf.LanguageOptions.GetDisplayName(o)) { Tag = o })];

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

        private static void AddFontItem(AppConfContainer container, string text, object source, string propertyPath)
        {
            IEnumerable<FontFamily> families = propertyPath == nameof(AppConfM.FontSettings.CodeFontFamily)
                ? AppFontProvider.CodeFonts
                : AppFontProvider.UiFonts;

            FontPickerDropdown picker = new()
            {
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Right,
                Families = families
            };
            picker.SetBinding(
                FontPickerDropdown.SelectedFontFamilyProperty,
                new Binding(propertyPath) { Source = source, Mode = BindingMode.TwoWay });

            container.Items.Add(new AppConfItem { Text = text, Content = picker });
        }
        #endregion

        #region Language Switching

        private void OnLanguageChanged()
        {
            Lang = AppConfLangProvider.Current;
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(HeaderText));
            OnPropertyChanged(nameof(NotificationPolicyHint));
            OnPropertyChanged(nameof(ClearOldQueueJsonHint));

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
}
