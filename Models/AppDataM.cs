using OneColumnEncoder.Persistence;
using System.IO;
using System.Text.Json.Serialization;

namespace OneColumnEncoder.Models
{
    public class AppDataM : SaveLoadBase<AppDataM>
    {
        private static readonly string ConfigFilePath =
            Path.Combine(GetConfigDirectory(), "appdata.json");
        protected override string FilePath => ConfigFilePath;

        public Importables Tools { get; set; } = new Importables();
        public EncodingSettings Encoding { get; set; } = new EncodingSettings();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsMiniUpstreamsZone { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsMiniEncodersZone { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsMiniAnalyticsZone { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsMiniDependenciesZone { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsMiniVideoSrcImportZone { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsMiniScriptSrcImportZone { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsMiniEncodingConfZone { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsMiniSrcValidationCard { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsMiniEncTermsCard { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsMiniBestPracticesCard { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsMiniToolsImportCard { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsMiniStartEncodingZone { get; set; }

        // File sizes are used for detecting tool replacements
        #region ImportedTools data structure
        public class Importables
        {
            public string? FfmpegPath { get; set; }
            public string? FfmpegVer { get; set; }
            public long? FfmpegSize { get; set; }
            public string? VspipePath { get; set; }
            public string? VspipeVer { get; set; }
            public long? VspipeSize { get; set; }
            public string? VspipeY4mArg { get; set; }
            public string? Avs2yuvPath { get; set; }
            public string? Avs2yuvVer { get; set; }
            public long? Avs2yuvSize { get; set; }
            public string? Avs2pipemodPath { get; set; }
            public string? Avs2pipemodVer { get; set; }
            public long? Avs2pipemodSize { get; set; }
            public string? OneLineShotArgsPath { get; set; }
            public string? OneLineShotArgsVer { get; set; }
            public long? OneLineShotArgsSize { get; set; }
            public string? X264Path { get; set; }
            public string? X264Ver { get; set; }
            public long? X264Size { get; set; }
            public string? X265Path { get; set; }
            public string? X265Ver { get; set; }
            public long? X265Size { get; set; }
            public string? SvtAv1Path { get; set; }
            public string? SvtAv1Ver { get; set; }
            public long? SvtAv1Size { get; set; }
            public string? FfprobePath { get; set; }
            public string? FfprobeVer { get; set; }
            public long? FfprobeSize { get; set; }
            public string? AviSynthDllPath { get; set; }
            public string? AviSynthDllVer { get; set; }
            public long? AviSynthDllSize { get; set; }
            // VideoSrcImportZone, ScriptSrcImportZone
            public string? VideoSourcePath { get; set; }
            public string? AvsSourcePath { get; set; }
            public string? VpySourcePath { get; set; }
            public string? SvfiSourcePath { get; set; }
        }

        public class EncodingSettings
        {
            public string OutputDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }
        #endregion
    }
}
