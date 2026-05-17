using OneColumnEncoder.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneColumnEncoder.Stores
{
    /// <summary>
    /// Store imported tools (their paths) and version strings
    /// </summary>
    public class AppDataS : SaveLoadBase<AppDataS>
    {
        // Tools file path
        private static readonly string ConfigFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appdata.json");
        protected override string FilePath => ConfigFilePath;

        // Paths and versions of imported tools
        public Importables Tools { get; set; } = new Importables();

        // Property change is within SaveLoadBaseS, so no need to implement it here

        #region ImportedTools data structure
        public class Importables
        {
            // Every tool has their way of printing versions, so just clip their version string
            public string? FfmpegPath { get; set; }
            public string? FfmpegVer { get; set; }
            public string? VspipePath { get; set; }
            public string? VspipeVer { get; set; }
            public string? Avs2yuvPath { get; set; }
            public string? Avs2yuvVer { get; set; }
            public string? Avs2pipemodPath { get; set; }
            public string? Avs2pipemodVer { get; set; }
            public string? OneLineShotArgsPath { get; set; }
            // Unused: public string? OneLineShotArgsVer { get; set; }
            public string? X264Path { get; set; }
            public string? X264Ver { get; set; }
            public string? X265Path { get; set; }
            public string? X265Ver { get; set; }
            public string? SvtAv1Path { get; set; }
            public string? SvtAv1Ver { get; set; }
            public string? FfprobePath { get; set; }
            public string? FfprobeVer { get; set; }
            public string? AviSynthDllPath { get; set; }
            // Unused: public string? AviSynthDllVer { get; set; }
        }
        #endregion
    }
}
