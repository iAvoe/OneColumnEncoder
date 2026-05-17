using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Stores
{
    public static class SettinglistProviderS
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
            new SettingItemDefinitionM("General: disable Start Encode when...", "PC is off-grid / on battery", SettingControlType.CheckBox, nameof(AppConfS.GeneralSettings.OffGrid)),
            new SettingItemDefinitionM("General: disable Start Encode when...", "Insufficient RAM", SettingControlType.CheckBox, nameof(AppConfS.GeneralSettings.InsufficientRAM)),
            new SettingItemDefinitionM("General: disable Start Encode when...", "Insufficient Disk Space", SettingControlType.CheckBox, nameof(AppConfS.GeneralSettings.InsufficientDiskSpace)),
            new SettingItemDefinitionM("General: disable Start Encode when...", "Filename is Invalid for OS", SettingControlType.CheckBox, nameof(AppConfS.GeneralSettings.OSFileNameInvalid)),
            new SettingItemDefinitionM("General: disable Start Encode when...", "Filename is Invalid for FTP", SettingControlType.CheckBox, nameof(AppConfS.GeneralSettings.FTPFileNameInvalid)),
            new SettingItemDefinitionM("General: disable Start Encode when...", "Lack of Write Permission", SettingControlType.CheckBox, nameof(AppConfS.GeneralSettings.NoWritePermission)),
            new SettingItemDefinitionM("General: disable Start Encode when...", "Overwriting a File", SettingControlType.CheckBox, nameof(AppConfS.GeneralSettings.IsOverwriting)),
        ];

        public static List<SettingItemDefinitionM> GetOverwriteSettings() =>
        [
            new SettingItemDefinitionM("Overwrite Handling", "Long Press Megabyte Divisor", SettingControlType.TextBox, nameof(AppConfS.OverwriteSettings.LongPressMegabyteDivisor)),
            new SettingItemDefinitionM("Overwrite Handling", "Minimum Long Press Duration (ms)", SettingControlType.TextBox, nameof(AppConfS.OverwriteSettings.MinLongPressMs)),
            new SettingItemDefinitionM("Overwrite Handling", "Maximum Long Press Duration (ms)", SettingControlType.TextBox, nameof(AppConfS.OverwriteSettings.MaxLongPressMs)),
        ];

        public static List<SettingItemDefinitionM> GetSmtpSettings() =>
        [
            new SettingItemDefinitionM("SMTP", "Server URL", SettingControlType.TextBox, nameof(AppConfS.SmtpSettings.ServerUrl)),
            new SettingItemDefinitionM("SMTP", "Port", SettingControlType.TextBox, nameof(AppConfS.SmtpSettings.Port)),
            new SettingItemDefinitionM("SMTP", "Use SSL", SettingControlType.CheckBox, nameof(AppConfS.SmtpSettings.UseSSL)),
            new SettingItemDefinitionM("SMTP", "Username", SettingControlType.TextBox, nameof(AppConfS.SmtpSettings.Username)),
            new SettingItemDefinitionM("SMTP", "Password", SettingControlType.PasswordBox, nameof(AppConfS.SmtpSettings.Password)),
            new SettingItemDefinitionM("SMTP", "From Email Address", SettingControlType.TextBox, nameof(AppConfS.SmtpSettings.FromEmail)),
            new SettingItemDefinitionM("SMTP", "To Email Address", SettingControlType.TextBox, nameof(AppConfS.SmtpSettings.ToEmail)),
            new SettingItemDefinitionM("SMTP", "Notify on Success", SettingControlType.CheckBox, nameof(AppConfS.SmtpSettings.NotifyOnSuccess)),
            new SettingItemDefinitionM("SMTP", "Notify on Failure", SettingControlType.CheckBox, nameof(AppConfS.SmtpSettings.NotifyOnFailure)),
            new SettingItemDefinitionM("SMTP", "Notify when AFK", SettingControlType.CheckBox, nameof(AppConfS.SmtpSettings.NotifyOnNoInput)),
            new SettingItemDefinitionM("SMTP", "Notify on Success Threshold (min)", SettingControlType.TextBox, nameof(AppConfS.SmtpSettings.NotifySuccessThresholdMin)),
            new SettingItemDefinitionM("SMTP", "Notify on Failure Threshold (min)", SettingControlType.TextBox, nameof(AppConfS.SmtpSettings.NotifyFailureThresholdMin)),
            new SettingItemDefinitionM("SMTP", "Notify on AFK for (min)", SettingControlType.TextBox, nameof(AppConfS.SmtpSettings.NotifyAfterNoInputThresholdMin)),
        ];

        public static List<SettingItemDefinitionM> GetLanguageSettings() =>
        [
            new SettingItemDefinitionM("Language/语言", "Language Code (e.g. en, zh)", SettingControlType.Dropdown, nameof(AppConfS.Language.LanguageCode))
        ];
    }
}
