using OneColumnEncoder.ViewModels.MuxTracks;
using System.IO;

namespace OneColumnEncoder.Commands.OpenClose;

public sealed class OpenMuxTracksCmd(
    ModalNavS modalNavS,
    Func<string[]> getSourcePaths,
    Func<string, IReadOnlyList<MuxTrackM>> getTracks,
    Func<string> getFfprobePath,
    Action<string, IReadOnlyList<MuxTrackM>> applyTracks,
    Func<bool> canOpen) : OpenCloseBase(modalNavS)
{
    private bool _isExecuting;

    public override async void Execute(object? parameter)
    {
        if (_isExecuting || !canOpen() || TryActivateExistingWindow<MuxTracksConfModal>()) return;

        string[] paths = [.. getSourcePaths()
            .Where(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)];
        if (paths.Length == 0) return;

        _isExecuting = true;
        OnCanExecuteChanged();
        try
        {
            // Required: subtitle tracks come from a full ffprobe result for each source,
            // rather than from the general source-analysis lifecycle.
            string ffprobePath = getFfprobePath();
            Task<string>[] analyses = [.. paths.Select(path =>
                FFProbeVideoAnalysis.AnalyzeAsync(ffprobePath, path))];
            string[] ffprobeJson = await Task.WhenAll(analyses);
            Dictionary<string, string?> ffprobeJsonByPath =
                paths.Zip(ffprobeJson, (path, json) => (path, json))
                    .ToDictionary(item => item.path, item => (string?)item.json, StringComparer.OrdinalIgnoreCase);

            MuxTracksConfModal window = new();
            MuxTracksConfVM vm = new(window.Close, paths, getTracks, ffprobeJsonByPath, applyTracks);
            ShowModal(window, vm, showDialog: true);
        }
        catch (Exception ex)
        {
            new OpenErrModalCmd(
                ModalNavS,
                MuxLangProvider.WindowTitle,
                ex.Message).Execute(null);
        }
        finally
        {
            _isExecuting = false;
            OnCanExecuteChanged();
        }
    }
}
