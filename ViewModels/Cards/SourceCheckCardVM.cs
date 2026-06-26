using OneColumnEncoder.FFmpeg;
using OneColumnEncoder.Models;

namespace OneColumnEncoder.ViewModels.Cards
{
    public class SourceCheckCardVM : ValidationCardBaseVM
    {
        private bool _isBypassed;
        public bool IsBypassed
        {
            get => _isBypassed;
            private set => SetProperty(ref _isBypassed, value);
        }

        public Func<bool>? IsSvtav1SelectedFunc { get; set; }

        private string? _lastAnalysisJson;

        protected const int MetadataChecklistIdx = 0;
        protected const int ProgressiveChecklistIdx = 1;
        protected const int Svtav1BitDepthChecklistIdx = 2;
        protected const int MaxBitDepthChecklistIdx = 3;

        public SourceCheckCardVM()
        {
            FillCollection(Checklist1, ChecklistProviderM.GetSourceChecklist1());
            FillCollection(Checklist2, ChecklistProviderM.GetSourceChecklist2());
        }

        #region TwoButtonGroup commands
        public void SetBypassed(bool isBypassed)
        {
            IsBypassed = isBypassed;
            CardOpacity = isBypassed ? 0.45 : 1.0;
        }
        public void ResetAnalysisStatus()
        {
            _lastAnalysisJson = null;
            SetBypassed(false);

            for (int i = 0; i < Checklist1.Count; i++)
                SetChecklist1(i, StatusType.Waiting);

            for (int i = 0; i < Checklist2.Count; i++)
                SetChecklist2(i, StatusType.Waiting);
        }
        #endregion

        #region FFprobe Analysis

        public void ApplyFfprobeAnalysisJson(string rawJson)
        {
            _lastAnalysisJson = rawJson;
            try
            {
                FFProbeSourceValidationResult result = FFProbeSourceValidation.Analyze(rawJson);

                SetChecklist1(MetadataChecklistIdx, StatusType.Success);
                SetChecklist1(ProgressiveChecklistIdx, result.IsProgressive
                    ? StatusType.Success : StatusType.Error);
                SetChecklist1(Svtav1BitDepthChecklistIdx, result.IsSvtAv1BitDepthSupported
                    ? StatusType.Success
                    : IsSelectingSvtav1()
                        ? StatusType.Error
                        : StatusType.Warning);
                SetChecklist1(MaxBitDepthChecklistIdx, result.IsMaxBitDepthSupported
                    ? StatusType.Success : StatusType.Error);
                SetChecklist2(0, result.HasConstantFrameRate
                    ? StatusType.Success : StatusType.Warning);
                SetChecklist2(1, result.HasSquarePixels
                    ? StatusType.Success : StatusType.Warning);
                SetChecklist2(2, result.HasColorSpace
                    ? StatusType.Success : StatusType.Warning);
                SetChecklist2(3, result.HasColorTransfer
                    ? StatusType.Success : StatusType.Warning);
                SetChecklist2(4, result.HasColorPrimaries
                    ? StatusType.Success : StatusType.Warning);
                SetChecklist2(5, result.HasSupportedChroma
                    ? StatusType.Success : StatusType.Warning);
            }
            catch
            {
                SetAnalysisFailedStatus();
                throw;
            }
        }

        private bool IsSelectingSvtav1()
        {
            return IsSvtav1SelectedFunc?.Invoke() ?? false;
        }

        public void RefreshSvtav1BitDepthStatus()
        {
            if (_lastAnalysisJson == null) return;

            try
            {
                SetChecklist1(Svtav1BitDepthChecklistIdx, FFProbeSourceValidation.IsSvtAv1BitDepthSupported(_lastAnalysisJson)
                    ? StatusType.Success
                    : IsSelectingSvtav1()
                        ? StatusType.Error
                        : StatusType.Warning);
            }
            catch
            {
                // Leave current status as-is
            }
        }

        public void SetAnalysisFailedStatus()
        {
            _lastAnalysisJson = null;
            SetChecklist1(MetadataChecklistIdx, StatusType.Error);
            SetChecklist1(ProgressiveChecklistIdx, StatusType.Waiting);
            SetChecklist1(Svtav1BitDepthChecklistIdx, StatusType.Waiting);

            for (int i = 0; i < Checklist2.Count; i++) SetChecklist2(i, StatusType.Waiting);
        }

        public SourceCheckSignature GetSignature() =>
            new(
                [.. Checklist1.Select(entry => entry.Status)],
                [.. Checklist2.Select(entry => entry.Status)]);

        #endregion

        public void RefreshLanguage()
        {
            RefreshChecklist(Checklist1, ChecklistProviderM.GetSourceChecklist1());
            RefreshChecklist(Checklist2, ChecklistProviderM.GetSourceChecklist2());
        }

        #region Issue Formatting (for ConfirmationModal)

        public string SevereIssuesFormatted => FormatIssues(StatusType.Error);
        public string ModerateIssuesFormatted => FormatIssues(StatusType.Warning);

        private string FormatIssues(StatusType status)
        {
            var checklist1Issues = Checklist1
                .Select((entry, index) => (entry, description: GetChecklist1IssueDescription(index)))
                .Where(e => e.entry.IsEnabled && e.entry.Status == status)
                .Select(e => FormatIssue(e.entry.Text, e.description));

            var checklist2Issues = Checklist2
                .Select((entry, index) => (entry, description: GetChecklist2IssueDescription(index)))
                .Where(e => e.entry.IsEnabled && e.entry.Status == status)
                .Select(e => FormatIssue(e.entry.Text, e.description));

            return string.Join(
                Environment.NewLine + Environment.NewLine, checklist1Issues.Concat(checklist2Issues));
        }

        private static string FormatIssue(string fallbackText, string? description)
        {
            if (string.IsNullOrWhiteSpace(description)) return fallbackText;
            return description;
        }

        private static string? GetChecklist1IssueDescription(int index) => index switch
        {
            MetadataChecklistIdx => UICaptionProviderM.SourceInspect.MetadataP1Text,
            ProgressiveChecklistIdx => UICaptionProviderM.SourceInspect.ProgressiveP1Text,
            Svtav1BitDepthChecklistIdx => UICaptionProviderM.SourceInspect.BitDepthP1Text,
            _ => null,
        };

        private static string? GetChecklist2IssueDescription(int index) => index switch
        {
            0 => UICaptionProviderM.SourceInspect.FramerateP1Text,
            1 => UICaptionProviderM.SourceInspect.AspectRatioP1Text,
            2 => UICaptionProviderM.SourceInspect.ColorMatrixP1Text,
            3 => UICaptionProviderM.SourceInspect.TransferCharsP1Text,
            4 => UICaptionProviderM.SourceInspect.ColorPrimariesP1Text,
            5 => UICaptionProviderM.SourceInspect.ChromaSubsamplingP1Text,
            _ => null,
        };

        #endregion

        #region Private Checklist Setters

        protected void SetChecklist1(int index, StatusType status)
        {
            if (index >= 0 && index < Checklist1.Count)
                Checklist1[index].Status = status;
        }

        protected void SetChecklist2(int index, StatusType status)
        {
            if (index >= 0 && index < Checklist2.Count)
                Checklist2[index].Status = status;
        }

        #endregion

    }

    public sealed record SourceCheckSignature(StatusType[] Checklist1, StatusType[] Checklist2)
    {
        public bool Matches(SourceCheckSignature other) =>
            Checklist1.SequenceEqual(other.Checklist1) && Checklist2.SequenceEqual(other.Checklist2);
    }
}
