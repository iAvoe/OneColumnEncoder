using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using OneColumnEncoder.Helpers;

namespace OneColumnEncoder.ViewModels
{
    public partial class FilenameScribeModalVM : BaseVM
    {
        private const string PossibleExtensions = ".mp4|.hevc|.ivf";

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
        public static string MiniHeader => UILangProviderM.Current["FilenameScribe.MiniHeader"];
        public static string PlaceholderText => UILangProviderM.Current["FilenameScribe.Placeholder"];
        public static string ExtensionText => PossibleExtensions;
        public static string SelfCheckDate => UILangProviderM.Current["FilenameScribe.SelfCheckDate"];
        public static string SelfCheckSeason => UILangProviderM.Current["FilenameScribe.SelfCheckSeason"];
        public static string SelfCheckVersion => UILangProviderM.Current["FilenameScribe.SelfCheckVersion"];
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
                Text = UILangProviderM.Current["FilenameScribe.CheckReserved"] });
            FilenameChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckInvalidChars"] });
            FilenameChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckExtendedChars"] });
            FilenameChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckSpaces"] });
            FilenameChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckFtpSafe"] });
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
            if (FilenameChecklist.Count < 6 || FilenameButtons is null) return;

            string filename = VideoFilename.Trim();
            SetChecklistStatus(0, ValidationH.IsValidLength(filename));
            SetChecklistStatus(1, ValidationH.IsNotReservedName(filename));
            SetChecklistStatus(2, ValidationH.HasNoInvalidChars(filename));
            SetChecklistStatus(3, ValidationH.HasNoExtendedChars(filename));

            FilenameChecklist[4].Status = ValidationH.HasSpaces(VideoFilename)
                ? StatusType.Warning
                : StatusType.Success;

            SetChecklistStatus(5, ValidationH.IsModernFtpSafe(filename));

            FilenameButtons.B3_3IsEnabled = FilenameChecklist
                .Where((e, i) => e.IsEnabled && i != 4)
                .All(e => e.Status == StatusType.Success);
            (FilenameButtons.Cmd3 as BaseCmd)?.OnCanExecuteChanged();
        }

        private void SetChecklistStatus(int index, bool isValid)
        {
            FilenameChecklist[index].Status = FilenameChecklist[index].IsEnabled
                ? isValid ? StatusType.Success : StatusType.Error
                : StatusType.Waiting;
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
            OnPropertyChanged(nameof(SelfCheckDate));
            OnPropertyChanged(nameof(SelfCheckSeason));
            OnPropertyChanged(nameof(SelfCheckVersion));
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

    }
}
