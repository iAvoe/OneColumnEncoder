using OneColumnEncoder.Models;
using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Stores;
using System.IO;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels.Cards
{
    public class ToolItemCardVM(EncItemM baseModel) : BaseVM
    {
        public static string SeparatorText =>
            UILangProvider.Current["ItemCard.Separator"];

        private readonly EncItemM _baseModel = baseModel;

        #region Properties
        public string Name
        {
            get => _baseModel.Name;
            set
            {
                if (_baseModel.Name != value)
                {
                    _baseModel.Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public string P2TextData
        {
            get => _baseModel.Path;
            set
            {
                if (_baseModel.Path != value)
                {
                    _baseModel.Path = value;
                    OnPropertyChanged(nameof(P2TextData));
                    OnPropertyChanged(nameof(P2Text));
                    OnPropertyChanged(nameof(DisplayR1Text));
                    Validate(); // Changes both versionText and isReal
                }
            }
        }

        private string _p1TextData = string.Empty; // Non-nullable
        public string P1TextData
        {
            get => _p1TextData;
            set
            {
                SetProperty(ref _p1TextData, value);
                OnPropertyChanged(nameof(P1Text));
                OnPropertyChanged(nameof(P1TooltipText)); // Fallback value changed
            }
        }

        // Longer text shown in tooltip for queue items (up to ~512 chars of file names).
        // Falls back to P1TextData when not explicitly set, so non-queue items are unaffected.
        private string? _p1TooltipText;
        public string? P1TooltipText
        {
            get => _p1TooltipText ?? P1TextData;
            set
            {
                if (SetProperty(ref _p1TooltipText, value))
                    OnPropertyChanged(nameof(P1TooltipText));
            }
        }

        private bool _isReal = true;
        public bool IsReal
        {
            get => _isReal;
            set => SetProperty(ref _isReal, value);
        }

        private string _p1Name = "";
        public string P1Name
        {
            get => _p1Name;
            set => SetProperty(ref _p1Name, value);
        }

        public string P1Text => P1TextData;

        private string _p2Name = "";
        public string P2Name
        {
            get => _p2Name;
            set => SetProperty(ref _p2Name, value);
        }
        public string P2Text => P2TextData;

        private string _r1Text = "";
        public string R1Text // Maybe 'Edit' in some cases
        {
            get => _r1Text;
            set
            {
                if (!SetProperty(ref _r1Text, value)) return;
                OnPropertyChanged(nameof(DisplayR1Text));
            }
        }

        public string DisplayR1Text =>
            UseAutoAddReplaceText
                ? string.IsNullOrWhiteSpace(P2TextData)
                    ? UILangProvider.Current["Buttons.Add"]
                    : UILangProvider.Current["Buttons.Replace"]
                : R1Text;

        private string _r2Text = "";
        public string R2Text
        {
            get => _r2Text;
            set => SetProperty(ref _r2Text, value);
        }

        private bool _useAutoAddReplaceText;
        public bool UseAutoAddReplaceText
        {
            get => _useAutoAddReplaceText;
            set
            {
                if (!SetProperty(ref _useAutoAddReplaceText, value)) return;
                OnPropertyChanged(nameof(DisplayR1Text));
            }
        }

        private bool _enableRealCheck = true;
        public bool EnableRealCheck
        {
            get => _enableRealCheck;
            set
            {
                if (!SetProperty(ref _enableRealCheck, value)) return;
                if (!_enableRealCheck) IsReal = true;
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    // Notify to unselect other ItemCards
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private bool _isCancel;
        public bool IsCancel
        {
            get => _isCancel;
            set => SetProperty(ref _isCancel, value);
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private ICommand? _r1Command;
        public ICommand? R1Command
        {
            get => _r1Command;
            set => SetProperty(ref _r1Command, value);
        }

        private ICommand? _r2Command;
        public ICommand? R2Command
        {
            get => _r2Command;
            set => SetProperty(ref _r2Command, value);
        }

        public static bool R1IsEnabled => true;
        public static bool R2IsEnabled => true;
        public static string R3Text => string.Empty;
        public static ICommand? R3Command => null;
        public static bool R3IsEnabled => true;
        public static bool IsRecentlyMoved => false;

        #endregion

        #region Methods

        private long? _expectedFileSize;

        public void SetStoredFingerprint(long? fileSize)
        {
            _expectedFileSize = fileSize;
            Validate();
        }

        private void Validate()
        {
            if (!EnableRealCheck)
            {
                IsReal = true;
                return;
            }

            bool exists = File.Exists(P2TextData);
            bool isKnownBinary =
                P2TextData.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                P2TextData.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);

            bool sizeMatches = !_expectedFileSize.HasValue ||
                (exists && new FileInfo(P2TextData).Length == _expectedFileSize.Value);

            IsReal = exists && isKnownBinary && sizeMatches;
            if (!IsReal) P1TextData = string.Empty;
        }

        public void ApplyDefinition(ToolDefinitionM definition)
        {
            Name = definition.DisplayName;
            R1Text = definition.R1Text;
            R2Text = definition.R2Text;
            P1Name = definition.P1Name;
            P2Name = definition.P2Name ?? string.Empty;
        }

        public void RefreshLanguage()
        {
            OnPropertyChanged(nameof(SeparatorText));
            OnPropertyChanged(nameof(DisplayR1Text));
        }

        // Write back to MainVM EncodingConfZone[3] (Encoder Settings)
        public void SetEncodingSummary(string line1, string line2)
        {
            P2TextData = line2;
            P1TextData = line1;
        }

        private const string DefaultOutputSettingText = "1cenc output";

        public void InitializeOutputSetting(string outputDirectory)
        {
            if (string.IsNullOrWhiteSpace(P2TextData))
                P2TextData = outputDirectory;

            if (string.IsNullOrWhiteSpace(P1TextData))
                P1TextData = DefaultOutputSettingText;
        }

        public void RefreshOutputSetting(bool queueRouteActive, ModalNavS modalNavS, string? sourcePath = null)
        {
            R1Command = queueRouteActive
                ? new BrowseOutputDirectoryCmd(this)
                : new OpenFilenameScribeCmd(modalNavS, this);

            if (queueRouteActive)
            {
                P1TextData = "N/A";
                P1TooltipText = null;
                return;
            }

            if (!string.IsNullOrWhiteSpace(sourcePath))
                P1TextData = Path.GetFileNameWithoutExtension(sourcePath);
        }
        #endregion
    }
}
