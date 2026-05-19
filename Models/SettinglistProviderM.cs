using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Models
{
    public static class SettinglistProviderM
    {
        public static List<SettingItemDefinitionM> GetAllSettings() =>
        [
            .. GetGeneralSettings(),
            .. GetOverwriteSettings(),
            .. GetSmtpSettings(),
            .. GetLanguageSettings()
        ];

        public static List<SettingItemDefinitionM> GetGeneralSettings() =>
        [
            new SettingItemDefinitionM("General: disable Start Encode when...", "Not off-grid / powering via battery", SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.OffGrid)),
            new SettingItemDefinitionM("General: disable Start Encode when...", "Sufficient RAM availability", SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.InsufficientRAM)),
            new SettingItemDefinitionM("General: disable Start Encode when...", "Sufficient disk space availability", SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.InsufficientDiskSpace)),
            new SettingItemDefinitionM("General: disable Start Encode when...", "Output filename is valid for OS", SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.OSFileNameInvalid)),
            new SettingItemDefinitionM("General: disable Start Encode when...", "Output filename maybe valid for FTP (Pseudo-UTF-8)", SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.FTPFileNameInvalid)),
            new SettingItemDefinitionM("General: disable Start Encode when...", "Write permission in output folder", SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.NoWritePermission)),
            new SettingItemDefinitionM("General: disable Start Encode when...", "Output does not overwrite existing file", SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.IsOverwriting)),
        ];

        public static List<SettingItemDefinitionM> GetOverwriteSettings() =>
        [
            new SettingItemDefinitionM("Overwrite Handling", "Long Press Megabyte Divisor", SettingControlType.TextBox, nameof(AppConfM.OverwriteSettings.LongPressMegabyteDivisor)),
            new SettingItemDefinitionM("Overwrite Handling", "Minimum Long Press Duration (ms)", SettingControlType.TextBox, nameof(AppConfM.OverwriteSettings.MinLongPressMs)),
            new SettingItemDefinitionM("Overwrite Handling", "Maximum Long Press Duration (ms)", SettingControlType.TextBox, nameof(AppConfM.OverwriteSettings.MaxLongPressMs)),
        ];

        public static List<SettingItemDefinitionM> GetSmtpSettings() =>
        [
            new SettingItemDefinitionM("SMTP", "Server URL", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.ServerUrl)),
            new SettingItemDefinitionM("SMTP", "Port", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.Port)),
            new SettingItemDefinitionM("SMTP", "Use SSL", SettingControlType.CheckBox, nameof(AppConfM.SmtpSettings.UseSSL)),
            new SettingItemDefinitionM("SMTP", "Username", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.Username)),
            new SettingItemDefinitionM("SMTP", "Password", SettingControlType.PasswordBox, nameof(AppConfM.SmtpSettings.Password)),
            new SettingItemDefinitionM("SMTP", "From Email Address", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.FromEmail)),
            new SettingItemDefinitionM("SMTP", "To Email Address", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.ToEmail)),
            new SettingItemDefinitionM("SMTP", "Notify on Success", SettingControlType.CheckBox, nameof(AppConfM.SmtpSettings.NotifyOnSuccess)),
            new SettingItemDefinitionM("SMTP", "Notify on Failure", SettingControlType.CheckBox, nameof(AppConfM.SmtpSettings.NotifyOnFailure)),
            new SettingItemDefinitionM("SMTP", "Notify when AFK", SettingControlType.CheckBox, nameof(AppConfM.SmtpSettings.NotifyOnNoInput)),
            new SettingItemDefinitionM("SMTP", "Notify on Success Threshold (min)", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.NotifySuccessThresholdMin)),
            new SettingItemDefinitionM("SMTP", "Notify on Failure Threshold (min)", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.NotifyFailureThresholdMin)),
            new SettingItemDefinitionM("SMTP", "Notify on AFK for (min)", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.NotifyAfterNoInputThresholdMin)),
        ];

        public static List<SettingItemDefinitionM> GetLanguageSettings() =>
        [
            new SettingItemDefinitionM("Language/语言", "Select Language", SettingControlType.Dropdown, nameof(AppConfM.Language.LanguageCode))
        ];
    }
}
