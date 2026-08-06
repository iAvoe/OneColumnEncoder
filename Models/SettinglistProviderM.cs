namespace OneColumnEncoder.Models
{
    public static class SettinglistProviderM
    {
        public static List<SettingItemDefinitionM> GetAllSettings() =>
        [
            .. GetOverwriteSettings(),
            .. GetLanguageSettings(),
            .. GetInitModeSettings(),
            .. GetLogSettings()
        ];

        private static AppConfLangProvider Lang => AppConfLangProvider.Current;

        public static List<SettingItemDefinitionM> GetOverwriteSettings() =>
        [
            new(UICaptionProvider.AppConf.Groups.Overwrite,
                Lang["Setting.Overwrite.LongPressDivisor"],
                SettingControlType.TextBox,
                nameof(AppConfM.OverwriteSettings.LongPressMegabyteDivisor),
                MinValue: 1),
            new(UICaptionProvider.AppConf.Groups.Overwrite,
                Lang["Setting.Overwrite.MinLongPress"],
                SettingControlType.TextBox,
                nameof(AppConfM.OverwriteSettings.MinLongPressMs),
                MinValue: 0),
            new(UICaptionProvider.AppConf.Groups.Overwrite,
                Lang["Setting.Overwrite.MaxLongPress"],
                SettingControlType.TextBox,
                nameof(AppConfM.OverwriteSettings.MaxLongPressMs),
                MinValue: 0)
        ];

        public static List<SettingItemDefinitionM> GetLanguageSettings() =>
        [
            new(UICaptionProvider.AppConf.Groups.Language,
                Lang["Setting.Language.Select"],
                SettingControlType.Dropdown,
                nameof(AppConfM.Language.LanguageCode))
        ];

        public static List<SettingItemDefinitionM> GetInitModeSettings() =>
        [
            new(UICaptionProvider.AppConf.Groups.InitMode,
                Lang["Setting.InitMode.IsFirstLaunch"],
                SettingControlType.CheckBox,
                nameof(AppConfM.IsFirstLaunch))
        ];

        public static List<SettingItemDefinitionM> GetLogSettings() =>
        [
            new(UICaptionProvider.AppConf.Groups.Logs,
                Lang["Setting.Logs.SaveDefault"],
                SettingControlType.CheckBox,
                nameof(AppConfM.LogSettings.SaveLogsDefaultChecked)),
            new(UICaptionProvider.AppConf.Groups.Logs,
                Lang["Setting.Logs.MaxUpstream"],
                SettingControlType.TextBox,
                nameof(AppConfM.LogSettings.MaxUpstreamLogFiles),
                MinValue: 1),
            new(UICaptionProvider.AppConf.Groups.Logs,
                Lang["Setting.Logs.MaxDownstream"],
                SettingControlType.TextBox,
                nameof(AppConfM.LogSettings.MaxDownstreamLogFiles),
                MinValue: 1)
        ];


    }
}
