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

        private const int SourcePickedChecklistIdx = 0; 
        private const int MetadataChecklistIdx = 1;
        private const int ProgressiveChecklistIdx = 2;
        private const int BitDepthChecklistIdx = 3;

        public SourceCheckCardVM()
        {
            FillCollection(Checklist1, ChecklistProviderM.GetSourceChecklist1());
            FillCollection(Checklist2, ChecklistProviderM.GetSourceChecklist2());
        }

        public void SetSourcePickedStatus(bool isPicked)
        {
            if (SourcePickedChecklistIdx >= 0 && SourcePickedChecklistIdx < Checklist1.Count)
                Checklist1[SourcePickedChecklistIdx].Status = isPicked ? StatusType.Success : StatusType.Error;
        }

        public void SetBypassed(bool isBypassed)
        {
            IsBypassed = isBypassed;
            CardOpacity = isBypassed ? 0.45 : 1.0;
        }

        public void ResetAnalysisStatus(bool isSourcePicked)
        {
            SetBypassed(false);
            SetSourcePickedStatus(isSourcePicked);

            for (int i = 1; i < Checklist1.Count; i++)
                SetChecklist1(i, StatusType.Waiting);

            for (int i = 0; i < Checklist2.Count; i++)
                SetChecklist2(i, StatusType.Waiting);
        }

        public void ApplyFfprobeAnalysisJson(string rawJson)
        {
            try
            {
                using JsonDocument document = JsonDocument.Parse(rawJson);
                JsonElement stream = document.RootElement.GetProperty("streams")[0];

                SetChecklist1(MetadataChecklistIdx, StatusType.Success);
                SetChecklist1(ProgressiveChecklistIdx, IsProgressive(stream) ? StatusType.Success : StatusType.Error);
                SetChecklist1(BitDepthChecklistIdx, IsSupportedBitDepth(stream) ? StatusType.Success : StatusType.Error);

                SetChecklist2(0, HasConstantFrameRate(stream) ? StatusType.Success : StatusType.Warning);
                SetChecklist2(1, HasSquarePixels(stream) ? StatusType.Success : StatusType.Warning);
                SetChecklist2(2, HasKnownMetadata(stream, "color_space") ? StatusType.Success : StatusType.Warning);
                SetChecklist2(3, HasKnownMetadata(stream, "color_transfer") ? StatusType.Success : StatusType.Warning);
                SetChecklist2(4, HasKnownMetadata(stream, "color_primaries") ? StatusType.Success : StatusType.Warning);
                SetChecklist2(5, HasSupportedChroma(stream) ? StatusType.Success : StatusType.Warning);
            }
            catch
            {
                SetAnalysisFailedStatus();
                throw;
            }
        }

        public void SetAnalysisFailedStatus()
        {
            SetChecklist1(MetadataChecklistIdx, StatusType.Error);
            SetChecklist1(ProgressiveChecklistIdx, StatusType.Waiting);
            SetChecklist1(BitDepthChecklistIdx, StatusType.Waiting);

            for (int i = 0; i < Checklist2.Count; i++)
                SetChecklist2(i, StatusType.Waiting);
        }

        public void RefreshLanguage()
        {
            RefreshChecklist(Checklist1, ChecklistProviderM.GetSourceChecklist1());
            RefreshChecklist(Checklist2, ChecklistProviderM.GetSourceChecklist2());
        }

        private void SetChecklist1(int index, StatusType status)
        {
            if (index >= 0 && index < Checklist1.Count)
                Checklist1[index].Status = status;
        }

        private void SetChecklist2(int index, StatusType status)
        {
            if (index >= 0 && index < Checklist2.Count)
                Checklist2[index].Status = status;
        }

        private static bool IsProgressive(JsonElement stream)
        {
            string? fieldOrder = TryGetString(stream, "field_order");
            return string.IsNullOrWhiteSpace(fieldOrder)
                || fieldOrder.Equals("progressive", StringComparison.OrdinalIgnoreCase)
                || fieldOrder.Equals("unknown", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSupportedBitDepth(JsonElement stream)
        {
            int bitDepth = GetBitDepth(stream);
            return bitDepth is 8 or 9 or 10;
        }

        private static int GetBitDepth(JsonElement stream)
        {
            if (TryGetInt(stream, "bits_per_raw_sample", out int rawBits)) return rawBits;
            if (TryGetInt(stream, "bits_per_sample", out int sampleBits)) return sampleBits;

            string pixFmt = TryGetString(stream, "pix_fmt") ?? string.Empty;
            if (pixFmt.Contains("10", StringComparison.OrdinalIgnoreCase)) return 10;
            if (pixFmt.Contains("12", StringComparison.OrdinalIgnoreCase)) return 12;
            if (pixFmt.Contains("14", StringComparison.OrdinalIgnoreCase)) return 14;
            if (pixFmt.Contains("16", StringComparison.OrdinalIgnoreCase)) return 16;
            return string.IsNullOrWhiteSpace(pixFmt) ? 0 : 8;
        }

        private static bool HasConstantFrameRate(JsonElement stream)
        {
            string? avg = TryGetString(stream, "avg_frame_rate");
            string? r = TryGetString(stream, "r_frame_rate");
            return !string.IsNullOrWhiteSpace(avg)
                && !avg.Equals("0/0", StringComparison.OrdinalIgnoreCase)
                && string.Equals(avg, r, StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasSquarePixels(JsonElement stream)
        {
            string? sar = TryGetString(stream, "sample_aspect_ratio");
            return string.Equals(sar, "1:1", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasKnownMetadata(JsonElement stream, string propertyName)
        {
            string? value = TryGetString(stream, propertyName);
            return !string.IsNullOrWhiteSpace(value)
                && !value.Equals("unknown", StringComparison.OrdinalIgnoreCase)
                && !value.Equals("unspecified", StringComparison.OrdinalIgnoreCase)
                && !value.Equals("reserved", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasSupportedChroma(JsonElement stream)
        {
            string pixFmt = TryGetString(stream, "pix_fmt") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(pixFmt)) return false;
            if (pixFmt.Contains("444", StringComparison.OrdinalIgnoreCase)
                || pixFmt.Contains("rgb", StringComparison.OrdinalIgnoreCase)
                || pixFmt.Contains("gbr", StringComparison.OrdinalIgnoreCase)
                || pixFmt.Contains("gray", StringComparison.OrdinalIgnoreCase))
                return true;

            string? chromaLocation = TryGetString(stream, "chroma_location");
            return pixFmt.Contains("yuv", StringComparison.OrdinalIgnoreCase)
                && (chromaLocation?.Equals("left", StringComparison.OrdinalIgnoreCase) == true
                    || chromaLocation?.Equals("topleft", StringComparison.OrdinalIgnoreCase) == true);
        }

        private static string? TryGetString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out JsonElement property)
                ? property.GetString()
                : null;
        }

        private static bool TryGetInt(JsonElement element, string propertyName, out int value)
        {
            value = 0;
            if (!element.TryGetProperty(propertyName, out JsonElement property)) return false;
            if (property.ValueKind == JsonValueKind.Number) return property.TryGetInt32(out value);
            return property.ValueKind == JsonValueKind.String
                && int.TryParse(property.GetString(), out value);
        }
    }
}
