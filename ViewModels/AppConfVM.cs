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
        public ICommand CloseCmd { get; }
        public ICommand SaveCmd { get; }
        public ICommand LoadCmd { get; }
        // Expose settings for binding
        public AppConfS.GeneralSettings General => _appConfStore.General;
        public AppConfS.OverwriteSettings Overwrite => _appConfStore.Overwrite;
        public AppConfS.SmtpSettings Smtp => _appConfStore.Smtp;
        // Grouped settings listing for the ListView
        public ObservableCollection<AppConfContainer> SettingsListing { get; } = [];

        public AppConfVM(ModalNavS modalNavS, AppConfS appConfS)
        {
            CloseCmd = new CloseModalCmd(modalNavS);
            SaveCmd = new SaveAppConfCmd(appConfS);
            LoadCmd = new LoadAppConfCmd(appConfS);
            _appConfStore = appConfS;

            BuildSettingsListing();
        }

        private void BuildSettingsListing()
        {
            var general = new AppConfContainer { Header = "General" };
            AddCheckboxItem(general, "Allow Ctrl+Click", _appConfStore.General, nameof(AppConfS.GeneralSettings.AllowCtrlClick));
            AddCheckboxItem(general, "On External Power", _appConfStore.General, nameof(AppConfS.GeneralSettings.OnExternalPower));
            AddCheckboxItem(general, "Sufficient RAM", _appConfStore.General, nameof(AppConfS.GeneralSettings.SufficientRAM));
            AddCheckboxItem(general, "Sufficient Disk Space", _appConfStore.General, nameof(AppConfS.GeneralSettings.SufficientDiskSpace));
            AddCheckboxItem(general, "OS File Name Valid", _appConfStore.General, nameof(AppConfS.GeneralSettings.OSFileNameValid));
            AddCheckboxItem(general, "FTP File Name Valid", _appConfStore.General, nameof(AppConfS.GeneralSettings.FTPFileNameValid));
            AddCheckboxItem(general, "Output Folder Writable", _appConfStore.General, nameof(AppConfS.GeneralSettings.OutputFolderWritable));
            AddCheckboxItem(general, "No Overwrite", _appConfStore.General, nameof(AppConfS.GeneralSettings.NoOverwrite));
            SettingsListing.Add(general);

            var overwrite = new AppConfContainer { Header = "Overwrite" };
            AddTextboxItem(overwrite, "Long Press Megabyte Divisor", _appConfStore.Overwrite, nameof(AppConfS.OverwriteSettings.LongPressMegabyteDivisor));
            AddTextboxItem(overwrite, "Min Long Press (ms)", _appConfStore.Overwrite, nameof(AppConfS.OverwriteSettings.MinLongPressMs));
            AddTextboxItem(overwrite, "Max Long Press (ms)", _appConfStore.Overwrite, nameof(AppConfS.OverwriteSettings.MaxLongPressMs));
            SettingsListing.Add(overwrite);

            var smtp = new AppConfContainer { Header = "SMTP" };
            AddTextboxItem(smtp, "Server URL", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.ServerUrl));
            AddTextboxItem(smtp, "Port", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.Port));
            AddCheckboxItem(smtp, "Use SSL", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.UseSSL));
            AddTextboxItem(smtp, "Username", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.Username));
            AddTextboxItem(smtp, "Password", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.Password));
            AddTextboxItem(smtp, "From Email", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.FromEmail));
            AddTextboxItem(smtp, "To Email", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.ToEmail));
            AddCheckboxItem(smtp, "Notify on Success", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.NotifyOnSuccess));
            AddCheckboxItem(smtp, "Notify on Failure", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.NotifyOnFailure));
            AddCheckboxItem(smtp, "Notify on No Input", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.NotifyOnNoInput));
            AddTextboxItem(smtp, "Success Threshold (min)", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.NotifySuccessTaskThresholdMinutes));
            AddTextboxItem(smtp, "Failure Threshold (min)", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.NotifyFailureTaskThresholdMinutes));
            AddTextboxItem(smtp, "No Input Threshold (min)", _appConfStore.Smtp, nameof(AppConfS.SmtpSettings.NotifyNoInputTaskThresholdMinutes));
            SettingsListing.Add(smtp);
        }

        private static void AddCheckboxItem(AppConfContainer container, string text, object source, string propertyPath)
        {
            var cb = new CheckBox();
            cb.SetBinding(CheckBox.IsCheckedProperty, new Binding(propertyPath) { Source = source, Mode = BindingMode.TwoWay });
            container.Items.Add(new AppConfItem { Text = text, Content = cb });
        }

        private static void AddTextboxItem(AppConfContainer container, string text, object source, string propertyPath)
        {
            var tb = new TextBox { Width = 200, HorizontalAlignment = HorizontalAlignment.Right };
            tb.SetBinding(TextBox.TextProperty, new Binding(propertyPath) { Source = source, Mode = BindingMode.TwoWay });
            container.Items.Add(new AppConfItem { Text = text, Content = tb });
        }
    }
}
