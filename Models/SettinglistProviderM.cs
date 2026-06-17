namespace OneColumnEncoder.Models
{
    public static class SettinglistProviderM
    {
        public static List<SettingItemDefinitionM> GetAllSettings() =>
        [
            .. GetOverwriteSettings(),
            .. GetLanguageSettings(),
            .. GetInitModeSettings()
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

        public static List<SettingItemDefinitionM> GetLanguageSettings() =>
        [
            new(UICaptionProviderM.AppConf.Groups.Language,
                UILangProviderM.Current["Setting.Language.Select"],
                SettingControlType.Dropdown,
                nameof(AppConfM.Language.LanguageCode))
        ];

        public static List<SettingItemDefinitionM> GetInitModeSettings() =>
        [
            new(UICaptionProviderM.AppConf.Groups.InitMode,
                UILangProviderM.Current["Setting.InitMode.IsFirstLaunch"],
                SettingControlType.CheckBox,
                nameof(AppConfM.IsFirstLaunch))
        ];
    }
}
