namespace OneColumnEncoder.ViewModels;

public class SrcReviserVM : BaseVM
{
    private const int MaxResolutionDimension = 65535;
    private readonly ModalNavS _modalNavS;
    private readonly Action<bool> _finishAction;
    private readonly Func<SrcRevisionRequest, string?> _reviseSource;
    private readonly int _currentWidth;
    private readonly int _currentHeight;
    private readonly int _suggestedWidth;
    private readonly int _suggestedHeight;
    private string _resolutionWidthText;
    private string _resolutionHeightText;

    public SrcReviserVM(
        ModalNavS modalNavS,
        Action closeAction,
        Action<bool> finishAction,
        Func<SrcRevisionRequest, string?> reviseSource,
        int currentWidth,
        int currentHeight,
        int suggestedWidth,
        int suggestedHeight)
    {
        _modalNavS = modalNavS;
        _finishAction = finishAction;
        _reviseSource = reviseSource;
        _currentWidth = currentWidth;
        _currentHeight = currentHeight;
        _suggestedWidth = suggestedWidth;
        _suggestedHeight = suggestedHeight;

        ResolutionWidth = suggestedWidth;
        ResolutionHeight = suggestedHeight;
        _resolutionWidthText = suggestedWidth > 0
            ? suggestedWidth.ToString(CultureInfo.InvariantCulture)
            : string.Empty;
        _resolutionHeightText = suggestedHeight > 0
            ? suggestedHeight.ToString(CultureInfo.InvariantCulture)
            : string.Empty;

        UseCurrentResolutionCommand = new ActionCmd(
            _ => SetResolutionText(_currentWidth, _currentHeight),
            _ => _currentWidth > 0 && _currentHeight > 0);
        UseSuggestedResolutionCommand = new ActionCmd(
            _ => SetResolutionText(_suggestedWidth, _suggestedHeight),
            _ => _suggestedWidth > 0 && _suggestedHeight > 0);

        FinishButtons = ButtonGroupVM.CreateTwoButton(
            SrcReviserLangProvider.Current["SrcReviser.Cancel"],
            SrcReviserLangProvider.Current["SrcReviser.Confirm"],
            new CloseModalCmd(closeAction),
            new ActionCmd(_ => Confirm()));
        FinishButtons.B2_1Icon = SvgIconProvider.GameRefresh;

        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    public static string WindowTitle => SrcReviserLangProvider.WindowTitle;
    public static string Description => SrcReviserLangProvider.Current["SrcReviser.Description"];
    public static string SettingsHeader => SrcReviserLangProvider.Current["SrcReviser.SettingsHeader"];
    public static string WidthLabel => SrcReviserLangProvider.Current["SrcReviser.WidthLabel"];
    public static string HeightLabel => SrcReviserLangProvider.Current["SrcReviser.HeightLabel"];
    public static string CurrentResolutionLabel => SrcReviserLangProvider.Current["SrcReviser.CurrentLabel"];
    public static string SuggestedResolutionLabel => SrcReviserLangProvider.Current["SrcReviser.SuggestedLabel"];
    public static string EvenResolutionHint => SrcReviserLangProvider.Current["SrcReviser.EvenResolutionHint"];
    public string CurrentResolutionText => FormatResolution(_currentWidth, _currentHeight);
    public string SuggestedResolutionText => FormatResolution(_suggestedWidth, _suggestedHeight);

    public string ResolutionWidthText
    {
        get => _resolutionWidthText;
        set => SetProperty(ref _resolutionWidthText, value);
    }

    public string ResolutionHeightText
    {
        get => _resolutionHeightText;
        set => SetProperty(ref _resolutionHeightText, value);
    }

    public int ResolutionWidth { get; private set; }
    public int ResolutionHeight { get; private set; }
    public ActionCmd UseCurrentResolutionCommand { get; }
    public ActionCmd UseSuggestedResolutionCommand { get; }
    public ButtonGroupVM FinishButtons { get; }

    private void Confirm()
    {
        if (!TryParseResolution(out int width, out int height))
        {
            ShowError(SrcReviserLangProvider.Current["SrcReviser.InvalidInput"]);
            return;
        }

        string? error = _reviseSource(new SrcRevisionRequest(width, height));
        if (!string.IsNullOrWhiteSpace(error))
        {
            ShowError(error);
            return;
        }

        ResolutionWidth = width;
        ResolutionHeight = height;
        _finishAction(true);
    }

    private void SetResolutionText(int width, int height)
    {
        ResolutionWidthText = width.ToString(CultureInfo.InvariantCulture);
        ResolutionHeightText = height.ToString(CultureInfo.InvariantCulture);
    }

    private bool TryParseResolution(out int width, out int height)
    {
        bool hasWidth = TryParseDimension(ResolutionWidthText, out width);
        bool hasHeight = TryParseDimension(ResolutionHeightText, out height);
        return hasWidth && hasHeight;
    }

    private static bool TryParseDimension(string? text, out int value) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value)
        && value > 0
        && value <= MaxResolutionDimension;

    private static string FormatResolution(int width, int height) =>
        width > 0 && height > 0
            ? string.Format(SrcReviserLangProvider.ResolutionFormat, width, height)
            : SrcReviserLangProvider.UnknownResolution;

    private void ShowError(string message) =>
        new OpenErrModalCmd(_modalNavS, WindowTitle, message).Execute(null);

    private void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(SettingsHeader));
        OnPropertyChanged(nameof(WidthLabel));
        OnPropertyChanged(nameof(HeightLabel));
        OnPropertyChanged(nameof(CurrentResolutionLabel));
        OnPropertyChanged(nameof(SuggestedResolutionLabel));
        OnPropertyChanged(nameof(EvenResolutionHint));
        OnPropertyChanged(nameof(CurrentResolutionText));
        OnPropertyChanged(nameof(SuggestedResolutionText));

        FinishButtons.B2_1Text = SrcReviserLangProvider.Current["SrcReviser.Cancel"];
        FinishButtons.B2_2Text = SrcReviserLangProvider.Current["SrcReviser.Confirm"];
    }

    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
