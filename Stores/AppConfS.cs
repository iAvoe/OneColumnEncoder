using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel;
using OneColumnEncoder.Helpers;

namespace OneColumnEncoder.Stores
{
    /// <summary>
    /// Settings for this app
    /// </summary>
    public class AppConfS : SaveLoadBase<AppConfS>
    {
        // Settings file path
        private static readonly string ConfigFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appconfig.json");
        protected override string FilePath => ConfigFilePath;

        // App behaviors, File overwrite behaviors, SMTP settings, langyage
        public GeneralSettings General { get; set; } = new GeneralSettings();
        public OverwriteSettings Overwrite { get; set; } = new OverwriteSettings();
        public SmtpSettings Smtp { get; set; } = new SmtpSettings();
        public Language Lang { get; set; } = new Language();

        // Property change is within SaveLoadBaseS, so no need to implement it here

        #region Setting items
        public class GeneralSettings
        {
            // public bool AllowCtrlClick { get; set; } = true;
            // Hardware prereq
            public bool OffGrid { get; set; } = true;
            public bool InsufficientRAM { get; set; } = false;
            public bool InsufficientDiskSpace { get; set; } = true;
            // Software prereq
            public bool OSFileNameInvalid { get; set; } = true;
            public bool FTPFileNameInvalid { get; set; } = false;
            public bool NoWritePermission { get; set; } = true;
            public bool IsOverwriting { get; set; } = true;

            // Not planned yet
            // public bool AutoCheckForUpdates { get; set; } = true;
        }
        public class OverwriteSettings
        {
            public int LongPressMegabyteDivisor { get; set; } = 40;
            public int MinLongPressMs { get; set; } = 1250;
            public int MaxLongPressMs { get; set; } = 125000;
        }
        public class SmtpSettings
        {
            public string ServerUrl { get; set; } = "";
            public int Port { get; set; } = 587;
            public bool UseSSL { get; set; } = true;
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
            public string FromEmail { get; set; } = "";
            public string ToEmail { get; set; } = "";
            // When to send email notifications (if SMTP is configured)
            public bool NotifyOnSuccess { get; set; } = true;
            public bool NotifyOnFailure { get; set; } = true;
            public bool NotifyOnNoInput { get; set; } = true;
            // Thresholds to avoid sending notifications
            public int NotifySuccessThresholdMin { get; set; } = 9;
            public int NotifyFailureThresholdMin { get; set; } = 2;
            public int NotifyAfterNoInputThresholdMin { get; set; } = 2;
        }
        public class Language
        {
            public string LanguageCode { get; set; } = "en";
        }
        #endregion
    }
}
