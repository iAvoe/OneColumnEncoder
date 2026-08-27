namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Localized strings for the external audio/subtitle track editor.
/// </summary>
public sealed class MuxLangProvider(string languageCode) : LangProviderBase(languageCode, Data)
{
    public const string WindowTitle = "Add Audio/Subtitles";

    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["MuxTracks.QueueSources"] = "Queue Sources",
            ["MuxTracks.AudioHeader"] = "Audio Track Adding",
            ["MuxTracks.SubtitleHeader"] = "Subtitle Track Adding",
            ["MuxTracks.AddAudio"] = "Add Audio",
            ["MuxTracks.AddSubtitle"] = "Add Subtitle",
            ["MuxTracks.Sync"] = "Sync (\u00B1ms)",
            ["MuxTracks.Primary"] = "Primary track",
            ["MuxTracks.Empty"] = "No tracks added",
            ["MuxTracks.DurationUnknown"] = "N/A",
            ["MuxTracks.Moved"] = "moved",
            ["MuxTracks.Cancel"] = "Cancel",
            ["MuxTracks.Confirm"] = "Confirm",
            ["MuxTracks.InvalidSync"] = "Sync must be a whole number of milliseconds.",
            ["MuxTracks.FileFilter"] = "Media files|*.aac;*.ac3;*.eac3;*.flac;*.m4a;*.mka;*.mp3;*.ogg;*.opus;*.wav;*.ass;*.srt;*.ssa;*.sub;*.sup|All files (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Add Audio/Subtitles",
        },
    };

    static MuxLangProvider()
    {
        foreach (string code in new[] { "zh-cn", "zh-tw", "fr", "es", "ja", "ru", "de", "ko", "pt-br" })
            Data[code] = new(Data["en"]);

        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "队列源",
            ["MuxTracks.AudioHeader"] = "添加音轨",
            ["MuxTracks.SubtitleHeader"] = "添加字幕轨",
            ["MuxTracks.AddAudio"] = "添加音频",
            ["MuxTracks.AddSubtitle"] = "添加字幕",
            ["MuxTracks.Sync"] = "同步（+-毫秒）",
            ["MuxTracks.Primary"] = "主轨",
            ["MuxTracks.Cancel"] = "取消",
            ["MuxTracks.Confirm"] = "确认",
            ["MuxTracks.InvalidSync"] = "同步偏移值必须是整数毫秒。",
            ["MuxTracks.FileFilter"] = "媒体文件|*.aac;*.ac3;*.eac3;*.flac;*.m4a;*.mka;*.mp3;*.ogg;*.opus;*.wav;*.ass;*.srt;*.ssa;*.sub;*.sup|All files (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "添加音频/字幕轨道",
        }) Data["zh-cn"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "佇列來源",
            ["MuxTracks.AudioHeader"] = "新增音軌",
            ["MuxTracks.SubtitleHeader"] = "新增字幕軌",
            ["MuxTracks.AddAudio"] = "新增音訊",
            ["MuxTracks.AddSubtitle"] = "新增字幕",
            ["MuxTracks.Sync"] = "同步（+-毫秒）",
            ["MuxTracks.Primary"] = "主要軌",
            ["MuxTracks.Cancel"] = "取消",
            ["MuxTracks.Confirm"] = "確認",
            ["MuxTracks.InvalidSync"] = "同步偏移值必須是整數毫秒。",
            ["MuxTracks.FileFilter"] = "媒體檔案|*.aac;*.ac3;*.eac3;*.flac;*.m4a;*.mka;*.mp3;*.ogg;*.opus;*.wav;*.ass;*.srt;*.ssa;*.sub;*.sup|All files (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "添加音訊/字幕軌道",
        }) Data["zh-tw"][pair.Key] = pair.Value;
    }

    public static MuxLangProvider Current => new(UILangProvider.Current.LanguageCode);
}
