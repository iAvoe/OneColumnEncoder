using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using System.Text.Json;

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
                using JsonDocument document = JsonDocument.Parse(rawJson);
                JsonElement stream = document.RootElement.GetProperty("streams")[0];

                SetChecklist1(MetadataChecklistIdx, StatusType.Success);
                SetChecklist1(ProgressiveChecklistIdx, IsProgressive(stream)
                    ? StatusType.Success : StatusType.Error);
                SetChecklist1(Svtav1BitDepthChecklistIdx, IsSupportedBitDepth(stream, 10)
                    ? StatusType.Success
                    : IsSelectingSvtav1()
                        ? StatusType.Error
                        : StatusType.Warning);
                SetChecklist1(MaxBitDepthChecklistIdx, IsSupportedBitDepth(stream, 12)
                    ? StatusType.Success : StatusType.Error);
                SetChecklist2(0, HasConstantFrameRate(stream)
                    ? StatusType.Success : StatusType.Warning);
                SetChecklist2(1, HasSquarePixels(stream)
                    ? StatusType.Success : StatusType.Warning);
                SetChecklist2(2, HasKnownMetadata(stream, "color_space")
                    ? StatusType.Success : StatusType.Warning);
                SetChecklist2(3, HasKnownMetadata(stream, "color_transfer")
                    ? StatusType.Success : StatusType.Warning);
                SetChecklist2(4, HasKnownMetadata(stream, "color_primaries")
                    ? StatusType.Success : StatusType.Warning);
                SetChecklist2(5, HasSupportedChroma(stream)
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
                using JsonDocument document = JsonDocument.Parse(_lastAnalysisJson);
                JsonElement stream = document.RootElement.GetProperty("streams")[0];

                SetChecklist1(Svtav1BitDepthChecklistIdx, IsSupportedBitDepth(stream, 10)
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

        #region Static Analysis Helpers

        private static bool IsProgressive(JsonElement stream)
        {
            string? fieldOrder = JsonElementHelper.TryGetString(stream, "field_order");
            return string.IsNullOrWhiteSpace(fieldOrder)
                || fieldOrder.Equals("progressive", StringComparison.OrdinalIgnoreCase)
                || fieldOrder.Equals("unknown", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSupportedBitDepth(JsonElement stream, int max = 12)
        {
            int bitDepth = GetBitDepth(stream);
            return bitDepth == 8 || bitDepth == 10 || bitDepth == max;
        }

        private static int GetBitDepth(JsonElement stream)
        {
            if (JsonElementHelper.TryGetInt(stream, "bits_per_raw_sample", out int rawBits)) return rawBits;
            if (JsonElementHelper.TryGetInt(stream, "bits_per_sample", out int sampleBits)) return sampleBits;

            string pixFmt = JsonElementHelper.TryGetString(stream, "pix_fmt") ?? string.Empty;
            if (pixFmt.Contains("10", StringComparison.OrdinalIgnoreCase)) return 10;
            if (pixFmt.Contains("12", StringComparison.OrdinalIgnoreCase)) return 12;
            if (pixFmt.Contains("14", StringComparison.OrdinalIgnoreCase)) return 14;
            if (pixFmt.Contains("16", StringComparison.OrdinalIgnoreCase)) return 16;
            return string.IsNullOrWhiteSpace(pixFmt) ? 0 : 8;
        }

        private static bool HasConstantFrameRate(JsonElement stream)
        {
            string? avg = JsonElementHelper.TryGetString(stream, "avg_frame_rate");
            string? r = JsonElementHelper.TryGetString(stream, "r_frame_rate");
            return !string.IsNullOrWhiteSpace(avg)
                && !avg.Equals("0/0", StringComparison.OrdinalIgnoreCase)
                && string.Equals(avg, r, StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasSquarePixels(JsonElement stream)
        {
            string? sar = JsonElementHelper.TryGetString(stream, "sample_aspect_ratio");
            return string.Equals(sar, "1:1", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasKnownMetadata(JsonElement stream, string propertyName)
        {
            string? value = JsonElementHelper.TryGetString(stream, propertyName);
            return !string.IsNullOrWhiteSpace(value)
                && !value.Equals("unknown", StringComparison.OrdinalIgnoreCase)
                && !value.Equals("unspecified", StringComparison.OrdinalIgnoreCase)
                && !value.Equals("reserved", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasSupportedChroma(JsonElement stream)
        {
            string pixFmt = JsonElementHelper.TryGetString(stream, "pix_fmt") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(pixFmt)) return false;
            if (pixFmt.Contains("444", StringComparison.OrdinalIgnoreCase)
                || pixFmt.Contains("rgb", StringComparison.OrdinalIgnoreCase)
                || pixFmt.Contains("gbr", StringComparison.OrdinalIgnoreCase)
                || pixFmt.Contains("gray", StringComparison.OrdinalIgnoreCase))
                return true;

            string? chromaLocation = JsonElementHelper.TryGetString(stream, "chroma_location");
            return pixFmt.Contains("yuv", StringComparison.OrdinalIgnoreCase)
                && (chromaLocation?.Equals("left", StringComparison.OrdinalIgnoreCase) == true
                    || chromaLocation?.Equals("topleft", StringComparison.OrdinalIgnoreCase) == true);
        }



        #endregion
    }

    public sealed record SourceCheckSignature(StatusType[] Checklist1, StatusType[] Checklist2)
    {
        public bool Matches(SourceCheckSignature other) =>
            Checklist1.SequenceEqual(other.Checklist1) && Checklist2.SequenceEqual(other.Checklist2);
    }
}
