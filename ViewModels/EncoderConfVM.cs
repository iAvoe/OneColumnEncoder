namespace OneColumnEncoder.ViewModels;

public class EncoderConfVM : BaseVM
{
    private EncoderConfLangProvider _lang =
        new(UILangProvider.Current.LanguageCode);
    public EncoderConfLangProvider Lang
    {
        get => _lang;
        private set => SetProperty(ref _lang, value);
    }
    private readonly EncoderConfM _model;
    private readonly ToolItemCardVM? _targetItem;
    public CloseModalCmd CloseCmd { get; }
    public ActionCmd ConfirmCmd { get; }
    public ButtonGroupVM FinishButtons { get; }
    public ImgABPvVM PreviewVM { get; }

    private bool _isPreviewBusy;
    public bool IsPreviewBusy
    {
        get => _isPreviewBusy;
        private set
        {
            if (!SetProperty(ref _isPreviewBusy, value)) return;
            OnPropertyChanged(nameof(IsConfigUiEnabled));
        }
    }

    public bool IsConfigUiEnabled => !IsPreviewBusy;

    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    public static string WindowTitle => EncoderConfLangProvider.WindowTitle;
    public static string TitleText => EncoderConfLangProvider.TitleText;
    public string RateControlTitle => Lang.RateControlTitle;
    public string CustomParamsTitle => Lang.CustomParamsTitle;
    public string CrfModeText => Lang.CrfModeText;
    public string AbrModeText => Lang.AbrModeText;
    public string X264Text => Lang.X264Text;
    public string X265Text => Lang.X265Text;
    public string SvtAv1Text => Lang.SvtAv1Text;
    public string X264DefaultText => Lang.X264DefaultText;
    public string X265DefaultText => Lang.X265DefaultText;
    public string SvtAv1DefaultText => Lang.SvtAv1DefaultText;
    public string X264AbrValueText => Lang.X264AbrValueText;
    public string X265AbrValueText => Lang.X265AbrValueText;
    public string SvtAv1AbrValueText => Lang.SvtAv1AbrValueText;
    public string BasicParamsText => Lang.BasicParamsText;
    public string KeyframeSecondsText => Lang.KeyframeSecondsText;
    public string ThirdPartyParamsText => Lang.ThirdPartyParamsText;
    public string FreeTextControlTitle => Lang.FreeTextControlTitle;
    public string PreviewFreeTextControlTitle => Lang.PreviewFreeTextControlTitle;
    public string PreviewFreeTextHint => Lang.PreviewFreeTextHint;
    public string X264ModText => Lang.X264ModText;
    public string X265JpsdrAqText => Lang.X265JpsdrAqText;
    public string X265JpsdrDarkText => Lang.X265JpsdrDarkText;
    public string X265JpsdrTextureText => Lang.X265JpsdrTextureText;
    public string SvtAv1EssentialDl2Text => Lang.SvtAv1EssentialDl2Text;
    public string SvtAv1EssentialAutoTileText => Lang.SvtAv1EssentialAutoTileText;
    public string CancelButtonText => Lang.CancelButtonText;
    public string ConfirmButtonText => Lang.ConfirmButtonText;
    public string CrfHintText => Lang.CrfHintText;
    public string AbrHintText => Lang.AbrHintText;
    public string KeyframeHintText1 => Lang.KeyframeHintText1;
    public string KeyframeHintText2 => Lang.KeyframeHintText2;
    public string ThirdPartyHintText1 => Lang.ThirdPartyHintText1;
    public string ThirdPartyHintText2 => Lang.ThirdPartyHintText2;
    public string ThirdPartyHintText3 => Lang.ThirdPartyHintText3;
    public string ThirdPartyHintText4 => Lang.ThirdPartyHintText4;
    public string BlankPresetText => Lang.BlankPresetText;
    public string BlankPresetHint => Lang.BlankPresetHint;
    public string VvencText => Lang.VvencText;
    public string VvencHintText => Lang.VvencHintText;

    private string _freeTextParamsX264 = string.Empty;
    public string FreeTextParamsX264
    {
        get => _freeTextParamsX264;
        set => SetProperty(ref _freeTextParamsX264, value);
    }

    private string _freeTextParamsX265 = string.Empty;
    public string FreeTextParamsX265
    {
        get => _freeTextParamsX265;
        set => SetProperty(ref _freeTextParamsX265, value);
    }

    private string _freeTextParamsSvtAv1 = string.Empty;
    public string FreeTextParamsSvtAv1
    {
        get => _freeTextParamsSvtAv1;
        set => SetProperty(ref _freeTextParamsSvtAv1, value);
    }

    private string _previewFreeTextParamsX264 = string.Empty;
    public string PreviewFreeTextParamsX264
    {
        get => _previewFreeTextParamsX264;
        set => SetProperty(ref _previewFreeTextParamsX264, value);
    }

    private string _previewFreeTextParamsX265 = string.Empty;
    public string PreviewFreeTextParamsX265
    {
        get => _previewFreeTextParamsX265;
        set => SetProperty(ref _previewFreeTextParamsX265, value);
    }

    private string _previewFreeTextParamsSvtAv1 = string.Empty;
    public string PreviewFreeTextParamsSvtAv1
    {
        get => _previewFreeTextParamsSvtAv1;
        set => SetProperty(ref _previewFreeTextParamsSvtAv1, value);
    }

    private bool IsAbrTabSelected => SelectedTabIndex == 1;

    public DropdownMenuVM X264ModeDropdown { get; } = new();
    public DropdownMenuVM X265ModeDropdown { get; } = new();
    public DropdownMenuVM SvtAv1ModeDropdown { get; } = new();
    public DropdownMenuVM VvencModeDropdown { get; } = new();

    private int _x264Crf = 23;
    public int X264Crf
    {
        get => _x264Crf;
        set => SetProperty(ref _x264Crf, value);
    }
    private int _x265Crf = 28;
    public int X265Crf
    {
        get => _x265Crf;
        set => SetProperty(ref _x265Crf, value);
    }
    private int _svtAv1Crf = 35;
    public int SvtAv1Crf
    {
        get => _svtAv1Crf;
        set => SetProperty(ref _svtAv1Crf, value);
    }
    private int _vvencQp = 32;
    public int VvencQp
    {
        get => _vvencQp;
        set => SetProperty(ref _vvencQp, value);
    }

    private int _x264Abr = 209;
    public int X264Abr
    {
        get => _x264Abr; set => SetProperty(ref _x264Abr, value);
    }
    private int _x265Abr = 70;
    public int X265Abr
    {
        get => _x265Abr; set => SetProperty(ref _x265Abr, value);
    }
    private int _svtAv1Abr = 10;
    public int SvtAv1Abr
    {
        get => _svtAv1Abr; set => SetProperty(ref _svtAv1Abr, value);
    }

    private int _x264Keyframe = 9;
    public int X264Keyframe
    {
        get => _x264Keyframe;
        set => SetProperty(ref _x264Keyframe, value);
    }
    private int _x265Keyframe = 7;
    public int X265Keyframe
    {
        get => _x265Keyframe;
        set => SetProperty(ref _x265Keyframe, value);
    }
    private int _svtAv1Keyframe = 9;
    public int SvtAv1Keyframe
    {
        get => _svtAv1Keyframe;
        set => SetProperty(ref _svtAv1Keyframe, value);
    }
    private bool _x264Mod;
    public bool X264Mod
    {
        get => _x264Mod;
        set => SetProperty(ref _x264Mod, value);
    }
    private bool _x265Aq;
    public bool X265Aq
    {
        get => _x265Aq;
        set
        {
            if (!SetProperty(ref _x265Aq, value)) return;
            OnPropertyChanged(nameof(IsX265DarkEnabled));
            OnPropertyChanged(nameof(IsX265TextureEnabled));
            if (!value)
            {
                X265Dark = false;
                X265Texture = false;
            }
        }
    }
    private bool _x265Dark;
    public bool X265Dark
    {
        get => _x265Dark;
        set => SetProperty(ref _x265Dark, value);
    }
    private bool _x265Texture;
    public bool X265Texture
    {
        get => _x265Texture;
        set => SetProperty(ref _x265Texture, value);
    }
    private bool _svtAv1Dl2;
    public bool SvtAv1Dl2
    {
        get => _svtAv1Dl2;
        set => SetProperty(ref _svtAv1Dl2, value);
    }
    private bool _svtAv1AutoTile;
    public bool SvtAv1AutoTile
    {
        get => _svtAv1AutoTile;
        set => SetProperty(ref _svtAv1AutoTile, value);
    }

    public bool IsX265DarkEnabled => X265Aq;
    public bool IsX265TextureEnabled => X265Aq;

    public static IEnumerable<string> X264CrfLabels => ["0", "13", "17", "21", "25"];
    public static IEnumerable<string> X265CrfLabels => ["0", "17", "21", "25", "30"];
    public static IEnumerable<string> SvtAv1CrfLabels => ["0", "28", "33", "38", "43"];
    public static IEnumerable<string> X264AbrLabels => ["500", "200 Mbps", "70 Mbps", "10"];
    public static IEnumerable<string> X265AbrLabels => ["500", "200 Mbps", "70 Mbps", "10"];
    public static IEnumerable<string> SvtAv1AbrLabels => ["500", "200 Mbps", "70 Mbps", "10"];
    public static IEnumerable<string> X264KeyframeLabels => ["6", "9 ", "12", "15"];
    public static IEnumerable<string> X265KeyframeLabels => ["4", "7", "10", "13"];
    public static IEnumerable<string> SvtAv1KeyframeLabels => ["6", "9", "12", "15"];
    public static IEnumerable<string> VvencQpLabels => ["0", "16", "32", "48", "63"];

    public EncoderConfVM(
        Action closeAction,
        ToolItemCardVM? targetItem,
        Stores.ModalNavS modalNavS,
        string? ffmpegPath,
        string? sourceVideoPath,
        string? sourceFfprobeJson)
    {
        _model = EncoderConfM.Load();
        _targetItem = targetItem;
        CloseCmd = new CloseModalCmd(closeAction);
        ConfirmCmd = new ActionCmd(_ =>
        {
            SaveModel();
            ApplySettingsToTarget();
            closeAction();
        });
        FinishButtons = ButtonGroupVM.CreateTwoButton(
            CancelButtonText, ConfirmButtonText, CloseCmd, ConfirmCmd);
        PopulateDropdowns();
        LoadModelToUi();
        PreviewVM = new ImgABPvVM(this, modalNavS, ffmpegPath, sourceVideoPath, sourceFfprobeJson);
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    #region Dropdown initialization
    private void PopulateDropdowns()
    {
        AddBlankPresetItem(X264ModeDropdown);
        AddBlankPresetItem(X265ModeDropdown);
        AddBlankPresetItem(SvtAv1ModeDropdown);

        foreach (EncoderPresetItem preset in EncoderPresetsM.X264Presets)
            X264ModeDropdown.Items.Add(new DropdownItemM(Lang[preset.NameKey]) { Tag = preset.Key });
        foreach (EncoderPresetItem preset in EncoderPresetsM.X265Presets)
            X265ModeDropdown.Items.Add(new DropdownItemM(Lang[preset.NameKey]) { Tag = preset.Key });
        foreach (EncoderPresetItem preset in EncoderPresetsM.SvtAv1Presets)
            SvtAv1ModeDropdown.Items.Add(new DropdownItemM(Lang[preset.NameKey]) { Tag = preset.Key });

        // VVenC preview presets — always qpa enabled
        VvencModeDropdown.Items.Add(new DropdownItemM("qpa medium") { Tag = 0 });
        VvencModeDropdown.Items.Add(new DropdownItemM("qpa slower") { Tag = 1 });
        VvencModeDropdown.Items.Add(new DropdownItemM("qpa slow") { Tag = 2 });

        SelectDropdownByKey(X264ModeDropdown, _model.X264Mode);
        SelectDropdownByKey(X265ModeDropdown, _model.X265Mode);
        SelectDropdownByKey(SvtAv1ModeDropdown, _model.SvtAv1Mode);
        SelectDropdownByKey(VvencModeDropdown, _model.VvencMode);
    }

    private void AddBlankPresetItem(DropdownMenuVM dropdown) =>
        dropdown.Items.Add(new DropdownItemM(BlankPresetText) { Tag = -1 });

    private static void SelectDropdownByKey(DropdownMenuVM dropdown, int key)
    {
        DropdownItemM? item = dropdown.Items.FirstOrDefault(i => i.Tag is int tag && tag == key);
        if (item != null)
            dropdown.SelectedItem = item;
    }
    #endregion

    #region Model-UI synchronization
    private void LoadModelToUi()
    {
        SelectedTabIndex = _model.RateControlMode == "ABR"
            ? 1
            : Math.Max(0, Math.Min(1, _model.EncoderModeTabIndex));
        X264Crf = _model.X264Crf;
        X265Crf = _model.X265Crf;
        SvtAv1Crf = _model.SvtAv1Crf;
        X264Abr = _model.X264Abr;
        X265Abr = _model.X265Abr;
        SvtAv1Abr = _model.SvtAv1Abr;
        X264Keyframe = _model.X264Keyframe;
        X265Keyframe = _model.X265Keyframe;
        SvtAv1Keyframe = _model.SvtAv1Keyframe;
        X264Mod = _model.X264Mod;
        X265Aq = _model.X265Aq;
        X265Dark = _model.X265Dark;
        X265Texture = _model.X265Texture;
        SvtAv1Dl2 = _model.SvtAv1Dl2;
        SvtAv1AutoTile = _model.SvtAv1AutoTile;
        VvencQp = _model.VvencQp;
        SelectDropdownByKey(VvencModeDropdown, _model.VvencMode);
        FreeTextParamsX264 = _model.CustomParamsX264;
        FreeTextParamsX265 = _model.CustomParamsX265;
        FreeTextParamsSvtAv1 = _model.CustomParamsSvtAv1;
        PreviewFreeTextParamsX264 = _model.PreviewCustomParamsX264;
        PreviewFreeTextParamsX265 = _model.PreviewCustomParamsX265;
        PreviewFreeTextParamsSvtAv1 = _model.PreviewCustomParamsSvtAv1;
    }

    private void SaveModel()
    {
        FillModelFromUi(_model, forceCrfMode: false);
        _model.Save();
    }

    public EncoderConfM CreatePreviewModel()
    {
        EncoderConfM previewModel = new();
        FillModelFromUi(previewModel, forceCrfMode: true);
        previewModel.CustomParamsX264 = PreviewFreeTextParamsX264;
        previewModel.CustomParamsX265 = PreviewFreeTextParamsX265;
        previewModel.CustomParamsSvtAv1 = PreviewFreeTextParamsSvtAv1;
        return previewModel;
    }

    public void SetPreviewBusy(bool isBusy) => IsPreviewBusy = isBusy;

    private void FillModelFromUi(EncoderConfM model, bool forceCrfMode)
    {
        model.EncoderModeTabIndex = SelectedTabIndex;
        model.RateControlMode = forceCrfMode ? "CRF" : IsAbrTabSelected ? "ABR" : "CRF";
        model.X264Crf = X264Crf;
        model.X265Crf = X265Crf;
        model.SvtAv1Crf = SvtAv1Crf;
        model.X264Abr = X264Abr;
        model.X265Abr = X265Abr;
        model.SvtAv1Abr = SvtAv1Abr;
        model.X264Keyframe = X264Keyframe;
        model.X265Keyframe = X265Keyframe;
        model.SvtAv1Keyframe = SvtAv1Keyframe;
        model.X264Mod = X264Mod;
        model.X265Aq = X265Aq;
        model.X265Dark = X265Dark;
        model.X265Texture = X265Texture;
        model.SvtAv1Dl2 = SvtAv1Dl2;
        model.SvtAv1AutoTile = SvtAv1AutoTile;
        model.VvencQp = VvencQp;
        model.VvencMode = VvencModeDropdown.SelectedItem?.Tag is int vvencMode ? vvencMode : 0;
        model.CustomParamsX264 = FreeTextParamsX264;
        model.CustomParamsX265 = FreeTextParamsX265;
        model.CustomParamsSvtAv1 = FreeTextParamsSvtAv1;
        model.PreviewCustomParamsX264 = PreviewFreeTextParamsX264;
        model.PreviewCustomParamsX265 = PreviewFreeTextParamsX265;
        model.PreviewCustomParamsSvtAv1 = PreviewFreeTextParamsSvtAv1;
        model.X264Mode = X264ModeDropdown.SelectedItem?.Tag is int x264Mode ? x264Mode : -1;
        model.X265Mode = X265ModeDropdown.SelectedItem?.Tag is int x265Mode ? x265Mode : -1;
        model.SvtAv1Mode = SvtAv1ModeDropdown.SelectedItem?.Tag is int svtAv1Mode ? svtAv1Mode : -1;
    }
    #endregion

    #region Apply settings to card
    private void ApplySettingsToTarget()
    {
        if (_targetItem is null) return;
        _targetItem.SetEncodingSummary(BuildPrimarySummary(_model), BuildSecondarySummary(_model));
    }

    public static void ApplySavedSettingsToCard(ToolItemCardVM targetItem)
    {
        EncoderConfM model = EncoderConfM.Load();
        targetItem.SetEncodingSummary(BuildPrimarySummary(model), BuildSecondarySummary(model));
    }

    private static string BuildPrimarySummary(EncoderConfM model)
    {
        if (model.RateControlMode == "ABR")
            return $"ABR {model.X264Abr},{model.X265Abr},{model.SvtAv1Abr}Mbps";

        return $"CRF {model.X264Crf},{model.X265Crf},{model.SvtAv1Crf}";
    }

    private static string BuildSecondarySummary(EncoderConfM model) =>
        $"{model.X264Keyframe},{model.X265Keyframe},{model.SvtAv1Keyframe}s";
    #endregion

    #region Language/Localization
    private void OnLanguageChanged()
    {
        Lang = new EncoderConfLangProvider(UILangProvider.Current.LanguageCode);
        RefreshDropdownTitles();
        OnPropertyChanged(nameof(TitleText));
        OnPropertyChanged(nameof(RateControlTitle));
        OnPropertyChanged(nameof(CustomParamsTitle));
        OnPropertyChanged(nameof(CrfModeText));
        OnPropertyChanged(nameof(AbrModeText));
        OnPropertyChanged(nameof(X264Text));
        OnPropertyChanged(nameof(X265Text));
        OnPropertyChanged(nameof(SvtAv1Text));
        OnPropertyChanged(nameof(X264DefaultText));
        OnPropertyChanged(nameof(X265DefaultText));
        OnPropertyChanged(nameof(SvtAv1DefaultText));
        OnPropertyChanged(nameof(BasicParamsText));
        OnPropertyChanged(nameof(KeyframeSecondsText));
        OnPropertyChanged(nameof(ThirdPartyParamsText));
        OnPropertyChanged(nameof(FreeTextControlTitle));
        OnPropertyChanged(nameof(PreviewFreeTextControlTitle));
        OnPropertyChanged(nameof(PreviewFreeTextHint));
        OnPropertyChanged(nameof(X264ModText));
        OnPropertyChanged(nameof(X265JpsdrAqText));
        OnPropertyChanged(nameof(X265JpsdrDarkText));
        OnPropertyChanged(nameof(X265JpsdrTextureText));
        OnPropertyChanged(nameof(SvtAv1EssentialDl2Text));
        OnPropertyChanged(nameof(SvtAv1EssentialAutoTileText));
        OnPropertyChanged(nameof(CancelButtonText));
        OnPropertyChanged(nameof(ConfirmButtonText));
        OnPropertyChanged(nameof(CrfHintText));
        OnPropertyChanged(nameof(AbrHintText));
        OnPropertyChanged(nameof(KeyframeHintText1));
        OnPropertyChanged(nameof(KeyframeHintText2));
        OnPropertyChanged(nameof(ThirdPartyHintText1));
        OnPropertyChanged(nameof(ThirdPartyHintText2));
        OnPropertyChanged(nameof(ThirdPartyHintText3));
        OnPropertyChanged(nameof(ThirdPartyHintText4));
        OnPropertyChanged(nameof(BlankPresetText));
        OnPropertyChanged(nameof(BlankPresetHint));
        OnPropertyChanged(nameof(VvencText));
        OnPropertyChanged(nameof(VvencHintText));
        FinishButtons.B2_1Text = CancelButtonText;
        FinishButtons.B2_2Text = ConfirmButtonText;
    }

    private void RefreshDropdownTitles()
    {
        void SyncTitles(IReadOnlyList<EncoderPresetItem> presets, DropdownMenuVM dropdown)
        {
            foreach (DropdownItemM item in dropdown.Items)
            {
                if (item.Tag is not int key) continue;

                if (key == -1)
                {
                    item.Title = BlankPresetText;
                    continue;
                }

                EncoderPresetItem? preset = presets.FirstOrDefault(p => p.Key == key);
                if (preset != null)
                    item.Title = Lang[preset.NameKey];
            }
        }
        SyncTitles(EncoderPresetsM.X264Presets, X264ModeDropdown);
        SyncTitles(EncoderPresetsM.X265Presets, X265ModeDropdown);
        SyncTitles(EncoderPresetsM.SvtAv1Presets, SvtAv1ModeDropdown);
    }
    #endregion

    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        PreviewVM.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
