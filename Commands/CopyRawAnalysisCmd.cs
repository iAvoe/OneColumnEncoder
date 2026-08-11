using OneColumnEncoder.Models.Analysis;

namespace OneColumnEncoder.Commands;

public class CopyRawAnalysisCmd(
    VideoAnalysisM analysis,
    ModalNavS modalNavS) : BaseCmd
{
    private readonly VideoAnalysisM _analysis = analysis;
    private readonly ModalNavS _modalNavS = modalNavS;

    public override bool CanExecute(object? parameter) =>
        !string.IsNullOrWhiteSpace(GetRawJson());

    public override void Execute(object? parameter)
    {
        if (!CanExecute(null)) return;

        try
        {
            Clipboard.SetText(GetRawJson());
            new OpenSuccModalCmd(
                _modalNavS,
                UILangProvider.SrcAnalysisWindowTitle,
                UILangProvider.Current["SrcAnalysis.Copied"]).Execute(null);
        }
        catch (Exception ex)
        {
            new OpenErrModalCmd(
                _modalNavS,
                UILangProvider.SrcAnalysisWindowTitle,
                ex.Message).Execute(null);
        }
    }

    private string GetRawJson() =>
        _analysis.Route is SrcRouteKind.Queue or SrcRouteKind.Concat or SrcRouteKind.Repart
            && !string.IsNullOrWhiteSpace(_analysis.BatchRawJson)
            ? FormatBatchRawJson(_analysis.BatchRawJson)
            : _analysis.RawJson;

    private static string FormatBatchRawJson(string batchRawJson)
    {
        try
        {
            RawAnalysisBatchM data = JsonSerializer.Deserialize<RawAnalysisBatchM>(batchRawJson)
                ?? throw new InvalidOperationException("Failed to parse BatchRawJson.");
            return JsonSerializer.Serialize(data, FFProbeJsonFormatting.Options);
        }
        catch
        {
            return batchRawJson;
        }
    }
}
