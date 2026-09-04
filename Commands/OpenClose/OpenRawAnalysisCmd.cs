using OneColumnEncoder.Models.Analysis;
using System.IO;

namespace OneColumnEncoder.Commands.OpenClose;

public class OpenRawAnalysisCmd(
    VideoAnalysisM analysis,
    AppConfM appConfM,
    ModalNavS modalNavS) : BaseCmd
{
    private readonly VideoAnalysisM _analysis = analysis;
    private readonly AppConfM _appConfM = appConfM;
    private readonly ModalNavS _modalNavS = modalNavS;

    public override bool CanExecute(object? parameter) =>
        !string.IsNullOrWhiteSpace(GetRawJson());

    public override void Execute(object? parameter)
    {
        if (!CanExecute(null)) return;

        try
        {
            string json = GetRawJson();
            string sourceName = string.IsNullOrWhiteSpace(_analysis.SrcPath)
                ? "RawAnalysis"
                : Path.GetFileNameWithoutExtension(_analysis.SrcPath);
            string tempFile = Path.Combine(Path.GetTempPath(), $"{sourceName}_RawAnalysis.json");
            File.WriteAllText(tempFile, json);
            string editorPath = string.IsNullOrWhiteSpace(_appConfM.TextEditor.TextEditorPath)
                ? "notepad.exe"
                : _appConfM.TextEditor.TextEditorPath;
            Process.Start(new ProcessStartInfo
            {
                FileName = editorPath,
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