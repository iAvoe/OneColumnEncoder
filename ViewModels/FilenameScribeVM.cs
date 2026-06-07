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
    public partial class FilenameScribeVM : BaseVM
    {
        private const string PossibleExtensions = ".mp4|.hevc|.ivf";
        private static readonly double[] RotatingFontSizes = [10, 13, 14, 16];

        private readonly Action _closeAction;
        private readonly ToolItemCardVM _outputSettingItem;
        public CloseModalCmd CloseCmd { get; }
        public ActionCmd RotateFontSizeCmd { get; }
        public ButtonGroupVM FilenameActionButtons { get; private set; } = null!;
        public ButtonGroupVM FilenameFinishButtons { get; private set; } = null!;
        public ObservableCollection<ChecklistEntryVM> SevereIssueChecklist { get; } = [];
        public ObservableCollection<ChecklistEntryVM> GeneralIssueChecklist { get; } = [];

        private string _videoFilename = string.Empty;
        private double _videoFilenameFontSize = 14;
        public string VideoFilename
        {
            get => _videoFilename;
            set
            {
                if (!SetProperty(ref _videoFilename, value)) return;
                ValidateFilename();
            }
        }

        public double VideoFilenameFontSize
        {
            get => _videoFilenameFontSize;
            private set => SetProperty(ref _videoFilenameFontSize, value);
        }

        public static string WindowTitle => UILangProviderM.Current["FilenameScribe.WindowTitle"];
        public static string MiniHeader => UILangProviderM.Current["FilenameScribe.MiniHeader"];
        public static string PlaceholderText => UILangProviderM.Current["FilenameScribe.Placeholder"];
        public static string ExtensionText => PossibleExtensions;
        public static string PreviewHeader => UILangProviderM.Current["FilenameScribe.PreviewHeader"];
        public static string Preview30Label => UILangProviderM.Current["FilenameScribe.Preview30Label"];
        public static string Preview25Label => UILangProviderM.Current["FilenameScribe.Preview25Label"];
        public static string Preview20Label => UILangProviderM.Current["FilenameScribe.Preview20Label"];
        public static string Preview15Label => UILangProviderM.Current["FilenameScribe.Preview15Label"];
        public static string FormatCheckHeader => UILangProviderM.Current["FilenameScribe.FormatCheckHeader"];
        public static string SevereIssueHeader => UILangProviderM.Current["FilenameScribe.SevereIssueHeader"];
        public static string GeneralIssueHeader => UILangProviderM.Current["FilenameScribe.GeneralIssueHeader"];
        public static string SelfCheckHeader => UILangProviderM.Current["FilenameScribe.SelfCheckHeader"];
        public static string SelfCheckDate => UILangProviderM.Current["FilenameScribe.SelfCheck1"];
        public static string SelfCheckSeason => UILangProviderM.Current["FilenameScribe.SelfCheck2"];
        public static string SelfCheckVersion => UILangProviderM.Current["FilenameScribe.SelfCheck3"];
        public static string FooterHint => UILangProviderM.Current["FilenameScribe.FooterHint"];

        public FilenameScribeVM(Action closeAction, ToolItemCardVM outputSettingItem)
        {
            _closeAction = closeAction;
            _outputSettingItem = outputSettingItem;
            _videoFilename = OutputPathH.GetInitialFilename(outputSettingItem.P1TextData, outputSettingItem.P2TextData);
            CloseCmd = new CloseModalCmd(closeAction);
            RotateFontSizeCmd = new ActionCmd(_ => RotateFontSize());
            BuildChecklist();
            BuildButtonGroup();
            ValidateFilename();
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        private void BuildChecklist()
        {
            SevereIssueChecklist.Clear();
            SevereIssueChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckEmpty"] });
            SevereIssueChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckInvalidChars"] });
            SevereIssueChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckCombiningMarks"] });
            SevereIssueChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckSpecialSpaceVariants"] });
            SevereIssueChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckReserved"] });

            GeneralIssueChecklist.Clear();
            GeneralIssueChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckLength"] });
            GeneralIssueChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckSpaces"] });
            GeneralIssueChecklist.Add(new ChecklistEntryVM {
                Text = UILangProviderM.Current["FilenameScribe.CheckExtendedChars"] });
        }

        private void BuildButtonGroup()
        {
            FilenameActionButtons = ButtonGroupVM.CreateTwoButton(
                UILangProviderM.Current["FilenameScribe.PasteFromClipboard"],
                UILangProviderM.Current["FilenameScribe.RotateFontSize"],
                new ActionCmd(_ => PasteFromClipboard()),
                RotateFontSizeCmd);
            FilenameActionButtons.B2_1Icon = SvgIconProviderH.GamePaste;

            FilenameFinishButtons = ButtonGroupVM.CreateTwoButton(
                UILangProviderM.Current["FilenameScribe.Cancel"],
                UILangProviderM.Current["FilenameScribe.Confirm"],
                CloseCmd,
                new ActionCmd(_ => Confirm(), _ => FilenameFinishButtons.B2_2IsEnabled));
        }

        private void PasteFromClipboard()
        {
            if (Clipboard.ContainsText()) VideoFilename = Clipboard.GetText().Trim();
        }

        private void RotateFontSize()
        {
            int index = Array.IndexOf(RotatingFontSizes, VideoFilenameFontSize);
            VideoFilenameFontSize = RotatingFontSizes[(index + 1) % RotatingFontSizes.Length];
        }

        // Filename is good, proceed to select path & write back to MainUI outputSetting ItemCard
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

            Window? owner = Application.Current.MainWindow;
            bool? result = owner is null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
            if (result != true) return;

            _outputSettingItem.P2TextData = dialog.FolderName;
            _outputSettingItem.P1TextData = filename;
            _closeAction();
            Application.Current.MainWindow?.Activate();
        }

        private bool CanConfirm() => FilenameFinishButtons.B2_2IsEnabled;

        private void ValidateFilename()
        {
            if (SevereIssueChecklist.Count < 5 || GeneralIssueChecklist.Count < 3 || FilenameFinishButtons is null) return;

            string filename = VideoFilename.Trim();
            SetChecklistStatus(SevereIssueChecklist, 0, !string.IsNullOrWhiteSpace(VideoFilename));
            SetChecklistStatus(SevereIssueChecklist, 1, FilenameValidationH.HasNoInvalidChars(filename));
            SetChecklistStatus(SevereIssueChecklist, 2, FilenameValidationH.HasUnicodeCombiningMarks(filename));
            SetChecklistStatus(SevereIssueChecklist, 3, FilenameValidationH.HasNoSpecialSpaceVariants(VideoFilename));
            SetChecklistStatus(SevereIssueChecklist, 4, FilenameValidationH.IsNotReservedName(filename));
            SetChecklistStatus(GeneralIssueChecklist, 0, FilenameValidationH.IsValidLength(filename), useWarning: true);
            GeneralIssueChecklist[1].Status = FilenameValidationH.HasSpaces(VideoFilename)
                ? StatusType.Warning
                : StatusType.Success;
            GeneralIssueChecklist[2].Status = FilenameValidationH.HasNoExtendedChars(filename)
                ? StatusType.Success
                : StatusType.Warning;

            FilenameFinishButtons.B2_2IsEnabled = SevereIssueChecklist
                .Where(e => e.IsEnabled)
                .All(e => e.Status == StatusType.Success);
            (FilenameFinishButtons.Cmd2 as BaseCmd)?.OnCanExecuteChanged();
        }

        private static void SetChecklistStatus(ObservableCollection<ChecklistEntryVM> checklist, int index, bool isValid, bool useWarning = false)
        {
            checklist[index].Status = checklist[index].IsEnabled
                ? isValid ? StatusType.Success : (useWarning ? StatusType.Warning : StatusType.Error)
                : StatusType.Waiting;
        }

        private void OnLanguageChanged()
        {
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(PlaceholderText));
            OnPropertyChanged(nameof(PreviewHeader));
            OnPropertyChanged(nameof(Preview30Label));
            OnPropertyChanged(nameof(Preview25Label));
            OnPropertyChanged(nameof(Preview20Label));
            OnPropertyChanged(nameof(Preview15Label));
            OnPropertyChanged(nameof(FormatCheckHeader));
            OnPropertyChanged(nameof(SevereIssueHeader));
            OnPropertyChanged(nameof(GeneralIssueHeader));
            OnPropertyChanged(nameof(SelfCheckHeader));
            OnPropertyChanged(nameof(SelfCheckDate));
            OnPropertyChanged(nameof(SelfCheckSeason));
            OnPropertyChanged(nameof(SelfCheckVersion));
            OnPropertyChanged(nameof(FooterHint));

            BuildChecklist();
            BuildButtonGroup();
            OnPropertyChanged(nameof(FilenameActionButtons));
            OnPropertyChanged(nameof(FilenameFinishButtons));
            ValidateFilename();
        }

        public override void Dispose()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            base.Dispose();
        }

    }
}
