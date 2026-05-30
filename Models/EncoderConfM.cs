using System;
using System.IO;
using OneColumnEncoder.Helpers;

namespace OneColumnEncoder.Models
{
    public class EncoderConfM : SaveLoadBaseH<EncoderConfM>
    {
        private static readonly string ConfigFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "1cenc-encconf.json");

        protected override string FilePath => ConfigFilePath;

        public int EncoderModeTabIndex { get; set; } = 0;
        public string RateControlMode { get; set; } = "CRF";

        // x264
        public int X264Crf { get; set; } = 23;
        public int X264Abr { get; set; } = 209;
        public int X264Keyframe { get; set; } = 9;
        public string X264Mode { get; set; } = "a"; // See EncoderPresetsM
        public bool X264Mod { get; set; } = false;

        // x265
        public int X265Crf { get; set; } = 28;
        public int X265Abr { get; set; } = 70;
        public int X265Keyframe { get; set; } = 7;
        public string X265Mode { get; set; } = "a";
        public bool X265Aq { get; set; } = false;
        public bool X265Dark { get; set; } = false;
        public bool X265Texture { get; set; } = false;

        // SVT-AV1
        public int SvtAv1Crf { get; set; } = 35;
        public int SvtAv1Abr { get; set; } = 10;
        public int SvtAv1Keyframe { get; set; } = 9;
        public string SvtAv1Mode { get; set; } = "a";
        public bool SvtAv1Dl2 { get; set; } = false;
        public bool SvtAv1AutoTile { get; set; } = false;

        public bool UseLargePages { get; set; } = false;
        public string CustomParams { get; set; } = "";
    }
}
