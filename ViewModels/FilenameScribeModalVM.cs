using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace OneColumnEncoder.ViewModels
{
    public partial class FilenameScribeModalVM : BaseVM
    {
        private const string PossibleExtensions = ".mp4|.hevc|.ivf";
        private static readonly Regex ReservedNameRegex = ReservedFilenames();

        private readonly Action _closeAction;
        private readonly AppConfM _appConfM;
        private readonly ToolItemVM _outputSettingItem;
        public CloseModalCmd CloseCmd { get; }
        public ButtonGroupVM FilenameButtons { get; private set; } = null!;
        public ObservableCollection<ChecklistEntryVM> FilenameChecklist { get; } = [];

        private string _videoFilename = string.Empty;
        public string VideoFilename
        {
            get => _videoFilename;
            set
            {
                if (SetProperty(ref _videoFilename, value)) ValidateFilename();
            }
        }

        public static string WindowTitle => UILangProviderM.Current["FilenameScribe.WindowTitle"];
        public static string PlaceholderText => UILangProviderM.Current["FilenameScribe.PlaceholderText"];
        public static string ExtensionText => PossibleExtensions;
        public static string AutoRuleDate => UILangProviderM.Current["FilenameScribe.AutoRuleDate"];
        public static string AutoRuleSeason => UILangProviderM.Current["FilenameScribe.AutoRuleSeason"];
        public static string AutoRuleVersion => UILangProviderM.Current["FilenameScribe.AutoRuleVersion"];
        public static string FooterHint => UILangProviderM.Current["FilenameScribe.FooterHint"];

        public FilenameScribeModalVM(Action closeAction, AppConfM appConfM, ToolItemVM outputSettingItem)
        {
            _closeAction = closeAction;
            _appConfM = appConfM;
            _outputSettingItem = outputSettingItem;
            _videoFilename = GetInitialFilename(outputSettingItem);
            CloseCmd = new CloseModalCmd(closeAction);
            BuildChecklist();
            BuildButtonGroup();
            ValidateFilename();
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        private void BuildChecklist()
        {
            FilenameChecklist.Clear();
            FilenameChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckLength"] });
            FilenameChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckReserved"],IsEnabled = _appConfM.General.OSFileNameInvalid });
            FilenameChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckInvalidChars"], IsEnabled = _appConfM.General.OSFileNameInvalid });
            FilenameChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckExtendedChars"], IsEnabled = _appConfM.General.FTPFileNameInvalid });
            FilenameChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckSpaces"], IsEnabled = _appConfM.General.FTPFileNameInvalid });
        }

        private void BuildButtonGroup()
        {
            FilenameButtons = ButtonGroupVM.CreateThreeButton(
                UILangProviderM.Current["FilenameScribe.PasteFromClipboard"],
                UILangProviderM.Current["FilenameScribe.Cancel"],
                UILangProviderM.Current["FilenameScribe.Confirm"],
                new ActionCmd(_ => PasteFromClipboard()),
                CloseCmd,
                new ActionCmd(_ => Confirm(), _ => FilenameButtons.B3_3IsEnabled));
        }

        private void PasteFromClipboard()
        {
            if (Clipboard.ContainsText()) VideoFilename = Clipboard.GetText().Trim();
        }

        private void Confirm()
        {
            if (!CanConfirm()) return;

            string filename = VideoFilename.Trim();
            if (string.IsNullOrWhiteSpace(filename)) return;

            OpenFolderDialog dialog = new()
            {
                Title = WindowTitle,
                InitialDirectory = GetInitialDirectory()
            };

            if (dialog.ShowDialog() != true) return;

            string ext = PossibleExtensions.Split('|')[0];
            _outputSettingItem.Path = Path.Combine(dialog.FolderName, filename + ext);
            _outputSettingItem.VersionText = filename;
            _closeAction();
        }

        private bool CanConfirm() => FilenameButtons.B3_3IsEnabled;

        private void ValidateFilename()
        {
            if (FilenameChecklist.Count < 5 || FilenameButtons is null) return;

            string filename = VideoFilename.Trim();
            SetChecklistStatus(0, filename.Length <= 50);
            SetChecklistStatus(1, !ReservedNameRegex.IsMatch(filename));
            SetChecklistStatus(2, !ContainsInvalidFileNameChar(filename));
            SetChecklistStatus(3, !filename.Any(char.IsSurrogate) && filename.All(c => c <= 0x7f));
            SetChecklistStatus(4, !filename.Contains(' '));

            FilenameButtons.B3_3IsEnabled = FilenameChecklist
                .Where(e => e.IsEnabled)
                .All(e => e.Status == StatusType.Success);
            (FilenameButtons.Cmd3 as BaseCmd)?.OnCanExecuteChanged();
        }

        private void SetChecklistStatus(int index, bool isValid)
        {
            FilenameChecklist[index].Status = FilenameChecklist[index].IsEnabled
                ? isValid ? StatusType.Success : StatusType.Error
                : StatusType.Waiting;
        }

        private static bool ContainsInvalidFileNameChar(string filename)
        {
            return filename.Length == 0
                || filename.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0
                || filename.Contains('&')
                || filename.EndsWith('.')
                || filename.EndsWith(' ');
        }

        private static string GetInitialFilename(ToolItemVM outputSettingItem)
        {
            if (!string.IsNullOrWhiteSpace(outputSettingItem.VersionText))
                return outputSettingItem.VersionText;

            if (!string.IsNullOrWhiteSpace(outputSettingItem.Path))
                return Path.GetFileNameWithoutExtension(outputSettingItem.Path);

            return string.Empty;
        }

        private string GetInitialDirectory()
        {
            if (string.IsNullOrWhiteSpace(_outputSettingItem.Path))
                return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            string? directory = Path.GetDirectoryName(_outputSettingItem.Path);
            return Directory.Exists(directory) ? directory : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }

        private void OnLanguageChanged()
        {
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(PlaceholderText));
            OnPropertyChanged(nameof(AutoRuleDate));
            OnPropertyChanged(nameof(AutoRuleSeason));
            OnPropertyChanged(nameof(AutoRuleVersion));
            OnPropertyChanged(nameof(FooterHint));

            BuildChecklist();
            BuildButtonGroup();
            OnPropertyChanged(nameof(FilenameButtons));
            ValidateFilename();
        }

        public override void Dispose()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            base.Dispose();
        }

        // Window reserved filenames
        [GeneratedRegex(@"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\..*)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex ReservedFilenames();
    }
}
