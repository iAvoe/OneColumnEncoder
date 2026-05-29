using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels.Cards;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class EncoderConfVM : BaseVM
    {
        private EncoderConfLangProviderM _lang = new(UILangProviderM.Current.LanguageCode);
        public EncoderConfLangProviderM Lang
        {
            get => _lang;
            private set => SetProperty(ref _lang, value);
        }

        private readonly EncoderConfM _model;
        private readonly ToolItemCardVM? _rateControlItem;
        private readonly ToolItemCardVM? _baseParamsItem;
        private readonly ToolItemCardVM? _customParamsItem;

        public CloseModalCmd CloseCmd { get; }
        public ICommand ConfirmCmd { get; }
        public ButtonGroupVM FinishButtons { get; }

        public DropdownMenuVM RateControlModeDropdown { get; } = new();
        public DropdownMenuVM PresetDropdown { get; } = new();
        public DropdownMenuVM TuneDropdown { get; } = new();
        public DropdownMenuVM ProfileDropdown { get; } = new();

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        private int _crfValue = 23;
        public int CrfValue
        {
            get => _crfValue;
            set => SetProperty(ref _crfValue, value);
        }

        private int _targetBitrate = 2000;
        public int TargetBitrate
        {
            get => _targetBitrate;
            set => SetProperty(ref _targetBitrate, value);
        }

        private int _keyframeInterval = 250;
        public int KeyframeInterval
        {
            get => _keyframeInterval;
            set => SetProperty(ref _keyframeInterval, value);
        }

        private bool _fastDecode;
        public bool FastDecode
        {
            get => _fastDecode;
            set => SetProperty(ref _fastDecode, value);
        }

        private bool _zeroLatency;
        public bool ZeroLatency
        {
            get => _zeroLatency;
            set => SetProperty(ref _zeroLatency, value);
        }

        private string _customParams = "";
        public string CustomParams
        {
            get => _customParams;
            set => SetProperty(ref _customParams, value);
        }

        public string WindowTitle => Lang.WindowTitle;
        public string TabRateControl => Lang.TabRateControl;
        public string TabAdvanced => Lang.TabAdvanced;
        public string RateControlModeText => Lang.RateControlModeText;
        public string CrfSliderLabel => Lang.CrfSliderLabel;
        public string TargetBitrateText => Lang.TargetBitrateText;
        public string PresetText => Lang.PresetText;
        public string TuneText => Lang.TuneText;
        public string ProfileText => Lang.ProfileText;
        public string KeyframeIntervalText => Lang.KeyframeIntervalText;
        public string FastDecodeText => Lang.FastDecodeText;
        public string ZeroLatencyText => Lang.ZeroLatencyText;
        public string CustomParamsText => Lang.CustomParamsText;
        public string CustomParamsHint => Lang.CustomParamsHint;
        public string CancelButtonText => Lang.CancelButtonText;
        public string ConfirmButtonText => Lang.ConfirmButtonText;
        public IEnumerable<string> CrfTickLabels => BuildCrfTickLabels();
        public IEnumerable<string> KeyframeTickLabels => BuildKeyframeTickLabels();

        public EncoderConfVM(Action closeAction, ToolItemCardVM? rateControlItem = null,
            ToolItemCardVM? baseParamsItem = null, ToolItemCardVM? customParamsItem = null,
            int initialTab = 0)
        {
            _model = EncoderConfM.Load();
            _rateControlItem = rateControlItem;
            _baseParamsItem = baseParamsItem;
            _customParamsItem = customParamsItem;
            Lang = new EncoderConfLangProviderM(UILangProviderM.Current.LanguageCode);
            CloseCmd = new CloseModalCmd(closeAction);
            ConfirmCmd = new ActionCmd(_ =>
            {
                ApplySettingsToTargets();
                SaveModel();
                closeAction();
            });
            FinishButtons = ButtonGroupVM.CreateTwoButton(CancelButtonText, ConfirmButtonText, CloseCmd, ConfirmCmd);

            PopulateDropdowns();
            LoadModelToUi();

            if (initialTab is 0 or 1) SelectedTabIndex = initialTab;

            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        private void PopulateDropdowns()
        {
            PopulateRateControlModes();
            PopulatePresets();
            PopulateTunes();
            PopulateProfiles();
        }

        private void PopulateRateControlModes()
        {
            RateControlModeDropdown.Items.Add(new DropdownItemM(Lang.ModeCrf));
            RateControlModeDropdown.Items.Add(new DropdownItemM(Lang.ModeCbr));
            RateControlModeDropdown.Items.Add(new DropdownItemM(Lang.ModeVbr));
            RateControlModeDropdown.SelectedItem = RateControlModeDropdown.Items[0];
            RateControlModeDropdown.SelectionChangedCommand = new ActionCmd(_ =>
                OnPropertyChanged(nameof(IsCrfMode)));
        }

        private void PopulatePresets()
        {
            string[] presetKeys =
                ["placebo", "veryslow", "slower", "slow", "medium", "fast", "faster", "veryfast", "superfast", "ultrafast"];
            foreach (string key in presetKeys)
                PresetDropdown.Items.Add(new DropdownItemM(Lang[$"Preset{Capitalize(key)}"]));
            PresetDropdown.SelectedItem = PresetDropdown.Items.First(i => i.Title == Lang.PresetMedium);
        }

        private void PopulateTunes()
        {
            string[] tuneKeys = ["none", "film", "animation", "grain", "stillimage", "psnr", "ssim", "fastdecode", "zerolatency"];
            foreach (string key in tuneKeys)
                TuneDropdown.Items.Add(new DropdownItemM(Lang[$"Tune{Capitalize(key)}"]));
            TuneDropdown.SelectedItem = TuneDropdown.Items[0];
        }

        private void PopulateProfiles()
        {
            string[] profileKeys = ["auto", "main", "high", "high10", "high444"];
            foreach (string key in profileKeys)
                ProfileDropdown.Items.Add(new DropdownItemM(Lang[$"Profile{Capitalize(key)}"]));
            ProfileDropdown.SelectedItem = ProfileDropdown.Items[0];
        }

        public bool IsCrfMode => RateControlModeDropdown.SelectedItem?.Title == Lang.ModeCrf;

        private void ApplySettingsToTargets()
        {
            if (_rateControlItem != null)
            {
                _rateControlItem.PrimaryValueText = RateControlModeDropdown.SelectedItem?.Title ?? Lang.ModeCrf;
                _rateControlItem.Path = IsCrfMode ? $"CRF: {CrfValue}" : $"{TargetBitrate} kbps";
            }
            if (_baseParamsItem != null)
            {
                string preset = PresetDropdown.SelectedItem?.Title ?? Lang.PresetMedium;
                string tune = TuneDropdown.SelectedItem?.Title ?? Lang.TuneNone;
                _baseParamsItem.PrimaryValueText = $"{preset}, {tune}";
                _baseParamsItem.Path = $"Keyframes: {KeyframeInterval}";
            }
            if (_customParamsItem != null)
            {
                _customParamsItem.PrimaryValueText = ProfileDropdown.SelectedItem?.Title ?? Lang.ProfileAuto;
                _customParamsItem.Path = string.IsNullOrWhiteSpace(CustomParams) ? "-" : CustomParams;
            }
        }

        private void SaveModel()
        {
            _model.RateControlMode = RateControlModeDropdown.SelectedItem?.Title ?? Lang.ModeCrf;
            _model.CrfValue = CrfValue;
            _model.TargetBitrate = TargetBitrate;
            _model.Preset = PresetDropdown.SelectedItem?.Title ?? Lang.PresetMedium;
            _model.Tune = TuneDropdown.SelectedItem?.Title ?? Lang.TuneNone;
            _model.Profile = ProfileDropdown.SelectedItem?.Title ?? Lang.ProfileAuto;
            _model.KeyframeInterval = KeyframeInterval;
            _model.FastDecode = FastDecode;
            _model.ZeroLatency = ZeroLatency;
            _model.CustomParams = CustomParams;
            _model.Save();
        }

        private void LoadModelToUi()
        {
            CrfValue = _model.CrfValue;
            TargetBitrate = _model.TargetBitrate;
            KeyframeInterval = _model.KeyframeInterval;
            FastDecode = _model.FastDecode;
            ZeroLatency = _model.ZeroLatency;
            CustomParams = _model.CustomParams;

            DropdownItemM? modeItem = RateControlModeDropdown.Items.FirstOrDefault(i =>
                i.Title == LookupModeDisplay(_model.RateControlMode));
            if (modeItem != null) RateControlModeDropdown.SelectedItem = modeItem;

            DropdownItemM? presetItem = PresetDropdown.Items.FirstOrDefault(i =>
                i.Title == LookupPresetDisplay(_model.Preset));
            if (presetItem != null) PresetDropdown.SelectedItem = presetItem;

            DropdownItemM? tuneItem = TuneDropdown.Items.FirstOrDefault(i =>
                i.Title == LookupTuneDisplay(_model.Tune));
            if (tuneItem != null) TuneDropdown.SelectedItem = tuneItem;

            DropdownItemM? profileItem = ProfileDropdown.Items.FirstOrDefault(i =>
                i.Title == LookupProfileDisplay(_model.Profile));
            if (profileItem != null) ProfileDropdown.SelectedItem = profileItem;
        }

        private string LookupModeDisplay(string key) => key switch
        {
            "CRF" => Lang.ModeCrf,
            "CBR" => Lang.ModeCbr,
            "VBR" => Lang.ModeVbr,
            _ => Lang.ModeCrf
        };

        private string LookupPresetDisplay(string key) => key switch
        {
            "placebo" => Lang.PresetPlacebo,
            "veryslow" => Lang.PresetVerySlow,
            "slower" => Lang.PresetSlower,
            "slow" => Lang.PresetSlow,
            "medium" => Lang.PresetMedium,
            "fast" => Lang.PresetFast,
            "faster" => Lang.PresetFaster,
            "veryfast" => Lang.PresetVeryFast,
            "superfast" => Lang.PresetSuperFast,
            "ultrafast" => Lang.PresetUltraFast,
            _ => Lang.PresetMedium
        };

        private string LookupTuneDisplay(string key) => key switch
        {
            "none" => Lang.TuneNone,
            "film" => Lang.TuneFilm,
            "animation" => Lang.TuneAnimation,
            "grain" => Lang.TuneGrain,
            "stillimage" => Lang.TuneStillImage,
            "psnr" => Lang.TunePsnr,
            "ssim" => Lang.TuneSsim,
            "fastdecode" => Lang.TuneFastDecode,
            "zerolatency" => Lang.TuneZeroLatency,
            _ => Lang.TuneNone
        };

        private string LookupProfileDisplay(string key) => key switch
        {
            "auto" => Lang.ProfileAuto,
            "main" => Lang.ProfileMain,
            "high" => Lang.ProfileHigh,
            "high10" => Lang.ProfileHigh10,
            "high444" => Lang.ProfileHigh444,
            _ => Lang.ProfileAuto
        };

        private static IEnumerable<string> BuildCrfTickLabels()
        {
            return Enumerable.Range(0, 10).Select(i =>
            {
                int val = i switch
                {
                    0 => 0,
                    9 => 51,
                    _ => (int)Math.Round(Math.Pow(51.0, i / 9.0))
                };
                return val.ToString();
            });
        }

        private static IEnumerable<string> BuildKeyframeTickLabels()
        {
            return Enumerable.Range(0, 8).Select(i =>
            {
                int val = (int)(10 + i * (1000.0 - 10) / 7);
                return val >= 1000 ? "1000+" : val.ToString();
            });
        }

        private static string Capitalize(string s) =>
            string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];

        private void OnLanguageChanged()
        {
            Lang = new EncoderConfLangProviderM(UILangProviderM.Current.LanguageCode);
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(TabRateControl));
            OnPropertyChanged(nameof(TabAdvanced));
            OnPropertyChanged(nameof(RateControlModeText));
            OnPropertyChanged(nameof(CrfSliderLabel));
            OnPropertyChanged(nameof(TargetBitrateText));
            OnPropertyChanged(nameof(PresetText));
            OnPropertyChanged(nameof(TuneText));
            OnPropertyChanged(nameof(ProfileText));
            OnPropertyChanged(nameof(KeyframeIntervalText));
            OnPropertyChanged(nameof(FastDecodeText));
            OnPropertyChanged(nameof(ZeroLatencyText));
            OnPropertyChanged(nameof(CustomParamsText));
            OnPropertyChanged(nameof(CustomParamsHint));
            OnPropertyChanged(nameof(CancelButtonText));
            OnPropertyChanged(nameof(ConfirmButtonText));
            OnPropertyChanged(nameof(CrfTickLabels));
            OnPropertyChanged(nameof(KeyframeTickLabels));
            OnPropertyChanged(nameof(IsCrfMode));

            FinishButtons.B2_1Text = CancelButtonText;
            FinishButtons.B2_2Text = ConfirmButtonText;

            RebuildDropdownDisplay();
        }

        private void RebuildDropdownDisplay()
        {
            string currentMode = RateControlModeDropdown.SelectedItem?.Title ?? "";
            string currentPreset = PresetDropdown.SelectedItem?.Title ?? "";
            string currentTune = TuneDropdown.SelectedItem?.Title ?? "";
            string currentProfile = ProfileDropdown.SelectedItem?.Title ?? "";

            RateControlModeDropdown.Items.Clear();
            PresetDropdown.Items.Clear();
            TuneDropdown.Items.Clear();
            ProfileDropdown.Items.Clear();

            PopulateDropdowns();

            RateControlModeDropdown.SelectedItem = RateControlModeDropdown.Items.FirstOrDefault(i => i.Title == currentMode)
                ?? RateControlModeDropdown.Items[0];
            PresetDropdown.SelectedItem = PresetDropdown.Items.FirstOrDefault(i => i.Title == currentPreset)
                ?? PresetDropdown.Items[0];
            TuneDropdown.SelectedItem = TuneDropdown.Items.FirstOrDefault(i => i.Title == currentTune)
                ?? TuneDropdown.Items[0];
            ProfileDropdown.SelectedItem = ProfileDropdown.Items.FirstOrDefault(i => i.Title == currentProfile)
                ?? ProfileDropdown.Items[0];
        }

        public override void Dispose()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            base.Dispose();
        }
    }
}
