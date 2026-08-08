using System.Text.Json.Nodes;

namespace OneColumnEncoder.Commands;

public class CopyRawAnalysisCmd(
    VideoAnalysisM analysis,
    ModalNavS modalNavS,
    Func<bool>? isQueueRoute = null,
    Func<bool>? isConcatRoute = null) : BaseCmd
{
    private readonly VideoAnalysisM _analysis = analysis;
    private readonly ModalNavS _modalNavS = modalNavS;
    private readonly Func<bool>? _isQueueRoute = isQueueRoute;
    private readonly Func<bool>? _isConcatRoute = isConcatRoute;

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
        (_isQueueRoute?.Invoke() == true || _isConcatRoute?.Invoke() == true) && !string.IsNullOrWhiteSpace(_analysis.QueueRawJson)
            ? FormatQueueRawJson(_analysis.QueueRawJson)
            : _analysis.RawJson;

    private static string FormatQueueRawJson(string queueRawJson)
    {
        try
        {
            JsonNode? root = JsonNode.Parse(queueRawJson);
            if (root is not JsonArray entries)
                return queueRawJson;

            bool transformed = false;
            foreach (JsonNode? entryNode in entries)
            {
                if (entryNode is not JsonObject entry)
                    continue;

                if (entry["RawJson"] is not JsonValue rawJsonValue
                    || !rawJsonValue.TryGetValue<string>(out string? rawJson)
                    || string.IsNullOrWhiteSpace(rawJson))
                    continue;

                JsonNode? parsedRawJson = JsonNode.Parse(rawJson);
                if (parsedRawJson == null)
                    continue;

                entry["RawJson"] = parsedRawJson;
                transformed = true;
            }

            return transformed
                ? root.ToJsonString(FFProbeJsonFormatting.Options)
                : queueRawJson;
        }
        catch
        {
            return queueRawJson;
        }
    }
}
