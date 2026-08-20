namespace OneColumnEncoder.Models;

/// <summary>
/// Builds configuration UI setting definitions.
/// </summary>
public static class SettinglistProviderM
{
    public static List<SettingItemDefinitionM> GetAllSettings() =>
    [
        .. GetOverwriteSettings(),
        .. GetLanguageSettings(),
        .. GetInitModeSettings(),
        .. GetFontSettings(),
        .. GetLogSettings(),
        .. GetAudioMuxSettings()
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
            nameof(AppConfM.Language.LanguageCode),
            Options: UICaptionProvider.AppConf.LanguageOptions.Codes,
            DisplayNameResolver: UICaptionProvider.AppConf.LanguageOptions.GetDisplayName)
    ];

    public static List<SettingItemDefinitionM> GetInitModeSettings() =>
    [
        new(UICaptionProvider.AppConf.Groups.InitMode,
            Lang["Setting.InitMode.IsFirstLaunch"],
            SettingControlType.CheckBox,
            nameof(AppConfM.IsFirstLaunch))
    ];

    public static List<SettingItemDefinitionM> GetFontSettings() =>
    [
        new(UICaptionProvider.AppConf.Groups.Fonts,
            Lang["Setting.Font.Ui"],
            SettingControlType.Font,
            nameof(AppConfM.FontSettings.UiFontFamily)),
        new(UICaptionProvider.AppConf.Groups.Fonts,
            Lang["Setting.Font.Code"],
            SettingControlType.Font,
            nameof(AppConfM.FontSettings.CodeFontFamily))
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

    public static List<SettingItemDefinitionM> GetAudioMuxSettings() =>
    [
        new(UICaptionProvider.AppConf.Groups.AudioMux,
            Lang["Setting.AudioMux.Single"],
            SettingControlType.Dropdown,
            nameof(AppConfM.AudioMuxSettings.SingleMode),
            Options: UICaptionProvider.AppConf.AudioMuxOptions.Codes,
            DisplayNameResolver: UICaptionProvider.AppConf.AudioMuxOptions.GetDisplayName),
        new(UICaptionProvider.AppConf.Groups.AudioMux,
            Lang["Setting.AudioMux.Queue"],
            SettingControlType.Dropdown,
            nameof(AppConfM.AudioMuxSettings.QueueMode),
            Options: UICaptionProvider.AppConf.AudioMuxOptions.Codes,
            DisplayNameResolver: UICaptionProvider.AppConf.AudioMuxOptions.GetDisplayName),
        new(UICaptionProvider.AppConf.Groups.AudioMux,
            Lang["Setting.AudioMux.Concat"],
            SettingControlType.Dropdown,
            nameof(AppConfM.AudioMuxSettings.ConcatMode),
            Options: UICaptionProvider.AppConf.AudioMuxOptions.Codes,
            DisplayNameResolver: UICaptionProvider.AppConf.AudioMuxOptions.GetDisplayName),
        new(UICaptionProvider.AppConf.Groups.AudioMux,
            Lang["Setting.AudioMux.Repart"],
            SettingControlType.Dropdown,
            nameof(AppConfM.AudioMuxSettings.RepartMode),
            Options: UICaptionProvider.AppConf.AudioMuxOptions.Codes,
            DisplayNameResolver: UICaptionProvider.AppConf.AudioMuxOptions.GetDisplayName)
    ];


}
