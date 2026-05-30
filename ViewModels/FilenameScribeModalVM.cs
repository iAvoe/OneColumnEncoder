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
using OneColumnEncoder.ViewModels.Cards;

namespace OneColumnEncoder.ViewModels
{
    public partial class FilenameScribeModalVM : BaseVM
    {
        private const string PossibleExtensions = ".mp4|.hevc|.ivf";

        private readonly Action _closeAction;
        private readonly ToolItemCardVM _outputSettingItem;
        public CloseModalCmd CloseCmd { get; }
        public ButtonGroupVM FilenameButtons { get; private set; } = null!;
        public ObservableCollection<ChecklistEntryVM> FilenameChecklist { get; } = [];

        private string _videoFilename = string.Empty;
        public string VideoFilename
        {
            get => _videoFilename;
            set
            {
                if (!SetProperty(ref _videoFilename, value)) return;
                ValidateFilename();
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

        public FilenameScribeModalVM(Action closeAction, ToolItemCardVM outputSettingItem)
        {
            _closeAction = closeAction;
            _outputSettingItem = outputSettingItem;
            _videoFilename = OutputPathH.GetInitialFilename(outputSettingItem.P1TextData, outputSettingItem.P2TextData);
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
                Text = UILangProviderM.Current["FilenameScribe.CheckCombiningMarks"] });
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

        // Filename is good, proceed to select path
        private void Confirm()
        {
            if (!CanConfirm()) return;

            string filename = VideoFilename.Trim();
            if (string.IsNullOrWhiteSpace(filename)) return;

            OpenFolderDialog dialog = new()
            {
                Title = WindowTitle,
                InitialDirectory = OutputPathH.GetInitialDirectory(_outputSettingItem.P2TextData)
            };

            if (dialog.ShowDialog() != true) return;

            _outputSettingItem.P2TextData = Path.Combine(dialog.FolderName, filename);
            _outputSettingItem.P1TextData = filename;
            _closeAction();
        }

        private bool CanConfirm() => FilenameButtons.B3_3IsEnabled;

        private void ValidateFilename()
        {
            if (FilenameChecklist.Count < 6 || FilenameButtons is null) return;

            string filename = VideoFilename.Trim();
            SetChecklistStatus(0, FilenameValidationH.IsValidLength(filename));
            SetChecklistStatus(1, FilenameValidationH.IsNotReservedName(filename));
            SetChecklistStatus(2, FilenameValidationH.HasNoInvalidChars(filename));
            // SetChecklistStatus(3, FilenameValidationH.HasNoExtendedChars(filename));
            FilenameChecklist[3].Status = FilenameValidationH.HasNoExtendedChars(filename)
                ? StatusType.Success
                : StatusType.Warning;
            FilenameChecklist[4].Status = FilenameValidationH.HasSpaces(VideoFilename)
                ? StatusType.Warning
                : StatusType.Success;

            SetChecklistStatus(5, FilenameValidationH.HasUnicodeCombiningMarks(filename));

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
