using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel;

namespace OneColumnEncoder.Stores
{
    /// <summary>
    /// Settings for this app
    /// </summary>
    public class AppConfS : SaveLoadBaseS<AppConfS>
    {
        // Settings file path
        private static readonly string ConfigFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appconfig.json");
        protected override string FilePath => ConfigFilePath;

        // App behaviors, File overwrite behaviors, SMTP settings
        public GeneralSettings General { get; set; } = new GeneralSettings();
        public OverwriteSettings Overwrite { get; set; } = new OverwriteSettings();
        public SmtpSettings Smtp { get; set; } = new SmtpSettings();

        // Property change is within SaveLoadBaseS, so no need to implement it here

        #region Setting items
        public class GeneralSettings
        {
            public bool AllowCtrlClick { get; set; } = true;
            // Hardware prereq
            public bool OnExternalPower { get; set; } = true;
            public bool SufficientRAM { get; set; } = true;
            public bool SufficientDiskSpace { get; set; } = true;
            // Software prereq
            public bool OSFileNameValid { get; set; } = true;
            public bool FTPFileNameValid { get; set; } = true;
            public bool OutputFolderWritable { get; set; } = true;
            public bool NoOverwrite { get; set; } = true;

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
            public bool NotifyOnFailure { get; set; } = false;
            public bool NotifyOnNoInput { get; set; } = true;
            // Thresholds to avoid sending notifications
            public int NotifySuccessTaskThresholdMinutes { get; set; } = 9;
            public int NotifyFailureTaskThresholdMinutes { get; set; } = 2;
            public int NotifyNoInputTaskThresholdMinutes { get; set; } = 2;
        }
        #endregion
    }
}
