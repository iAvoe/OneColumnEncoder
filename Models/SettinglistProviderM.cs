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
            new(UICaptionProviderM.AppConf.Groups.General, UILangProviderM.Current["Setting.General.NotOffGrid"], SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.OffGrid)),
            new(UICaptionProviderM.AppConf.Groups.General, UILangProviderM.Current["Setting.General.SufficientRAM"], SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.InsufficientRAM)),
            new(UICaptionProviderM.AppConf.Groups.General, UILangProviderM.Current["Setting.General.SufficientDisk"], SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.InsufficientDiskSpace)),
            new(UICaptionProviderM.AppConf.Groups.General, UILangProviderM.Current["Setting.General.OSFilename"], SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.OSFileNameInvalid)),
            new(UICaptionProviderM.AppConf.Groups.General, UILangProviderM.Current["Setting.General.FTPFilename"], SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.FTPFileNameInvalid)),
            new(UICaptionProviderM.AppConf.Groups.General, UILangProviderM.Current["Setting.General.WritePermission"], SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.NoWritePermission)),
            new(UICaptionProviderM.AppConf.Groups.General, UILangProviderM.Current["Setting.General.NotOverwrite"], SettingControlType.CheckBox, nameof(AppConfM.GeneralSettings.IsOverwriting)),
        ];

        public static List<SettingItemDefinitionM> GetOverwriteSettings() =>
        [
            new(UICaptionProviderM.AppConf.Groups.Overwrite, UILangProviderM.Current["Setting.Overwrite.LongPressDivisor"], SettingControlType.TextBox, nameof(AppConfM.OverwriteSettings.LongPressMegabyteDivisor)),
            new(UICaptionProviderM.AppConf.Groups.Overwrite, UILangProviderM.Current["Setting.Overwrite.MinLongPress"], SettingControlType.TextBox, nameof(AppConfM.OverwriteSettings.MinLongPressMs)),
            new(UICaptionProviderM.AppConf.Groups.Overwrite, UILangProviderM.Current["Setting.Overwrite.MaxLongPress"], SettingControlType.TextBox, nameof(AppConfM.OverwriteSettings.MaxLongPressMs)),
        ];

        public static List<SettingItemDefinitionM> GetSmtpSettings() =>
        [
            new(UICaptionProviderM.AppConf.Groups.Smtp, UILangProviderM.Current["Setting.Smtp.ServerUrl"], SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.ServerUrl)),
            new(UICaptionProviderM.AppConf.Groups.Smtp, UILangProviderM.Current["Setting.Smtp.Port"], SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.Port)),
            new(UICaptionProviderM.AppConf.Groups.Smtp, UILangProviderM.Current["Setting.Smtp.UseSSL"], SettingControlType.CheckBox, nameof(AppConfM.SmtpSettings.UseSSL)),
            new(UICaptionProviderM.AppConf.Groups.Smtp, UILangProviderM.Current["Setting.Smtp.Username"], SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.Username)),
            new(UICaptionProviderM.AppConf.Groups.Smtp, UILangProviderM.Current["Setting.Smtp.Password"], SettingControlType.PasswordBox, nameof(AppConfM.SmtpSettings.Password)),
            new(UICaptionProviderM.AppConf.Groups.Smtp, UILangProviderM.Current["Setting.Smtp.FromEmail"], SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.FromEmail)),
            new(UICaptionProviderM.AppConf.Groups.Smtp, UILangProviderM.Current["Setting.Smtp.ToEmail"], SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.ToEmail)),
            new(UICaptionProviderM.AppConf.Groups.Smtp, UILangProviderM.Current["Setting.Smtp.NotifySuccess"], SettingControlType.CheckBox, nameof(AppConfM.SmtpSettings.NotifyOnSuccess)),
            new(UICaptionProviderM.AppConf.Groups.Smtp, UILangProviderM.Current["Setting.Smtp.NotifyFailure"], SettingControlType.CheckBox, nameof(AppConfM.SmtpSettings.NotifyOnFailure)),
            new(UICaptionProviderM.AppConf.Groups.Smtp, UILangProviderM.Current["Setting.Smtp.NotifyAFK"], SettingControlType.CheckBox, nameof(AppConfM.SmtpSettings.NotifyOnNoInput)),
            new(UICaptionProviderM.AppConf.Groups.Smtp, UILangProviderM.Current["Setting.Smtp.SuccessThreshold"], SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.NotifySuccessThresholdMin)),
            new(UICaptionProviderM.AppConf.Groups.Smtp, UILangProviderM.Current["Setting.Smtp.FailureThreshold"], SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.NotifyFailureThresholdMin)),
            new(UICaptionProviderM.AppConf.Groups.Smtp, UILangProviderM.Current["Setting.Smtp.AFKThreshold"], SettingControlType.TextBox, nameof(AppConfM.SmtpSettings.NotifyAfterNoInputThresholdMin)),
        ];

        public static List<SettingItemDefinitionM> GetLanguageSettings() =>
        [
            new(UICaptionProviderM.AppConf.Groups.Language, UILangProviderM.Current["Setting.Language.Select"], SettingControlType.Dropdown, nameof(AppConfM.Language.LanguageCode))
        ];
    }
}