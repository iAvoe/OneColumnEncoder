using System.IO;
using System.Text.Json.Serialization;

namespace OneColumnEncoder.Models.Encoding;

/// <summary>
/// Encoder configuration persisted across launches.
/// </summary>
public class EncoderConfM : SaveLoadBase<EncoderConfM>
{
    protected override string FilePath =>
        Path.Combine(GetConfigDirectory(), "encodingconfig.json");

    public int EncoderModeTabIndex { get; set; } = 0;
    public string RateControlMode { get; set; } = "CRF";

    // x264
    public int X264Crf { get; set; } = 23;
    public int X264Abr { get; set; } = 209;
    public int X264Keyframe { get; set; } = 9;
    [JsonConverter(typeof(LegacyPresetKeyIntConverter))]
    public int X264Mode { get; set; } = 0; // See EncoderPresetsM
    public bool X264Mod { get; set; } = false;

    // x265
    public int X265Crf { get; set; } = 28;
    public int X265Abr { get; set; } = 70;
    public int X265Keyframe { get; set; } = 7;
    [JsonConverter(typeof(LegacyPresetKeyIntConverter))]
    public int X265Mode { get; set; } = 0;
    public bool X265Aq { get; set; } = false;
    public bool X265Dark { get; set; } = false;
    public bool X265Texture { get; set; } = false;
    public bool X265Mcstf { get; set; } = false;

    // SVT-AV1
    public int SvtAv1Crf { get; set; } = 35;
    public int SvtAv1Abr { get; set; } = 10;
    public int SvtAv1Keyframe { get; set; } = 9;
    [JsonConverter(typeof(LegacyPresetKeyIntConverter))]
    public int SvtAv1Mode { get; set; } = 0;
    public bool SvtAv1Dl2 { get; set; } = false;
    public bool SvtAv1AutoTile { get; set; } = false;

    // Dolby Vision Compatibility
    public int DoviOnOff { get; set; } = 0; // 0=Disabled, 1=Always, 2=Source confirmed DV
    public int X264HrdMode { get; set; } = 0; // 0=Off, 1=100Gbps, 2=1Tbps, 3=Manual VBV
    public int X265HrdMode { get; set; } = 0; // 0=Off, 1=100Gbps, 2=1Tbps, 3=Manual VBV

    // VVenC (Preview only)
    public int VvencQp { get; set; } = 32;
    public int VvencMode { get; set; } = 0; // 0=medium(qpa), 1=slower(qpa), 2=slow(qpa)

    public string CustomParamsX264 { get; set; } = "";
    public string CustomParamsX265 { get; set; } = "";
    public string CustomParamsSvtAv1 { get; set; } = "";

    // Tuning values for ffprobe-derived automatic parameters.
    // Defaults mirror the constants that were previously hard-coded in EncodingPipeline.
    public int X264AutoLookaheadPercent { get; set; } = 180;
    public int X265AutoLookaheadPercent { get; set; } = 180;
    public int X265AutoMerange4K { get; set; } = 56;
    public int X265AutoMerange1440P { get; set; } = 52;
    public int X265AutoMerange1080P { get; set; } = 48;
    public int X265AutoMerange720P { get; set; } = 40;
    public int X265AutoMerangeSd { get; set; } = 36;
    public int X265AutoSubmeUnder25 { get; set; } = 3;
    public int X265AutoSubmeUnder49 { get; set; } = 4;
    public int X265AutoSubmeUnder61 { get; set; } = 5;
    public int X265AutoSubmeAtLeast61 { get; set; } = 6;

    public string PreviewCustomParamsX264 { get; set; } = "";
    public string PreviewCustomParamsX265 { get; set; } = "";
    public string PreviewCustomParamsSvtAv1 { get; set; } = "";

    private sealed class LegacyPresetKeyIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetInt32();

            if (reader.TokenType == JsonTokenType.String)
            {
                string? value = reader.GetString();
                if (int.TryParse(value, out int numericValue))
                    return numericValue;

                return value?.Trim().ToLowerInvariant() switch
                {
                    "a" => 0,
                    "b" => 1,
                    "c" => 2,
                    "d" => 3,
                    "e" => 4,
                    _ => 0,
                };
            }

            if (reader.TokenType == JsonTokenType.Null)
                return 0;

            throw new JsonException($"Unexpected token {reader.TokenType} for preset key.");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options) =>
            writer.WriteNumberValue(value);
    }
}
