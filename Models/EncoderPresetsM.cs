using System.Collections.Generic;
using System.Linq;

namespace OneColumnEncoder.Models
{
    public class EncoderPresetItem(string key, string nameKey, string param)
    {
        public string Key { get; } = key;
        public string NameKey { get; } = nameKey;
        public string Params { get; } = param;
    }

    public class ThirdPartyParamDef(string propertyName, string paramOn, string paramOff)
    {
        public string PropertyName { get; } = propertyName;
        public string ParamOn { get; } = paramOn;
        public string ParamOff { get; } = paramOff;
    }

    public static class EncoderPresetsM
    {
        public static IReadOnlyList<EncoderPresetItem> X264Presets { get; } =
        [
            new("a", "GeneralPurposeText",
                "--bframes 14 --b-adapt 2 --me umh --subme 9 --merange 48 --no-fast-pskip --direct auto --weightb --min-keyint 5 --ref 3 $crfParam --chroma-qp-offset -2 --aq-mode 3 --aq-strength 0.7 --trellis 2 --deblock 0:0 --psy-rd 0.77:0.22"),
            new("b", "StockFootageText",
                "--partitions all --bframes 12 --b-adapt 2 --me esa --merange 48 --no-fast-pskip --direct auto --weightb --min-keyint 1 --ref 3 $crfParam --tune grain --trellis 2"),
        ];

        public static IReadOnlyList<EncoderPresetItem> X265Presets { get; } =
        [
            new("a", "GeneralPurposeText",
                "--high-tier --preset slow --me umh --weightb --aq-mode 4 --bframes 5 --ref 3"),
            new("b", "FilmIRLText",
                "--high-tier --ctu 64 --tu-intra-depth 4 --tu-inter-depth 4 --limit-tu 1 --rect --tskip --tskip-fast --me star --weightb --ref 4 --max-merge 5 --no-open-gop --min-keyint 3 --fades --bframes 8 --b-adapt 2 --b-intra $crfParam --crqpoffs -3 --ipratio 1.2 --pbratio 1.5 --rdoq-level 2 --aq-mode 4 --aq-strength 1.1 --qg-size 8 --rd 5 --limit-refs 0 --rskip 0 --deblock 0:-1 --limit-sao --sao-non-deblock --selective-sao 3"),
            new("c", "StockFootageText",
                "--high-tier --ctu 32 --tskip --me star --max-merge 5 --early-skip --b-intra --no-open-gop --min-keyint 1 --ref 3 --fades --bframes 7 --b-adapt 2 $crfParam --crqpoffs -3 --cbqpoffs -2 --rd 3 --limit-modes --limit-refs 1 --rskip 1 --splitrd-skip --deblock -1:-1 --tune grain"),
            new("d", "AnimeText",
                "--high-tier --tu-intra-depth 4 --tu-inter-depth 4 --max-tu-size 16 --tskip --tskip-fast --me umh --weightb --max-merge 5 --early-skip --ref 3 --no-open-gop --min-keyint 5 --fades --bframes 16 --b-adapt 2 --bframe-bias 20 --constrained-intra --b-intra $crfParam --crqpoffs -4 --cbqpoffs -2 --ipratio 1.6 --pbratio 1.3 --cu-lossless --psy-rdoq 2.3 --rdoq-level 2 --hevc-aq --aq-strength 0.9 --qg-size 8 --rd 3 --limit-modes --limit-refs 1 --rskip 1 --rect --amp --psy-rd 1.5 --splitrd-skip --rdpenalty 2 --deblock -1:0 --limit-sao --sao-non-deblock"),
            new("e", "StressTestText",
                "--high-tier --tu-intra-depth 4 --tu-inter-depth 4 --max-tu-size 4 --limit-tu 1 --rect --amp --tskip --me star --weightb --max-merge 5 --ref 3 --no-open-gop --min-keyint 1 --fades --bframes 16 --b-adapt 2 --b-intra $crfParam --crqpoffs -5 --cbqpoffs -2 --ipratio 1.67 --pbratio 1.33 --cu-lossless --psy-rdoq 2.5 --rdoq-level 2 --hevc-aq --aq-strength 1.4 --qg-size 8 --rd 5 --limit-refs 0 --rskip 2 --rskip-edge-threshold 3 --no-cutree --psy-rd 1.5 --rdpenalty 2 --deblock -2:-2 --limit-sao --sao-non-deblock --selective-sao 1"),
        ];

        public static IReadOnlyList<EncoderPresetItem> SvtAv1Presets { get; } =
        [
            new("a", "PeakQualityText",
                "--preset 2 --scd 1 --enable-tf 2 --tf-strength 2 $crfParam --enable-qm 1 --enable-variance-boost 1 --variance-boost-curve 2 --variance-boost-strength 2 --variance-octile 2 --sharpness 6 --progress 1 $deblock"),
            new("b", "CompressionOptText",
                "--preset 2 --scd 1 --enable-tf 2 --tf-strength 2 $crfParam --sharpness 4 --progress 1 $deblock"),
            new("c", "SpeedOptimizedText",
                "--preset 2 --scd 1 --scm 0 --enable-tf 2 --tf-strength 2 $crfParam --tune 0 --enable-variance-boost 1 --variance-boost-curve 2 --variance-boost-strength 2 --variance-octile 2 --sharpness 4 --progress 1"),
        ];

        public static IReadOnlyList<ThirdPartyParamDef> ThirdPartyParams { get; } =
        [
            new("X264Mod", "--fgo", ""),
            new("X265Aq", "--aq-auto 10", ""),
            new("X265Dark", "--aq-bias-strength 1.3", ""),
            new("X265Texture", "--aq-strength-edge 1.4", ""),
            new("SvtAv1Dl2", "--enable-dlf 2", "--enable-dlf 1"),
            new("SvtAv1AutoTile", "--auto-tiling 1", ""),
        ];

        public static EncoderPresetItem? GetX264Preset(string key) =>
            X264Presets.FirstOrDefault(p => p.Key == key);

        public static EncoderPresetItem? GetX265Preset(string key) =>
            X265Presets.FirstOrDefault(p => p.Key == key);

        public static EncoderPresetItem? GetSvtAv1Preset(string key) =>
            SvtAv1Presets.FirstOrDefault(p => p.Key == key);
    }
}
