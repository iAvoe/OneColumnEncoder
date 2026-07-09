using OneColumnEncoder.Persistence;
using System.IO;

namespace OneColumnEncoder.Models
{
    public class AppConfM : SaveLoadBase<AppConfM>
    {
        private static readonly string ConfigFilePath =
            Path.Combine(GetConfigDirectory(), "appconfig.json");
        protected override string FilePath => ConfigFilePath;
        public bool IsFirstLaunch { get; set; } = true;
        public OverwriteSettings Overwrite { get; set; } = new OverwriteSettings();
        public Language Lang { get; set; } = new Language();
        public BypassSettings Bypass { get; set; } = new BypassSettings();

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
        public class BypassSettings
        {
            public bool BypassSrcValidationGroup { get; set; } = true;
            public bool BypassEncTermsValidationGroup { get; set; } = true;
        }
        #endregion
    }
}
