using OneColumnEncoder.Models.Analysis;
using System.IO;

namespace OneColumnEncoder.Commands.OpenClose;

public class OpenRawAnalysisCmd(
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
            string json = GetRawJson();
            string tempFile = Path.Combine(Path.GetTempPath(), "RawAnalysis.json");
            File.WriteAllText(tempFile, json);
            Process.Start(new ProcessStartInfo
            {
                FileName = "notepad.exe",
                Arguments = tempFile,
                UseShellExecute = true
            });
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