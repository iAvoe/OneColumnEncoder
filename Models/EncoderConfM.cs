using System;
using System.IO;
using OneColumnEncoder.Helpers;

namespace OneColumnEncoder.Models
{
    public class EncoderConfM : SaveLoadBaseH<EncoderConfM>
    {
        private static readonly string ConfigFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "encoderconfig.json");

        protected override string FilePath => ConfigFilePath;

        public string RateControlMode { get; set; } = "CRF";
        public int CrfValue { get; set; } = 23;
        public int TargetBitrate { get; set; } = 2000;
        public string Preset { get; set; } = "medium";
        public string Tune { get; set; } = "none";
        public string Profile { get; set; } = "auto";
        public int KeyframeInterval { get; set; } = 250;
        public bool FastDecode { get; set; } = false;
        public bool ZeroLatency { get; set; } = false;
        public string CustomParams { get; set; } = "";
    }
}
