using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Commands.SaveLoad;
using OneColumnEncoder.Components;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class AppConfVM : BaseVM
    {
        private readonly AppConfM _appConfM;

        #region Properties & Commands

        public string WindowTitle => "1cenc Settings";
        public string HeaderText =>
            UILangProviderM.Current["AppConfModal.Header"];
        public string NotificationPolicyHint =>
            UICaptionProviderM.Hints.AppConfNotificationPolicy;

        public CloseModalCmd CloseCmd { get; }
        public SaveAppConfCmd SaveCmd { get; }
        public LoadAppConfCmd LoadCmd { get; }

        public ButtonGroupVM FinishSettingButtons { get; }

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
            FinishSettingButtons = ButtonGroupVM.CreateTwoButton(
                UICaptionProviderM.AppConf.Buttons.Cancel,
                UICaptionProviderM.AppConf.Buttons.Save,
                CloseCmd,
                SaveCmd);
            FinishSettingButtons.B2_2Icon = SvgIconProviderH.GameSave;
            BuildSettingsListing();
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        #endregion

        private void BuildSettingsListing()
        {
            Dictionary<string, object> sourceMap = new()
            {
                [UICaptionProviderM.AppConf.Groups.Overwrite] = _appConfM.Overwrite,
                [UICaptionProviderM.AppConf.Groups.Language] = _appConfM.Lang
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
                                UICaptionProviderM.AppConf.LanguageOptions.Codes);
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
                        UILangProviderM.Current["AppConf.Validation.InvalidNumericInput"],
                        UILangProviderM.Current["AppConf.Validation.InvalidNumericInputTitle"],
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
                        UILangProviderM.Current["AppConf.Validation.Required"],
                        FieldName));
                }

                if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
                {
                    return new ValidationResult(false, string.Format(
                        UILangProviderM.Current["AppConf.Validation.IntegerOnly"],
                        FieldName));
                }

                if (MinValue.HasValue && parsed < MinValue.Value)
                {
                    return new ValidationResult(false, string.Format(
                        UILangProviderM.Current["AppConf.Validation.Min"],
                        FieldName,
                        MinValue.Value));
                }

                if (MaxValue.HasValue && parsed > MaxValue.Value)
                {
                    return new ValidationResult(false, string.Format(
                        UILangProviderM.Current["AppConf.Validation.Max"],
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
                UICaptionProviderM.AppConf.LanguageOptions.GetDisplayName(o)) { Tag = o })];

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
        #endregion

        #region Language Switching

        private void OnLanguageChanged()
        {
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(HeaderText));
            OnPropertyChanged(nameof(NotificationPolicyHint));

            FinishSettingButtons.B2_1Text = UICaptionProviderM.AppConf.Buttons.Cancel;
            FinishSettingButtons.B2_2Text = UICaptionProviderM.AppConf.Buttons.Save;

            SettingsListing.Clear();
            BuildSettingsListing();
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            base.Dispose();
        }

        #endregion
    }
}
