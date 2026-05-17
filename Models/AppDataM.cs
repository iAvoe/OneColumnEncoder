using OneColumnEncoder.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneColumnEncoder.Models
{
    public class AppDataM : SaveLoadBase<AppDataM>
    {
        private static readonly string ConfigFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appdata.json");
        protected override string FilePath => ConfigFilePath;

        public Importables Tools { get; set; } = new Importables();

        #region ImportedTools data structure
        public class Importables
        {
            public string? FfmpegPath { get; set; }
            public string? FfmpegVer { get; set; }
            public string? VspipePath { get; set; }
            public string? VspipeVer { get; set; }
            public string? Avs2yuvPath { get; set; }
            public string? Avs2yuvVer { get; set; }
            public string? Avs2pipemodPath { get; set; }
            public string? Avs2pipemodVer { get; set; }
            public string? OneLineShotArgsPath { get; set; }
            public string? X264Path { get; set; }
            public string? X264Ver { get; set; }
            public string? X265Path { get; set; }
            public string? X265Ver { get; set; }
            public string? SvtAv1Path { get; set; }
            public string? SvtAv1Ver { get; set; }
            public string? FfprobePath { get; set; }
            public string? FfprobeVer { get; set; }
            public string? AviSynthDllPath { get; set; }
        }
        #endregion
    }
}
