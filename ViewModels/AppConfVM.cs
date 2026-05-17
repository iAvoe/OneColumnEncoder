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

namespace OneColumnEncoder.ViewModels
{
    public class AppConfVM : BaseVM
    {
        // SSOT Store for app settings
        private readonly AppConfS _appConfStore;

        // Commands for UI interactions
        public CloseModalCmd CloseCmd { get; }
        public SaveAppConfCmd SaveCmd { get; }
        public LoadAppConfCmd LoadCmd { get; }

        // Save and Cancel buttons
        // public ConfirmCancelButtonsVM ConfirmCancelButtons { get; }
        public SmtpConfirmCancelButtonsVM SmtpConfirmCancelButtons { get; }

        // Settings for binding
        public AppConfS.GeneralSettings General => _appConfStore.General;
        public AppConfS.OverwriteSettings Overwrite => _appConfStore.Overwrite;
        public AppConfS.SmtpSettings Smtp => _appConfStore.Smtp;

        // Grouped settings listing for the ListView
        public ObservableCollection<AppConfContainer> SettingsListing { get; } = [];

        public AppConfVM(ModalNavS modalNavS, AppConfS appConfS, Action closeAction)
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
            AppConfContainer general = new() { Header = "General: disable Start Encode when..." };
            AddCheckboxItem(general, "PC is off-grid / on battery", _appConfStore.General, nameof(AppConfS.GeneralSettings.OffGrid));
            AddCheckboxItem(general, "Insufficient RAM", _appConfStore.General, nameof(AppConfS.GeneralSettings.InsufficientRAM));
            AddCheckboxItem(general, "Insufficient disk space", _appConfStore.General, nameof(AppConfS.GeneralSettings.InsufficientDiskSpace));
            AddCheckboxItem(general, "Invalid file name for OS", _appConfStore.General, nameof(AppConfS.GeneralSettings.OSFileNameInvalid));
            AddCheckboxItem(general, "Invalid file name for FTP", _appConfStore.General, nameof(AppConfS.GeneralSettings.FTPFileNameInvalid));
            AddCheckboxItem(general, "Lack of write permission", _appConfStore.General, nameof(AppConfS.GeneralSettings.NoWritePermission));
            AddCheckboxItem(general, "Overwriting a file", _appConfStore.General, nameof(AppConfS.GeneralSettings.IsOverwriting));
            SettingsListing.Add(general);

            AppConfContainer overwrite = new() { Header = "Overwrite Handling" };
            AddTextboxItem(overwrite, "Long press megabyte divisor", _appConfStore.Overwrite, nameof(AppConfS.OverwriteSettings.LongPressMegabyteDivisor));
            AddTextboxItem(overwrite, "Min long press MS", _appConfStore.Overwrite, nameof(AppConfS.OverwriteSettings.MinLongPressMs));
            AddTextboxItem(overwrite, "Max long press MS", _appConfStore.Overwrite, nameof(AppConfS.OverwriteSettings.MaxLongPressMs));
            SettingsListing.Add(overwrite);

            AppConfContainer smtp = new() { Header = "SMTP" };
            AddTextboxItem(smtp, "Server URL", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.ServerUrl));
            AddTextboxItem(smtp, "Port", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.Port));
            AddCheckboxItem(smtp, "Use SSL", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.UseSSL));
            AddTextboxItem(smtp, "Username", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.Username));
            AddPasswordBoxItem(smtp, "Password", () => _appConfStore.Smtp.Password, v => _appConfStore.Smtp.Password = v);
            AddTextboxItem(smtp, "From Email", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.FromEmail));
            AddTextboxItem(smtp, "To Email", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.ToEmail));
            AddCheckboxItem(smtp, "Notify on Success", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.NotifyOnSuccess));
            AddCheckboxItem(smtp, "Notify on Failure", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.NotifyOnFailure));
            AddCheckboxItem(smtp, "Notify on No Input", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.NotifyOnNoInput));
            AddTextboxItem(smtp, "Success Threshold (min)", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.NotifySuccessTaskThresholdMinutes));
            AddTextboxItem(smtp, "Failure Threshold (min)", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.NotifyFailureTaskThresholdMinutes));
            AddTextboxItem(smtp, "No Input Threshold (min)", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.NotifyNoInputTaskThresholdMinutes));
            SettingsListing.Add(smtp);

            AppConfContainer lang = new() { Header = "Language/语言" };
            AddTextboxItem(lang, "Language Code (e.g. en, zh)", _appConfStore.Lang, nameof(AppConfS.Language.LanguageCode));
            SettingsListing.Add(lang);
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
    }
}
