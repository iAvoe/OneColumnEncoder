using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace OneColumnEncoder.ViewModels
{
    public class FilenameScribeModalVM : BaseVM
    {
        private const string VideoExtension = ".hevc";
        private static readonly Regex ReservedNameRegex = new(@"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\..*)?$", RegexOptions.IgnoreCase);

        private readonly Action _closeAction;
        public CloseModalCmd CloseCmd { get; }
        public ButtonGroupVM FilenameButtons { get; private set; } = null!;
        public ObservableCollection<ChecklistEntryVM> FilenameChecklist { get; } = [];

        private string _videoFilename = string.Empty;
        public string VideoFilename
        {
            get => _videoFilename;
            set
            {
                if (SetProperty(ref _videoFilename, value))
                    ValidateFilename();
            }
        }

        public static string WindowTitle => UILangProviderM.Current["FilenameScribe.WindowTitle"];
        public static string HeaderText => UILangProviderM.Current["FilenameScribe.Header"];
        public static string PlaceholderText => UILangProviderM.Current["FilenameScribe.Placeholder"];
        public static string ExtensionText => VideoExtension;
        public static string AutoRuleDate => UILangProviderM.Current["FilenameScribe.AutoRuleDate"];
        public static string AutoRuleSeason => UILangProviderM.Current["FilenameScribe.AutoRuleSeason"];
        public static string AutoRuleVersion => UILangProviderM.Current["FilenameScribe.AutoRuleVersion"];
        public static string FooterHint => UILangProviderM.Current["FilenameScribe.FooterHint"];

        public FilenameScribeModalVM(Action closeAction)
        {
            _closeAction = closeAction;
            CloseCmd = new CloseModalCmd(closeAction);
            BuildChecklist();
            BuildButtonGroup();
            ValidateFilename();
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        private void BuildChecklist()
        {
            FilenameChecklist.Clear();
            FilenameChecklist.Add(new ChecklistEntryVM { Text = UILangProviderM.Current["FilenameScribe.CheckLength"] });
            FilenameChecklist.Add(new ChecklistEntryVM { Text = UILangProviderM.Current["FilenameScribe.CheckReserved"] });
            FilenameChecklist.Add(new ChecklistEntryVM { Text = UILangProviderM.Current["FilenameScribe.CheckInvalidChars"] });
            FilenameChecklist.Add(new ChecklistEntryVM { Text = UILangProviderM.Current["FilenameScribe.CheckExtendedChars"] });
            FilenameChecklist.Add(new ChecklistEntryVM { Text = UILangProviderM.Current["FilenameScribe.CheckSpaces"] });
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
            if (Clipboard.ContainsText())
                VideoFilename = Clipboard.GetText().Trim();
        }

        private void Confirm()
        {
            _closeAction();
        }

        private void ValidateFilename()
        {
            if (FilenameChecklist.Count < 5 || FilenameButtons is null) return;

            string filename = VideoFilename.Trim();
            FilenameChecklist[0].Status = filename.Length is >= 30 and <= 50 ? StatusType.Success : StatusType.Error;
            FilenameChecklist[1].Status = !ReservedNameRegex.IsMatch(filename) ? StatusType.Success : StatusType.Error;
            FilenameChecklist[2].Status = !ContainsInvalidFileNameChar(filename) ? StatusType.Success : StatusType.Error;
            FilenameChecklist[3].Status = !filename.Any(char.IsSurrogate) && filename.All(c => c <= 0x7f) ? StatusType.Success : StatusType.Error;
            FilenameChecklist[4].Status = !filename.Contains(' ') ? StatusType.Success : StatusType.Error;

            FilenameButtons.B3_3IsEnabled = FilenameChecklist.All(e => e.Status == StatusType.Success);
        }

        private static bool ContainsInvalidFileNameChar(string filename)
        {
            return filename.Length == 0
                || filename.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0
                || filename.Contains('&')
                || filename.EndsWith('.')
                || filename.EndsWith(' ');
        }

        private void OnLanguageChanged()
        {
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(HeaderText));
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
    }
}
