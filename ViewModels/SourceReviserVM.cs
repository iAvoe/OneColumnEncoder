using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.FFmpeg;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System.Globalization;

namespace OneColumnEncoder.ViewModels;

public class SourceReviserVM : BaseVM
{
    private const int MaxResolutionDimension = 65535;
    private readonly ModalNavS _modalNavS;
    private readonly Action<bool> _finishAction;
    private readonly Func<SourceRevisionRequest, string?> _reviseSource;
    private readonly string _rawJson;
    private readonly int _currentWidth;
    private readonly int _currentHeight;
    private readonly int _suggestedWidth;
    private readonly int _suggestedHeight;
    private readonly (int numerator, int denominator)? _sourceFrameRate;
    private IReadOnlyList<VideoAnalysisHypothesisOption> _hypotheses = [];
    private VideoAnalysisHypothesisOption? _selectedHypothesis;
    private string _resolutionWidthText;
    private string _resolutionHeightText;
    private string _outputFrameRateNumeratorText = string.Empty;
    private string _outputFrameRateDenominatorText = string.Empty;

    public SourceReviserVM(
        ModalNavS modalNavS,
        Action closeAction,
        Action<bool> finishAction,
        Func<SourceRevisionRequest, string?> reviseSource,
        string rawJson,
        int currentWidth,
        int currentHeight,
        int suggestedWidth,
        int suggestedHeight)
    {
        _modalNavS = modalNavS;
        _finishAction = finishAction;
        _reviseSource = reviseSource;
        _rawJson = rawJson;
        _currentWidth = currentWidth;
        _currentHeight = currentHeight;
        _suggestedWidth = suggestedWidth;
        _suggestedHeight = suggestedHeight;
        _sourceFrameRate = FFProbeFPSReviser.ReadSourceFrameRate(rawJson);

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
        UseCurrentFrameRateCommand = new ActionCmd(
            _ => SetFrameRateText(_sourceFrameRate),
            _ => _sourceFrameRate.HasValue);
        PatternDropdown.SelectionChangedCommand = new ActionCmd(_ => OnPatternSelectionChanged());
        BuildPatternListing();
        SelectedHypothesis = _hypotheses.Count > 0 ? _hypotheses[0] : null;

        FinishButtons = ButtonGroupVM.CreateTwoButton(
            SourceReviserLangProvider.Current["SourceReviser.Cancel"],
            SourceReviserLangProvider.Current["SourceReviser.Confirm"],
            new CloseModalCmd(closeAction),
            new ActionCmd(_ => Confirm()));

        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    public static string WindowTitle => "1cenc Source Reviser";
    public static string Description => SourceReviserLangProvider.Current["SourceReviser.Description"];
    public static string SettingsHeader => SourceReviserLangProvider.Current["SourceReviser.SettingsHeader"];
    public static string WidthLabel => SourceReviserLangProvider.Current["SourceReviser.WidthLabel"];
    public static string HeightLabel => SourceReviserLangProvider.Current["SourceReviser.HeightLabel"];
    public static string CurrentResolutionLabel => SourceReviserLangProvider.Current["SourceReviser.CurrentLabel"];
    public static string SuggestedResolutionLabel => SourceReviserLangProvider.Current["SourceReviser.SuggestedLabel"];
    public static string EvenResolutionHint => SourceReviserLangProvider.Current["SourceReviser.EvenResolutionHint"];
    public static string PatternLabel => SourceReviserLangProvider.Current["SourceReviser.PatternLabel"];
    public static string InputFpsLabel => SourceReviserLangProvider.Current["SourceReviser.InputFpsLabel"];
    public static string OutputFpsLabel => SourceReviserLangProvider.Current["SourceReviser.OutputFpsLabel"];
    public static string OutputFpsNumeratorLabel => SourceReviserLangProvider.Current["SourceReviser.OutputFpsNumeratorLabel"];
    public static string OutputFpsDenominatorLabel => SourceReviserLangProvider.Current["SourceReviser.OutputFpsDenominatorLabel"];
    public static string OutputFramesLabel => SourceReviserLangProvider.Current["SourceReviser.OutputFramesLabel"];
    public static string WarningLabel => SourceReviserLangProvider.Current["SourceReviser.WarningLabel"];
    public string CurrentResolutionText => FormatResolution(_currentWidth, _currentHeight);
    public string SuggestedResolutionText => FormatResolution(_suggestedWidth, _suggestedHeight);
    public string CurrentFrameRateText => FormatFrameRate(_sourceFrameRate);
    public DropdownMenuVM PatternDropdown { get; } = new();

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

    public IReadOnlyList<VideoAnalysisHypothesisOption> Hypotheses
    {
        get => _hypotheses;
        private set => SetProperty(ref _hypotheses, value);
    }

    public VideoAnalysisHypothesisOption? SelectedHypothesis
    {
        get => _selectedHypothesis;
        set
        {
            if (!SetProperty(ref _selectedHypothesis, value) || value == null) return;

            SyncPatternDropdownSelection(value);
            (int numerator, int denominator) = FFProbeFPSReviser.GetDefaultOutputFrameRate(_rawJson, value.Kind);
            OutputFrameRateNumeratorText = numerator.ToString(CultureInfo.InvariantCulture);
            OutputFrameRateDenominatorText = denominator.ToString(CultureInfo.InvariantCulture);
            OnPropertyChanged(nameof(SelectedDescription));
            RefreshPreview();
        }
    }

    public string SelectedDescription => SelectedHypothesis?.Description ?? string.Empty;

    public string OutputFrameRateNumeratorText
    {
        get => _outputFrameRateNumeratorText;
        set
        {
            if (!SetProperty(ref _outputFrameRateNumeratorText, value)) return;
            RefreshPreview();
        }
    }

    public string OutputFrameRateDenominatorText
    {
        get => _outputFrameRateDenominatorText;
        set
        {
            if (!SetProperty(ref _outputFrameRateDenominatorText, value)) return;
            RefreshPreview();
        }
    }

    public string OutputFrameRateText =>
        TryParseFrameRate(out int numerator, out int denominator)
            ? $"{numerator}/{denominator}"
            : "?";

    public string OutputFrameCountText
    {
        get
        {
            FPSReviserResult? result = GetPreviewResult();
            if (result?.OutputFrameCount is not > 0)
                return SourceReviserLangProvider.Current["SourceReviser.OutputFramesUnknown"];

            string kind = result.FrameCountKind switch
            {
                VideoAnalysisFrameCountKind.Exact => SourceReviserLangProvider.Current["SourceReviser.OutputFramesExact"],
                VideoAnalysisFrameCountKind.Estimated => SourceReviserLangProvider.Current["SourceReviser.OutputFramesEstimated"],
                _ => SourceReviserLangProvider.Current["SourceReviser.OutputFramesUnknown"]
            };
            return $"{result.OutputFrameCount.Value.ToString("N0", CultureInfo.InvariantCulture)} ({kind})";
        }
    }

    public string WarningText
    {
        get
        {
            FPSReviserResult? result = GetPreviewResult();
            if (result?.Kind == VideoAnalysisHypothesisKind.EuroPulldown)
                return SourceReviserLangProvider.Current["SourceReviser.AudioSpeedWarning"];
            return result?.FrameCountKind == VideoAnalysisFrameCountKind.Unknown
                ? SourceReviserLangProvider.Current["SourceReviser.UnknownFramesWarning"]
                : string.Empty;
        }
    }

    public bool HasWarning => !string.IsNullOrWhiteSpace(WarningText);
    public int ResolutionWidth { get; private set; }
    public int ResolutionHeight { get; private set; }
    public ActionCmd UseCurrentResolutionCommand { get; }
    public ActionCmd UseSuggestedResolutionCommand { get; }
    public ActionCmd UseCurrentFrameRateCommand { get; }
    public ButtonGroupVM FinishButtons { get; }

    private void Confirm()
    {
        if (!TryParseResolution(out int width, out int height))
        {
            ShowError(SourceReviserLangProvider.Current["SourceReviser.InvalidInput"]);
            return;
        }

        if (SelectedHypothesis == null || !TryParseFrameRate(out int numerator, out int denominator))
        {
            ShowError(SourceReviserLangProvider.Current["SourceReviser.InvalidFps"]);
            return;
        }

        if (SelectedHypothesis.IsUnsupported)
        {
            ShowError(SourceReviserLangProvider.Current["SourceReviser.UnsupportedPattern"]);
            return;
        }

        SourceRevisionRequest request = new(
            width,
            height,
            new(SelectedHypothesis.Id, numerator, denominator));
        string? error = _reviseSource(request);
        if (!string.IsNullOrWhiteSpace(error))
        {
            ShowError(error);
            return;
        }

        ResolutionWidth = width;
        ResolutionHeight = height;
        _finishAction(true);
    }

    private void BuildPatternListing()
    {
        _hypotheses = VideoAnalysisHypothesisCatalog.GetOptions();
        Hypotheses = _hypotheses;
        PatternDropdown.Items.Clear();

        foreach (VideoAnalysisHypothesisOption option in _hypotheses)
        {
            PatternDropdown.Items.Add(new DropdownItemM(option.DisplayName)
            {
                Tag = option
            });
        }
    }

    private void OnPatternSelectionChanged()
    {
        if (PatternDropdown.SelectedItem?.Tag is VideoAnalysisHypothesisOption selectedHypothesis)
            SelectedHypothesis = selectedHypothesis;
    }

    private void SyncPatternDropdownSelection(VideoAnalysisHypothesisOption selectedHypothesis)
    {
        DropdownItemM? selectedItem = null;
        for (int i = 0; i < PatternDropdown.Items.Count; i++)
        {
            DropdownItemM item = PatternDropdown.Items[i];
            if (item.Tag is VideoAnalysisHypothesisOption option && option.Id == selectedHypothesis.Id)
            {
                selectedItem = item;
                break;
            }
        }

        if (selectedItem == null && PatternDropdown.Items.Count > 0)
            selectedItem = PatternDropdown.Items[0];

        if (!ReferenceEquals(PatternDropdown.SelectedItem, selectedItem))
            PatternDropdown.SelectedItem = selectedItem;
    }

    private FPSReviserResult? GetPreviewResult()
    {
        if (SelectedHypothesis == null || !TryParseFrameRate(out int numerator, out int denominator))
            return null;

        try
        {
            return FFProbeFPSReviser.Apply(
                _rawJson,
                new(SelectedHypothesis.Id, numerator, denominator));
        }
        catch
        {
            return null;
        }
    }

    private void RefreshPreview()
    {
        OnPropertyChanged(nameof(OutputFrameRateText));
        OnPropertyChanged(nameof(OutputFrameCountText));
        OnPropertyChanged(nameof(WarningText));
        OnPropertyChanged(nameof(HasWarning));
    }

    private void SetResolutionText(int width, int height)
    {
        ResolutionWidthText = width.ToString(CultureInfo.InvariantCulture);
        ResolutionHeightText = height.ToString(CultureInfo.InvariantCulture);
    }

    private void SetFrameRateText((int numerator, int denominator)? frameRate)
    {
        if (!frameRate.HasValue)
            return;

        OutputFrameRateNumeratorText = frameRate.Value.numerator.ToString(CultureInfo.InvariantCulture);
        OutputFrameRateDenominatorText = frameRate.Value.denominator.ToString(CultureInfo.InvariantCulture);
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

    private bool TryParseFrameRate(out int numerator, out int denominator)
    {
        bool hasNumerator = int.TryParse(
            OutputFrameRateNumeratorText,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out numerator);
        bool hasDenominator = int.TryParse(
            OutputFrameRateDenominatorText,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out denominator);
        return hasNumerator && hasDenominator && numerator > 0 && denominator > 0;
    }

    private static string FormatResolution(int width, int height) =>
        width > 0 && height > 0
            ? string.Format(SourceReviserLangProvider.Current["SourceReviser.ResolutionFormat"], width, height)
            : SourceReviserLangProvider.Current["SourceReviser.UnknownResolution"];

    private static string FormatFrameRate((int numerator, int denominator)? frameRate) =>
        frameRate.HasValue
            ? $"{frameRate.Value.numerator}/{frameRate.Value.denominator}"
            : SourceReviserLangProvider.Current["SourceReviser.UnknownFrameRate"];

    private void ShowError(string message) =>
        new OpenErrModalCmd(_modalNavS, WindowTitle, message).Execute(null);

    private void OnLanguageChanged()
    {
        string? selectedId = SelectedHypothesis?.Id;
        string numerator = OutputFrameRateNumeratorText;
        string denominator = OutputFrameRateDenominatorText;
        BuildPatternListing();
        SelectedHypothesis = null;
        for (int i = 0; i < Hypotheses.Count; i++)
        {
            if (Hypotheses[i].Id == selectedId)
            {
                SelectedHypothesis = Hypotheses[i];
                break;
            }
        }

        if (SelectedHypothesis == null && Hypotheses.Count > 0)
            SelectedHypothesis = Hypotheses[0];
        OutputFrameRateNumeratorText = numerator;
        OutputFrameRateDenominatorText = denominator;

        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(SettingsHeader));
        OnPropertyChanged(nameof(WidthLabel));
        OnPropertyChanged(nameof(HeightLabel));
        OnPropertyChanged(nameof(CurrentResolutionLabel));
        OnPropertyChanged(nameof(SuggestedResolutionLabel));
        OnPropertyChanged(nameof(EvenResolutionHint));
        OnPropertyChanged(nameof(PatternLabel));
        OnPropertyChanged(nameof(InputFpsLabel));
        OnPropertyChanged(nameof(OutputFpsLabel));
        OnPropertyChanged(nameof(OutputFpsNumeratorLabel));
        OnPropertyChanged(nameof(OutputFpsDenominatorLabel));
        OnPropertyChanged(nameof(OutputFramesLabel));
        OnPropertyChanged(nameof(WarningLabel));
        OnPropertyChanged(nameof(CurrentResolutionText));
        OnPropertyChanged(nameof(SuggestedResolutionText));
        OnPropertyChanged(nameof(CurrentFrameRateText));
        OnPropertyChanged(nameof(SelectedDescription));
        RefreshPreview();

        FinishButtons.B2_1Text = SourceReviserLangProvider.Current["SourceReviser.Cancel"];
        FinishButtons.B2_2Text = SourceReviserLangProvider.Current["SourceReviser.Confirm"];
    }

    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
