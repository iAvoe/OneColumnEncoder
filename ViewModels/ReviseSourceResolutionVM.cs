using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System.Globalization;

namespace OneColumnEncoder.ViewModels
{
    public class ReviseSourceResolutionVM : BaseVM
    {
        private const int MaxResolutionDimension = 65535;
        private readonly ModalNavS _modalNavS;
        private readonly Action<bool> _finishAction;
        private readonly Func<int, int, string?> _reviseResolution;
        private readonly int _currentWidth;
        private readonly int _currentHeight;
        private readonly int _suggestedWidth;
        private readonly int _suggestedHeight;

        public string WindowTitle => UILangProviderM.Current["ReviseSourceResolution.Title"];
        public string Description => UILangProviderM.Current["ReviseSourceResolution.Description"];
        public string SettingsHeader => UILangProviderM.Current["ReviseSourceResolution.SettingsHeader"];
        public string WidthLabel => UILangProviderM.Current["ReviseSourceResolution.WidthLabel"];
        public string HeightLabel => UILangProviderM.Current["ReviseSourceResolution.HeightLabel"];
        public string CurrentResolutionLabel => UILangProviderM.Current["ReviseSourceResolution.CurrentLabel"];
        public string SuggestedResolutionLabel => UILangProviderM.Current["ReviseSourceResolution.SuggestedLabel"];
        public string CurrentResolutionText => FormatResolution(_currentWidth, _currentHeight);
        public string SuggestedResolutionText => FormatResolution(_suggestedWidth, _suggestedHeight);

        private string _resolutionWidthText;
        public string ResolutionWidthText
        {
            get => _resolutionWidthText;
            set => SetProperty(ref _resolutionWidthText, value);
        }

        private string _resolutionHeightText;
        public string ResolutionHeightText
        {
            get => _resolutionHeightText;
            set => SetProperty(ref _resolutionHeightText, value);
        }

        public int ResolutionWidth { get; private set; }
        public int ResolutionHeight { get; private set; }
        public ButtonGroupVM FinishButtons { get; private set; }

        public ReviseSourceResolutionVM(
            ModalNavS modalNavS,
            Action closeAction,
            Action<bool> finishAction,
            Func<int, int, string?> reviseResolution,
            int currentWidth,
            int currentHeight,
            int suggestedWidth,
            int suggestedHeight)
        {
            _modalNavS = modalNavS;
            _finishAction = finishAction;
            _reviseResolution = reviseResolution;
            _currentWidth = currentWidth;
            _currentHeight = currentHeight;
            _suggestedWidth = suggestedWidth;
            _suggestedHeight = suggestedHeight;

            ResolutionWidth = suggestedWidth;
            ResolutionHeight = suggestedHeight;
            _resolutionWidthText = suggestedWidth > 0 ? suggestedWidth.ToString(CultureInfo.InvariantCulture) : string.Empty;
            _resolutionHeightText = suggestedHeight > 0 ? suggestedHeight.ToString(CultureInfo.InvariantCulture) : string.Empty;

            FinishButtons = ButtonGroupVM.CreateTwoButton(
                UILangProviderM.Current["ReviseSourceResolution.Cancel"],
                UILangProviderM.Current["ReviseSourceResolution.Confirm"],
                new CloseModalCmd(closeAction),
                new ActionCmd(_ => Confirm()));

            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        private void Confirm()
        {
            if (!TryParseResolution(out int width, out int height))
            {
                ShowError(UILangProviderM.Current["ReviseSourceResolution.InvalidInput"]);
                return;
            }

            string? error = _reviseResolution(width, height);
            if (!string.IsNullOrWhiteSpace(error))
            {
                ShowError(error);
                return;
            }

            ResolutionWidth = width;
            ResolutionHeight = height;
            _finishAction(true);
        }

        private bool TryParseResolution(out int width, out int height)
        {
            bool hasWidth = TryParseDimension(ResolutionWidthText, out width);
            bool hasHeight = TryParseDimension(ResolutionHeightText, out height);
            return hasWidth && hasHeight;
        }

        private static bool TryParseDimension(string? text, out int value)
        {
            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value)
                && value > 0
                && value <= MaxResolutionDimension;
        }

        private void ShowError(string message)
        {
            new OpenErrModalCmd(_modalNavS, WindowTitle, message).Execute(null);
        }

        private static string FormatResolution(int width, int height) =>
            width > 0 && height > 0
                ? string.Format(UILangProviderM.Current["ReviseSourceResolution.ResolutionFormat"], width, height)
                : UILangProviderM.Current["ReviseSourceResolution.UnknownResolution"];

        private void OnLanguageChanged()
        {
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(SettingsHeader));
            OnPropertyChanged(nameof(WidthLabel));
            OnPropertyChanged(nameof(HeightLabel));
            OnPropertyChanged(nameof(CurrentResolutionLabel));
            OnPropertyChanged(nameof(SuggestedResolutionLabel));
            OnPropertyChanged(nameof(CurrentResolutionText));
            OnPropertyChanged(nameof(SuggestedResolutionText));

            FinishButtons.B2_1Text = UILangProviderM.Current["ReviseSourceResolution.Cancel"];
            FinishButtons.B2_2Text = UILangProviderM.Current["ReviseSourceResolution.Confirm"];
        }

        public override void Dispose()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            base.Dispose();
        }
    }
}
