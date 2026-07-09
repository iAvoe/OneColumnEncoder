namespace OneColumnEncoder.Models
{
    public static class SettinglistProviderM
    {
        public static List<SettingItemDefinitionM> GetAllSettings() =>
        [
            .. GetOverwriteSettings(),
            .. GetLanguageSettings(),
            .. GetInitModeSettings(),
            .. GetBypassSettings()
        ];

        private static AppConfLangProviderM Lang => AppConfLangProviderM.Current;

        public static List<SettingItemDefinitionM> GetOverwriteSettings() =>
        [
            new(UICaptionProviderM.AppConf.Groups.Overwrite,
                Lang["Setting.Overwrite.LongPressDivisor"],
                SettingControlType.TextBox,
                nameof(AppConfM.OverwriteSettings.LongPressMegabyteDivisor),
                MinValue: 1),
            new(UICaptionProviderM.AppConf.Groups.Overwrite,
                Lang["Setting.Overwrite.MinLongPress"],
                SettingControlType.TextBox,
                nameof(AppConfM.OverwriteSettings.MinLongPressMs),
                MinValue: 0),
            new(UICaptionProviderM.AppConf.Groups.Overwrite,
                Lang["Setting.Overwrite.MaxLongPress"],
                SettingControlType.TextBox,
                nameof(AppConfM.OverwriteSettings.MaxLongPressMs),
                MinValue: 0)
        ];

        public static List<SettingItemDefinitionM> GetLanguageSettings() =>
        [
            new(UICaptionProviderM.AppConf.Groups.Language,
                Lang["Setting.Language.Select"],
                SettingControlType.Dropdown,
                nameof(AppConfM.Language.LanguageCode))
        ];

        public static List<SettingItemDefinitionM> GetInitModeSettings() =>
        [
            new(UICaptionProviderM.AppConf.Groups.InitMode,
                Lang["Setting.InitMode.IsFirstLaunch"],
                SettingControlType.CheckBox,
                nameof(AppConfM.IsFirstLaunch))
        ];

        public static List<SettingItemDefinitionM> GetBypassSettings() =>
        [
            new(UICaptionProviderM.AppConf.Groups.Bypass,
                Lang["Setting.Bypass.SrcValGroup"],
                SettingControlType.CheckBox,
                nameof(AppConfM.BypassSettings.BypassSrcValidationGroup)),
            new(UICaptionProviderM.AppConf.Groups.Bypass,
                Lang["Setting.Bypass.EncTermsValGroup"],
                SettingControlType.CheckBox,
                nameof(AppConfM.BypassSettings.BypassEncTermsValidationGroup))
        ];
    }
}
