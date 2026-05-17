using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OneColumnEncoder.Commands;
using OneColumnEncoder.Components;
using OneColumnEncoder.Stores;
using System.Windows.Input;
using OneColumnEncoder.Models;

namespace OneColumnEncoder.ViewModels
{
    public class AppConfVM : BaseVM
    {
        // SSOT Store for app settings
        private readonly AppConfM _appConfStore;

        // Commands for UI interactions
        public CloseModalCmd CloseCmd { get; }
        public SaveAppConfCmd SaveCmd { get; }
        public LoadAppConfCmd LoadCmd { get; }

        // Save and Cancel buttons
        // public ConfirmCancelButtonsVM ConfirmCancelButtons { get; }
        public SmtpConfirmCancelButtonsVM SmtpConfirmCancelButtons { get; }

        // Settings for binding
        public AppConfM.GeneralSettings General => _appConfStore.General;
        public AppConfM.OverwriteSettings Overwrite => _appConfStore.Overwrite;
        public AppConfM.SmtpSettings Smtp => _appConfStore.Smtp;

        // Grouped settings listing for the ListView
        public ObservableCollection<AppConfContainer> SettingsListing { get; } = [];

        public AppConfVM(ModalNavS modalNavS, AppConfM appConfS, Action closeAction)
        {
            _appConfStore = appConfS;
            CloseCmd = new CloseModalCmd(modalNavS, closeAction);
            SaveCmd = new SaveAppConfCmd(appConfS, modalNavS, closeAction);
            LoadCmd = new LoadAppConfCmd(appConfS);
            // ConfirmCancelButtons = new ConfirmCancelButtonsVM(CloseCmd, SaveCmd);
            SmtpConfirmCancelButtons = new SmtpConfirmCancelButtonsVM(CloseCmd, SaveCmd); // TODO: Smtp test command
            BuildSettingsListing();
        }

        private void BuildSettingsListing()
        {
            Dictionary<string, object> sourceMap = new()
            {
                ["General: disable Start Encode when..."] = _appConfStore.General,
                ["Overwrite Handling"] = _appConfStore.Overwrite,
                ["SMTP"] = _appConfStore.Smtp,
                ["Language/语言"] = _appConfStore.Lang
            };

            foreach (var group in SettinglistProviderM.GetAllSettings().GroupBy(s => s.GroupName))
            {
                var container = new AppConfContainer { Header = group.Key };
                var source = sourceMap[group.Key];
                foreach (var setting in group)
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
                                () => _appConfStore.Smtp.Password,
                                v => _appConfStore.Smtp.Password = v);
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

        private static void AddCheckboxItem(AppConfContainer container, string text, object source, string propertyPath)
        {
            var cb = new CheckBox();
            cb.SetBinding(CheckBox.IsCheckedProperty, new Binding(propertyPath) { Source=source, Mode=BindingMode.TwoWay });
            container.Items.Add(new AppConfItem { Text = text, Content = cb });
        }

        private static void AddTextboxItem(AppConfContainer container, string text, object source, string propertyPath)
        {
            var tb = new TextBox { Width = 200, HorizontalAlignment = HorizontalAlignment.Right };
            tb.SetBinding(TextBox.TextProperty, new Binding(propertyPath) { Source = source, Mode = BindingMode.TwoWay });
            container.Items.Add(new AppConfItem { Text = text, Content = tb });
        }

        private static void AddPasswordBoxItem(AppConfContainer container, string text, Func<string> getter, Action<string> setter)
        {
            var pb = new PasswordBox
            {
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Right,
                Password = getter()
            };
            pb.PasswordChanged += (_, _) => setter(pb.Password);
            container.Items.Add(new AppConfItem { Text = text, Content = pb });
        }

        private void AddDropdownItem(AppConfContainer container, string text, object source, string propertyPath, string[] options)
        {
            var currentValue = source.GetType().GetProperty(propertyPath)?.GetValue(source) as string ?? options[0];
            var items = options.Select(o => new DropdownItemM(o)).ToList();

            var dropdownVM = new DropdownMenuVM();
            foreach (var item in items)
                dropdownVM.Items.Add(item);
            dropdownVM.SelectedItem = items.FirstOrDefault(i => i.Title == currentValue) ?? items[0];

            dropdownVM.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(DropdownMenuVM.SelectedItem) && dropdownVM.SelectedItem is not null)
                {
                    var prop = source.GetType().GetProperty(propertyPath);
                    prop?.SetValue(source, dropdownVM.SelectedItem.Title);
                }
            };

            var dropdown = new DropdownMenu
            {
                DataContext = dropdownVM,
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            container.Items.Add(new AppConfItem { Text = text, Content = dropdown });
        }
    }
}
