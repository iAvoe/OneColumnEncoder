using System.IO;

namespace OneColumnEncoder.Models;

/// <summary>
/// Application configuration persisted across launches.
/// </summary>
public class AppConfM : SaveLoadBase<AppConfM>
{
    private static readonly string ConfigFilePath =
        Path.Combine(GetConfigDirectory(), "appconfig.json");
    protected override string FilePath => ConfigFilePath;
    public bool IsFirstLaunch { get; set; } = true;
    public OverwriteSettings Overwrite { get; set; } = new OverwriteSettings();
    public Language Lang { get; set; } = new Language();
    public FontSettings Font { get; set; } = new FontSettings();
    public LogSettings Logs { get; set; } = new LogSettings();
    public AudioMuxSettings AudioMux { get; set; } = new AudioMuxSettings();
    public AutoMuxSettings AutoMux { get; set; } = new AutoMuxSettings();
    #region Setting items
    public class OverwriteSettings
    {
        public int CooldownMegabyteDivisor { get; set; } = 40;
        public int MinCooldownMs { get; set; } = 1250;
        public int MaxCooldownMs { get; set; } = 12500;
    }
    public class Language
    {
        public string LanguageCode { get; set; } = "en";
    }
    public class FontSettings
    {
        public string UiFontFamily { get; set; } = string.Empty;
        public string CodeFontFamily { get; set; } = string.Empty;
    }
    public class LogSettings
    {
        public bool SaveLogsDefaultChecked { get; set; } = true;
        public int MaxUpstreamLogFiles { get; set; } = 30;
        public int MaxDownstreamLogFiles { get; set; } = 30;
    }
    public class AudioMuxSettings
    {
        public string SingleMode { get; set; } = "Copy";
        public string QueueMode { get; set; } = "Copy";
        public string ConcatMode { get; set; } = "ReEncodeAAC320";
        public string RepartMode { get; set; } = "ReEncodeAAC320";
    }

    /// <summary>
    /// Whether muxing after encoding is enabled automatically, per encoding route
    /// (Single / Queue / Concat / Repart) and per encoder (x264 / x265 / SVT-AV1).
    /// Defaults enable muxing everywhere.
    /// </summary>
    public class AutoMuxSettings
    {
        public bool SingleX264 { get; set; } = true;
        public bool SingleX265 { get; set; } = true;
        public bool SingleSvtAv1 { get; set; } = true;
        public bool QueueX264 { get; set; } = true;
        public bool QueueX265 { get; set; } = true;
        public bool QueueSvtAv1 { get; set; } = true;
        public bool ConcatX264 { get; set; } = true;
        public bool ConcatX265 { get; set; } = true;
        public bool ConcatSvtAv1 { get; set; } = true;
        public bool RepartX264 { get; set; } = true;
        public bool RepartX265 { get; set; } = true;
        public bool RepartSvtAv1 { get; set; } = true;
    }
    #endregion
}
