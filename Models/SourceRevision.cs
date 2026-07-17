using OneColumnEncoder.Models.Lang;

namespace OneColumnEncoder.Models;

public enum VideoAnalysisHypothesisKind
{
    ProgressiveSource,
    NativeDeinterlace,
    Pal22,
    Telecine3232,
    Telecine2323,
    Telecine3223,
    Telecine2332,
    FourField2224,
    FourField2242,
    FourField2422,
    FourField4222,
    EuroPulldown,
    MixedPip,
    Spliced
}

public enum VideoAnalysisFrameCountKind
{
    Exact,
    Estimated,
    Unknown
}

public sealed record VideoAnalysisHypothesisOption(
    string Id,
    string DisplayName,
    string Description,
    VideoAnalysisHypothesisKind Kind,
    bool IsUnsupported = false);

public sealed record FPSReviserRequest(
    string HypothesisId,
    int OutputFrameRateNumerator,
    int OutputFrameRateDenominator);

public sealed record SourceRevisionRequest(
    int Width,
    int Height,
    FPSReviserRequest FrameRate);

public sealed record FPSReviserResult(
    string EffectiveJson,
    VideoAnalysisHypothesisKind Kind,
    int OutputFrameRateNumerator,
    int OutputFrameRateDenominator,
    long? OutputFrameCount,
    VideoAnalysisFrameCountKind FrameCountKind,
    bool IsProgressive);

public static class VideoAnalysisHypothesisCatalog
{
    public static IReadOnlyList<VideoAnalysisHypothesisOption> GetOptions()
    {
        SourceReviserLangProvider lang = SourceReviserLangProvider.Current;
        return
        [
            Create(lang, "ProgressiveSource", VideoAnalysisHypothesisKind.ProgressiveSource),
            Create(lang, "NativeDeinterlace", VideoAnalysisHypothesisKind.NativeDeinterlace),
            Create(lang, "Pal22", VideoAnalysisHypothesisKind.Pal22),
            Create(lang, "Telecine3232", VideoAnalysisHypothesisKind.Telecine3232),
            Create(lang, "Telecine2323", VideoAnalysisHypothesisKind.Telecine2323),
            Create(lang, "Telecine3223", VideoAnalysisHypothesisKind.Telecine3223),
            Create(lang, "Telecine2332", VideoAnalysisHypothesisKind.Telecine2332),
            Create(lang, "FourField2224", VideoAnalysisHypothesisKind.FourField2224),
            Create(lang, "FourField2242", VideoAnalysisHypothesisKind.FourField2242),
            Create(lang, "FourField2422", VideoAnalysisHypothesisKind.FourField2422),
            Create(lang, "FourField4222", VideoAnalysisHypothesisKind.FourField4222),
            Create(lang, "EuroPulldown", VideoAnalysisHypothesisKind.EuroPulldown),
            Create(lang, "MixedPip", VideoAnalysisHypothesisKind.MixedPip, unsupported: true),
            Create(lang, "Spliced", VideoAnalysisHypothesisKind.Spliced, unsupported: true)
        ];
    }

    public static VideoAnalysisHypothesisKind ParseKind(string hypothesisId) => hypothesisId switch
    {
        "progressive-source" => VideoAnalysisHypothesisKind.ProgressiveSource,
        "native-deinterlace" => VideoAnalysisHypothesisKind.NativeDeinterlace,
        "pal-22" => VideoAnalysisHypothesisKind.Pal22,
        "telecine-3232" => VideoAnalysisHypothesisKind.Telecine3232,
        "telecine-2323" => VideoAnalysisHypothesisKind.Telecine2323,
        "telecine-3223" => VideoAnalysisHypothesisKind.Telecine3223,
        "telecine-2332" => VideoAnalysisHypothesisKind.Telecine2332,
        "four-field-2224" => VideoAnalysisHypothesisKind.FourField2224,
        "four-field-2242" => VideoAnalysisHypothesisKind.FourField2242,
        "four-field-2422" => VideoAnalysisHypothesisKind.FourField2422,
        "four-field-4222" => VideoAnalysisHypothesisKind.FourField4222,
        "euro-pulldown" => VideoAnalysisHypothesisKind.EuroPulldown,
        "mixed-pip" => VideoAnalysisHypothesisKind.MixedPip,
        "spliced" => VideoAnalysisHypothesisKind.Spliced,
        _ => throw new ArgumentException("Unknown video analysis hypothesis.", nameof(hypothesisId))
    };

    private static VideoAnalysisHypothesisOption Create(
        SourceReviserLangProvider lang,
        string key,
        VideoAnalysisHypothesisKind kind,
        bool unsupported = false) =>
        new(
            KindToId(kind),
            lang[$"SourceReviser.Option.{key}"],
            lang[$"SourceReviser.PatternDescription.{key}"],
            kind,
            unsupported);

    private static string KindToId(VideoAnalysisHypothesisKind kind) => kind switch
    {
        VideoAnalysisHypothesisKind.ProgressiveSource => "progressive-source",
        VideoAnalysisHypothesisKind.NativeDeinterlace => "native-deinterlace",
        VideoAnalysisHypothesisKind.Pal22 => "pal-22",
        VideoAnalysisHypothesisKind.Telecine3232 => "telecine-3232",
        VideoAnalysisHypothesisKind.Telecine2323 => "telecine-2323",
        VideoAnalysisHypothesisKind.Telecine3223 => "telecine-3223",
        VideoAnalysisHypothesisKind.Telecine2332 => "telecine-2332",
        VideoAnalysisHypothesisKind.FourField2224 => "four-field-2224",
        VideoAnalysisHypothesisKind.FourField2242 => "four-field-2242",
        VideoAnalysisHypothesisKind.FourField2422 => "four-field-2422",
        VideoAnalysisHypothesisKind.FourField4222 => "four-field-4222",
        VideoAnalysisHypothesisKind.EuroPulldown => "euro-pulldown",
        VideoAnalysisHypothesisKind.MixedPip => "mixed-pip",
        VideoAnalysisHypothesisKind.Spliced => "spliced",
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
