namespace OneColumnEncoder.Models
{
    public static class SettinglistProviderM
    {
        public static List<SettingItemDefinitionM> GetAllSettings() =>
        [
            .. GetOverwriteSettings(),
            .. GetSmtpSettings(),
            .. GetLanguageSettings()
        ];

        public static List<SettingItemDefinitionM> GetOverwriteSettings() =>
        [
            new(UICaptionProviderM.AppConf.Groups.Overwrite,
                UILangProviderM.Current["Setting.Overwrite.LongPressDivisor"],
                SettingControlType.TextBox,
                nameof(AppConfM.OverwriteSettings.LongPressMegabyteDivisor),
                MinValue: 1),
            new(UICaptionProviderM.AppConf.Groups.Overwrite,
                UILangProviderM.Current["Setting.Overwrite.MinLongPress"],
                SettingControlType.TextBox,
                nameof(AppConfM.OverwriteSettings.MinLongPressMs),
                MinValue: 0),
            new(UICaptionProviderM.AppConf.Groups.Overwrite,
                UILangProviderM.Current["Setting.Overwrite.MaxLongPress"],
                SettingControlType.TextBox,
                nameof(AppConfM.OverwriteSettings.MaxLongPressMs),
                MinValue: 0)
        ];

        public static List<SettingItemDefinitionM> GetSmtpSettings() =>
        [
            new(UICaptionProviderM.AppConf.Groups.Smtp,
                UILangProviderM.Current["Setting.Smtp.ServerUrl"],
                SettingControlType.TextBox,
                nameof(AppConfM.SmtpSettings.ServerUrl)),
            new(UICaptionProviderM.AppConf.Groups.Smtp,
                UILangProviderM.Current["Setting.Smtp.Port"],
                SettingControlType.TextBox,
                nameof(AppConfM.SmtpSettings.Port),
                MinValue: 25,
                MaxValue: 65535),
            new(UICaptionProviderM.AppConf.Groups.Smtp,
                UILangProviderM.Current["Setting.Smtp.UseSSL"],
                SettingControlType.CheckBox,
                nameof(AppConfM.SmtpSettings.UseSSL)),
            new(UICaptionProviderM.AppConf.Groups.Smtp,
                UILangProviderM.Current["Setting.Smtp.Username"],
                SettingControlType.TextBox,
                nameof(AppConfM.SmtpSettings.Username)),
            new(UICaptionProviderM.AppConf.Groups.Smtp,
                UILangProviderM.Current["Setting.Smtp.Password"],
                SettingControlType.PasswordBox,
                nameof(AppConfM.SmtpSettings.Password)),
            new(UICaptionProviderM.AppConf.Groups.Smtp,
                UILangProviderM.Current["Setting.Smtp.FromEmail"],
                SettingControlType.TextBox,
                nameof(AppConfM.SmtpSettings.FromEmail)),
            new(UICaptionProviderM.AppConf.Groups.Smtp,
                UILangProviderM.Current["Setting.Smtp.ToEmail"],
                SettingControlType.TextBox,
                nameof(AppConfM.SmtpSettings.ToEmail)),
            new(UICaptionProviderM.AppConf.Groups.Smtp,
                UILangProviderM.Current["Setting.Smtp.NotifySuccess"],
                SettingControlType.CheckBox,
                nameof(AppConfM.SmtpSettings.NotifyOnSuccess)),
            new(UICaptionProviderM.AppConf.Groups.Smtp,
                UILangProviderM.Current["Setting.Smtp.NotifyFailure"],
                SettingControlType.CheckBox,
                nameof(AppConfM.SmtpSettings.NotifyOnFailure)),
            new(UICaptionProviderM.AppConf.Groups.Smtp,
                UILangProviderM.Current["Setting.Smtp.NotifyAFK"],
                SettingControlType.CheckBox,
                nameof(AppConfM.SmtpSettings.NotifyOnNoInput)),
            new(UICaptionProviderM.AppConf.Groups.Smtp,
                UILangProviderM.Current["Setting.Smtp.SuccessThreshold"],
                SettingControlType.TextBox,
                nameof(AppConfM.SmtpSettings.NotifySuccessThresholdMin),
                MinValue: 0),
            new(UICaptionProviderM.AppConf.Groups.Smtp,
                UILangProviderM.Current["Setting.Smtp.FailureThreshold"],
                SettingControlType.TextBox,
                nameof(AppConfM.SmtpSettings.NotifyFailureThresholdMin),
                MinValue: 0),
            new(UICaptionProviderM.AppConf.Groups.Smtp,
                UILangProviderM.Current["Setting.Smtp.AFKThreshold"],
                SettingControlType.TextBox,
                nameof(AppConfM.SmtpSettings.NotifyAfterNoInputThresholdMin),
                MinValue: 0),
        ];

        public static List<SettingItemDefinitionM> GetLanguageSettings() =>
        [
            new(UICaptionProviderM.AppConf.Groups.Language,
                UILangProviderM.Current["Setting.Language.Select"],
                SettingControlType.Dropdown,
                nameof(AppConfM.Language.LanguageCode))
        ];
    }
}
