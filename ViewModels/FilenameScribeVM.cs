using OneColumnEncoder.Validation;

namespace OneColumnEncoder.ViewModels;

public partial class FilenameScribeVM : BaseVM
{
    private const string PossibleExtensions = ".mp4|.hevc|.ivf";

    private readonly Action _closeAction;
    private readonly ToolItemCardVM _outputSettingItem;
    public CloseModalCmd CloseCmd { get; }
    public ButtonGroupVM FilenameFinishButtons { get; private set; } = null!;
    public ObservableCollection<ChecklistEntryVM> SevereIssueChecklist { get; } = [];
    public ObservableCollection<ChecklistEntryVM> GeneralIssueChecklist { get; } = [];

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

    public static string WindowTitle => FilenameScribeModalLangProvider.WindowTitle;
    public static string MiniHeader => FilenameScribeModalLangProvider.Current["FilenameScribe.MiniHeader"];
    public static string PlaceholderText => FilenameScribeModalLangProvider.Current["FilenameScribe.Placeholder"];
    public static string ExtensionText => PossibleExtensions;
    public static string PreviewHeader => FilenameScribeModalLangProvider.Current["FilenameScribe.PreviewHeader"];
    public static string Preview30Label => FilenameScribeModalLangProvider.Current["FilenameScribe.Preview30Label"];
    public static string Preview25Label => FilenameScribeModalLangProvider.Current["FilenameScribe.Preview25Label"];
    public static string Preview20Label => FilenameScribeModalLangProvider.Current["FilenameScribe.Preview20Label"];
    public static string Preview15Label => FilenameScribeModalLangProvider.Current["FilenameScribe.Preview15Label"];
    public static string FormatCheckHeader => FilenameScribeModalLangProvider.Current["FilenameScribe.FormatCheckHeader"];
    public static string SevereIssueHeader => FilenameScribeModalLangProvider.Current["FilenameScribe.SevereIssueHeader"];
    public static string GeneralIssueHeader => FilenameScribeModalLangProvider.Current["FilenameScribe.GeneralIssueHeader"];
    public static string SelfCheckHeader => FilenameScribeModalLangProvider.Current["FilenameScribe.SelfCheckHeader"];
    public static string SelfCheckDate => FilenameScribeModalLangProvider.Current["FilenameScribe.SelfCheck1"];
    public static string SelfCheckSeason => FilenameScribeModalLangProvider.Current["FilenameScribe.SelfCheck2"];
    public static string SelfCheckVersion => FilenameScribeModalLangProvider.Current["FilenameScribe.SelfCheck3"];
    public static string FooterHint => FilenameScribeModalLangProvider.Current["FilenameScribe.FooterHint"];

    public FilenameScribeVM(Action closeAction, ToolItemCardVM outputSettingItem)
    {
        _closeAction = closeAction;
        _outputSettingItem = outputSettingItem;
        _videoFilename = OutputPath.GetInitialFilename(outputSettingItem.P1TextData, outputSettingItem.P2TextData);
        CloseCmd = new CloseModalCmd(closeAction);
        BuildChecklist();
        BuildButtonGroup();
        ValidateFilename();
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    private void BuildChecklist()
    {
        UpdateChecklist(SevereIssueChecklist,
        [
            "FilenameScribe.CheckEmpty",
            "FilenameScribe.CheckInvalidChars",
            "FilenameScribe.CheckCombiningMarks",
            "FilenameScribe.CheckSpecialSpaceVariants",
            "FilenameScribe.CheckReserved"
        ]);
        UpdateChecklist(GeneralIssueChecklist,
        [
            "FilenameScribe.CheckLength",
            "FilenameScribe.CheckSpaces",
            "FilenameScribe.CheckExtendedChars"
        ]);
    }

    private static void UpdateChecklist(ObservableCollection<ChecklistEntryVM> checklist, IEnumerable<string> keys)
    {
        checklist.Clear();
        foreach (var key in keys)
            checklist.Add(new ChecklistEntryVM { Text = FilenameScribeModalLangProvider.Current[key] });
    }

    private void BuildButtonGroup()
    {
        FilenameFinishButtons = ButtonGroupVM.CreateTwoButton(
            FilenameScribeModalLangProvider.Current["FilenameScribe.Cancel"],
            FilenameScribeModalLangProvider.Current["FilenameScribe.Confirm"],
            CloseCmd,
            new ActionCmd(_ => Confirm(), _ => FilenameFinishButtons.B2_2IsEnabled));
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
            InitialDirectory = OutputPath.GetInitialDirectory(_outputSettingItem.P2TextData)
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

    #region Command State Queries
    private bool CanConfirm() => FilenameFinishButtons.B2_2IsEnabled;
    #endregion

    private void ValidateFilename()
    {
        if (SevereIssueChecklist.Count < 5 || GeneralIssueChecklist.Count < 3 || FilenameFinishButtons is null) return;

        string filename = VideoFilename.Trim();
        SetChecklistStatus(SevereIssueChecklist, 0, !string.IsNullOrWhiteSpace(VideoFilename));
        SetChecklistStatus(SevereIssueChecklist, 1, FilenameValidation.HasNoInvalidChars(filename));
        SetChecklistStatus(SevereIssueChecklist, 2, FilenameValidation.HasUnicodeCombiningMarks(filename));
        SetChecklistStatus(SevereIssueChecklist, 3, FilenameValidation.HasNoSpecialSpaceVariants(VideoFilename));
        SetChecklistStatus(SevereIssueChecklist, 4, FilenameValidation.IsNotReservedName(filename));
        SetChecklistStatus(GeneralIssueChecklist, 0, FilenameValidation.IsValidLength(filename), useWarning: true);
        GeneralIssueChecklist[1].Status = FilenameValidation.HasSpaces(VideoFilename)
            ? StatusType.Warning
            : StatusType.Success;
        GeneralIssueChecklist[2].Status = FilenameValidation.HasNoExtendedChars(filename)
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
        OnPropertyChanged(nameof(FilenameFinishButtons));
        ValidateFilename();
    }

    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        base.Dispose();
        GC.SuppressFinalize(this);
    }

}
