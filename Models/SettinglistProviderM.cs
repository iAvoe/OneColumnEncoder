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
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.General, "Not off-grid / powering via battery", SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.OffGrid)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.General, "Sufficient RAM availability", SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.InsufficientRAM)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.General, "Sufficient disk space availability", SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.InsufficientDiskSpace)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.General, "Output filename is valid for OS", SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.OSFileNameInvalid)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.General, "Output filename maybe valid for FTP (Pseudo-UTF-8)", SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.FTPFileNameInvalid)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.General, "Write permission in output folder", SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.NoWritePermission)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.General, "Output does not overwrite existing file", SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.IsOverwriting)),
        ];

        public static List<SettingItemDefinitionM> GetOverwriteSettings() =>
        [
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Overwrite, "Long Press Megabyte Divisor", SettingControlType.TextBox, nameof(AppConfM.OverwriteSettings.LongPressMegabyteDivisor)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Overwrite, "Minimum Long Press Duration (ms)", SettingControlType.TextBox, nameof(AppConfM.OverwriteSettings.MinLongPressMs)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Overwrite, "Maximum Long Press Duration (ms)", SettingControlType.TextBox, nameof(AppConfM.OverwriteSettings.MaxLongPressMs)),
        ];

        public static List<SettingItemDefinitionM> GetSmtpSettings() =>
        [
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Smtp, "Server URL", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.ServerUrl)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Smtp, "Port", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.Port)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Smtp, "Use SSL", SettingControlType.CheckBox, nameof(AppConfM.SmtpSettings.UseSSL)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Smtp, "Username", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.Username)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Smtp, "Password", SettingControlType.PasswordBox, nameof(AppConfM.SmtpSettings.Password)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Smtp, "From Email Address", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.FromEmail)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Smtp, "To Email Address", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.ToEmail)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Smtp, "Notify on Success", SettingControlType.CheckBox, nameof(AppConfM.SmtpSettings.NotifyOnSuccess)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Smtp, "Notify on Failure", SettingControlType.CheckBox, nameof(AppConfM.SmtpSettings.NotifyOnFailure)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Smtp, "Notify when AFK", SettingControlType.CheckBox, nameof(AppConfM.SmtpSettings.NotifyOnNoInput)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Smtp, "Notify on Success Threshold (min)", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.NotifySuccessThresholdMin)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Smtp, "Notify on Failure Threshold (min)", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.NotifyFailureThresholdMin)),
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Smtp, "Notify on AFK for (min)", SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.NotifyAfterNoInputThresholdMin)),
        ];

        public static List<SettingItemDefinitionM> GetLanguageSettings() =>
        [
            new SettingItemDefinitionM(UICaptionProviderM.AppConf.Groups.Language, "Select Language", SettingControlType.Dropdown, nameof(AppConfM.Language.LanguageCode))
        ];
    }
}
