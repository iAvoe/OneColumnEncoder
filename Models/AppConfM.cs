using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel;
using OneColumnEncoder.Helpers;

namespace OneColumnEncoder.Models
{
    public class AppConfM : SaveLoadBaseH<AppConfM>
    {
        private static readonly string ConfigFilePath =
            Path.Combine(GetConfigDirectory(), "appconfig.json");
        protected override string FilePath => ConfigFilePath;
        public OverwriteSettings Overwrite { get; set; } = new OverwriteSettings();
        public Language Lang { get; set; } = new Language();

        #region Setting items
        public class OverwriteSettings
        {
            public int LongPressMegabyteDivisor { get; set; } = 40;
            public int MinLongPressMs { get; set; } = 1250;
            public int MaxLongPressMs { get; set; } = 12500;
        }
        public class Language
        {
            public string LanguageCode { get; set; } = "en";
        }
        #endregion
    }
}
