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

        // Commands for UI interactions
        public CloseModalCmd CloseCmd { get; }
        public SaveAppConfCmd SaveCmd { get; }
        public LoadAppConfCmd LoadCmd { get; }
        public ICommand? SmtpCmd { get; } // TODO

        public ButtonGroupVM FinishSettingButtons { get; }

        // Settings for binding
        public AppConfM.GeneralSettings General => _appConfM.General;
        public AppConfM.OverwriteSettings Overwrite => _appConfM.Overwrite;
        public AppConfM.SmtpSettings Smtp => _appConfM.Smtp;

        // Grouped settings listing for the ListView
        public ObservableCollection<AppConfContainer> SettingsListing { get; } = [];

        public AppConfVM(ModalNavS modalNavS, AppConfM appConfS, Action closeAction)
        {
            _appConfM = appConfS;
            CloseCmd = new CloseModalCmd(modalNavS, closeAction);
            SaveCmd = new SaveAppConfCmd(appConfS, modalNavS, closeAction);
            LoadCmd = new LoadAppConfCmd(appConfS);
            SmtpCmd = null; // TODO
            FinishSettingButtons = ButtonGroupVM.CreateThreeButton(
                "Test SMTP",
                "Cancel",
                "Save",
                SmtpCmd,
                CloseCmd,
                SaveCmd);
            BuildSettingsListing();
        }

        private void BuildSettingsListing()
        {
            Dictionary<string, object> sourceMap = new()
            {
                ["General: disable Start Encode when..."] = _appConfM.General,
                ["Overwrite Handling"] = _appConfM.Overwrite,
                ["SMTP"] = _appConfM.Smtp,
                ["Language/语言"] = _appConfM.Lang
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
                                ["en", "zh-cn", "zh-tw"]);
                            break;
                    }
                }
                SettingsListing.Add(container);
            }
        }

        #region Setting control elements
        private static void AddCheckboxItem(AppConfContainer container, string text, object source, string propertyPath)
        {
            CheckBox cb = new()
            {
                HorizontalAlignment = HorizontalAlignment.Right
            };
            cb.SetBinding(CheckBox.IsCheckedProperty, new Binding(propertyPath) { Source=source, Mode=BindingMode.TwoWay });
            container.Items.Add(new AppConfItem { Text = text, Content = cb });
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
    }
}
