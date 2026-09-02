using System.Text.Json;

namespace OneColumnEncoder.Models.Analysis;

/// <summary>
/// A single source file's raw ffprobe JSON inside a batch (Queue/Concat/Repart) analysis.
/// Serialized as one entry of <see cref="RawAnalysisBatchM"/>.
/// </summary>
public sealed record SourceRawAnalysisM(
    string FilePath,
    string DisplayName,
    JsonElement FfprobeJson);

/// <summary>
/// The serialized batch payload stored in <see cref="VideoAnalysisM.BatchRawJson"/>.
/// Shared by the writer (AnalyzeSrcVideoCmd / Repart plan application) and every reader
/// (CopyRawAnalysisCmd, FFProbeSrcRevisionModel) so the JSON contract cannot drift.
/// </summary>
public sealed record RawAnalysisBatchM(IReadOnlyList<SourceRawAnalysisM> Entries);
