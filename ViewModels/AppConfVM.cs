using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OneColumnEncoder.Components;
using OneColumnEncoder.Stores;
using System.Windows.Input;
using OneColumnEncoder.Models;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Commands.SaveLoad;

namespace OneColumnEncoder.ViewModels
{
    public class AppConfVM : BaseVM
    {
        private readonly AppConfM _appConfM;

        #region Properties & Commands

        public string WindowTitle => UILangProviderM.Current["AppConfModal.Title"];
        public string HeaderText => UILangProviderM.Current["AppConfModal.Header"];

        public CloseModalCmd CloseCmd { get; }
        public SaveAppConfCmd SaveCmd { get; }
        public LoadAppConfCmd LoadCmd { get; }
        public ICommand? SmtpCmd { get; } // TODO

        public ButtonGroupVM FinishSettingButtons { get; }

        public AppConfM.GeneralSettings General => _appConfM.General;
        public AppConfM.OverwriteSettings Overwrite => _appConfM.Overwrite;
        public AppConfM.SmtpSettings Smtp => _appConfM.Smtp;

        public ObservableCollection<AppConfContainer> SettingsListing { get; } = [];

        #endregion

        #region Constructor

        public AppConfVM(AppConfM appConfS, Action closeAction)
        {
            _appConfM = appConfS;
            CloseCmd = new CloseModalCmd(closeAction);
            SaveCmd = new SaveAppConfCmd(appConfS, closeAction);
            LoadCmd = new LoadAppConfCmd(appConfS);
            SmtpCmd = null; // TODO
            FinishSettingButtons = ButtonGroupVM.CreateThreeButton(
                UICaptionProviderM.AppConf.Buttons.TestSmtp,
                UICaptionProviderM.AppConf.Buttons.Cancel,
                UICaptionProviderM.AppConf.Buttons.Save,
                SmtpCmd,
                CloseCmd,
                SaveCmd);
            BuildSettingsListing();
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        #endregion

        private void BuildSettingsListing()
        {
            Dictionary<string, object> sourceMap = new()
            {
                [UICaptionProviderM.AppConf.Groups.General] = _appConfM.General,
                [UICaptionProviderM.AppConf.Groups.Overwrite] = _appConfM.Overwrite,
                [UICaptionProviderM.AppConf.Groups.Smtp] = _appConfM.Smtp,
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
                            AddTextboxItem(container, setting.Label, source, setting.PropertyName);
                            break;
                        case SettingControlType.PasswordBox:
                            AddPasswordBoxItem(container, setting.Label,
                                () => _appConfM.Smtp.Password,
                                v => _appConfM.Smtp.Password = v);
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

        private static void AddTextboxItem(AppConfContainer container, string text, object source, string propertyPath)
        {
            TextBox tb = new()
            {
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            tb.SetBinding(TextBox.TextProperty, new Binding(propertyPath) { Source = source, Mode = BindingMode.TwoWay });
            container.Items.Add(new AppConfItem { Text = text, Content = tb });
        }

        private static void AddPasswordBoxItem(AppConfContainer container, string text, Func<string> getter, Action<string> setter)
        {
            PasswordBox pb = new()
            {
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Right,
                Password = getter()
            };
            pb.PasswordChanged += (_, _) => setter(pb.Password);
            container.Items.Add(new AppConfItem { Text = text, Content = pb });
        }

        private static void AddDropdownItem(AppConfContainer container, string text, object source, string propertyPath, string[] options)
        {
            string currentValue = source.GetType().GetProperty(propertyPath)?.GetValue(source) as string ?? options[0];
            List<DropdownItemM> items = [.. options.Select(o => new DropdownItemM(o))];

            DropdownMenuVM dropdownVM = new();
            foreach (DropdownItemM item in items) dropdownVM.Items.Add(item);
            dropdownVM.SelectedItem =
                items.FirstOrDefault(i => i.Title == currentValue) ?? items[0];

            dropdownVM.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(DropdownMenuVM.SelectedItem)
                    && dropdownVM.SelectedItem is not null)
                {
                    System.Reflection.PropertyInfo? prop = source.GetType().GetProperty(propertyPath);
                    prop?.SetValue(source, dropdownVM.SelectedItem.Title);
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

            FinishSettingButtons.B3_1Text = UICaptionProviderM.AppConf.Buttons.TestSmtp;
            FinishSettingButtons.B3_2Text = UICaptionProviderM.AppConf.Buttons.Cancel;
            FinishSettingButtons.B3_3Text = UICaptionProviderM.AppConf.Buttons.Save;

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