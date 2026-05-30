using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels.Cards;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OneColumnEncoder.ViewModels
{
    public class EncoderConfVM : BaseVM
    {
        private EncoderConfLangProviderM _lang =
            new(UILangProviderM.Current.LanguageCode);
        public EncoderConfLangProviderM Lang
        {
            get => _lang;
            private set => SetProperty(ref _lang, value);
        }
        private readonly EncoderConfM _model;
        private readonly ToolItemCardVM? _targetItem;
        public CloseModalCmd CloseCmd { get; }
        public ActionCmd ConfirmCmd { get; }
        public ButtonGroupVM FinishButtons { get; }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        public string WindowTitle => "1cenc Encoding Settings";
        public string TitleText => WindowTitle;
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

        private bool IsAbrTabSelected => SelectedTabIndex == 1;

        public DropdownMenuVM X264ModeDropdown { get; } = new();
        public DropdownMenuVM X265ModeDropdown { get; } = new();
        public DropdownMenuVM SvtAv1ModeDropdown { get; } = new();

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
        public int SvtAv1Crf {
            get => _svtAv1Crf;
            set => SetProperty(ref _svtAv1Crf, value);
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
        public bool X265Dark {
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

        public EncoderConfVM(Action closeAction, ToolItemCardVM? targetItem)
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
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        private void PopulateDropdowns()
        {
            foreach (var preset in EncoderPresetsM.X264Presets)
                X264ModeDropdown.Items.Add(new DropdownItemM(Lang[preset.NameKey]) { Tag = preset.Key });
            foreach (var preset in EncoderPresetsM.X265Presets)
                X265ModeDropdown.Items.Add(new DropdownItemM(Lang[preset.NameKey]) { Tag = preset.Key });
            foreach (var preset in EncoderPresetsM.SvtAv1Presets)
                SvtAv1ModeDropdown.Items.Add(new DropdownItemM(Lang[preset.NameKey]) { Tag = preset.Key });

            SelectDropdownByKey(X264ModeDropdown, _model.X264Mode);
            SelectDropdownByKey(X265ModeDropdown, _model.X265Mode);
            SelectDropdownByKey(SvtAv1ModeDropdown, _model.SvtAv1Mode);
        }

        private static void SelectDropdownByKey(DropdownMenuVM dropdown, string key)
        {
            var item = dropdown.Items.FirstOrDefault(i => i.Tag as string == key);
            if (item != null)
                dropdown.SelectedItem = item;
        }

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
        }

        private void SaveModel()
        {
            _model.EncoderModeTabIndex = SelectedTabIndex;
            _model.RateControlMode = IsAbrTabSelected ? "ABR" : "CRF";
            _model.X264Crf = X264Crf;
            _model.X265Crf = X265Crf;
            _model.SvtAv1Crf = SvtAv1Crf;
            _model.X264Abr = X264Abr;
            _model.X265Abr = X265Abr;
            _model.SvtAv1Abr = SvtAv1Abr;
            _model.X264Keyframe = X264Keyframe;
            _model.X265Keyframe = X265Keyframe;
            _model.SvtAv1Keyframe = SvtAv1Keyframe;
            _model.X264Mod = X264Mod;
            _model.X265Aq = X265Aq;
            _model.X265Dark = X265Dark;
            _model.X265Texture = X265Texture;
            _model.SvtAv1Dl2 = SvtAv1Dl2;
            _model.SvtAv1AutoTile = SvtAv1AutoTile;
            _model.X264Mode = (X264ModeDropdown.SelectedItem?.Tag as string) ?? "a";
            _model.X265Mode = (X265ModeDropdown.SelectedItem?.Tag as string) ?? "a";
            _model.SvtAv1Mode = (SvtAv1ModeDropdown.SelectedItem?.Tag as string) ?? "a";
            _model.CustomParams = BuildCustomParams();
            _model.Save();
        }

        private string BuildCustomParams()
        {
            var parts = new List<string>();
            foreach (var def in EncoderPresetsM.ThirdPartyParams)
            {
                var prop = GetType().GetProperty(def.PropertyName);
                if (prop != null && prop.GetValue(this) is true)
                    parts.Add(def.ParamOn);
                else if (prop != null && !string.IsNullOrEmpty(def.ParamOff))
                    parts.Add(def.ParamOff);
            }
            return string.Join(" ", parts.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        private void ApplySettingsToTarget()
        {
            if (_targetItem is null) return;
            _targetItem.SetEncodingSummary(BuildPrimarySummary(_model), BuildSecondarySummary(_model));
        }

        public static void ApplySavedSettingsToCard(ToolItemCardVM targetItem)
        {
            var model = EncoderConfM.Load();
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

        private void OnLanguageChanged()
        {
            Lang = new EncoderConfLangProviderM(UILangProviderM.Current.LanguageCode);
            RefreshDropdownTitles();
            OnPropertyChanged(nameof(WindowTitle));
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
            FinishButtons.B2_1Text = CancelButtonText;
            FinishButtons.B2_2Text = ConfirmButtonText;
        }

        private void RefreshDropdownTitles()
        {
            void SyncTitles(IReadOnlyList<EncoderPresetItem> presets, DropdownMenuVM dropdown)
            {
                for (int i = 0; i < presets.Count && i < dropdown.Items.Count; i++)
                    dropdown.Items[i].Title = Lang[presets[i].NameKey];
            }
            SyncTitles(EncoderPresetsM.X264Presets, X264ModeDropdown);
            SyncTitles(EncoderPresetsM.X265Presets, X265ModeDropdown);
            SyncTitles(EncoderPresetsM.SvtAv1Presets, SvtAv1ModeDropdown);
        }

        public override void Dispose()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            base.Dispose();
        }
    }
}
