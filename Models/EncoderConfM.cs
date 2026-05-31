using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using OneColumnEncoder.Helpers;

namespace OneColumnEncoder.Models
{
    public class EncoderConfM : SaveLoadBaseH<EncoderConfM>
    {
        private static readonly string ConfigFilePath =
            Path.Combine(GetConfigDirectory(), "encodingconf.json");

        protected override string FilePath => ConfigFilePath;

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

        // SVT-AV1
        public int SvtAv1Crf { get; set; } = 35;
        public int SvtAv1Abr { get; set; } = 10;
        public int SvtAv1Keyframe { get; set; } = 9;
        [JsonConverter(typeof(LegacyPresetKeyIntConverter))]
        public int SvtAv1Mode { get; set; } = 0;
        public bool SvtAv1Dl2 { get; set; } = false;
        public bool SvtAv1AutoTile { get; set; } = false;

        public bool UseLargePages { get; set; } = false;
        public string CustomParams { get; set; } = "";

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
}
