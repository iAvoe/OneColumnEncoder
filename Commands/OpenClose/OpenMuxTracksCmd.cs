using OneColumnEncoder.ViewModels.MuxTracks;
using System.IO;

namespace OneColumnEncoder.Commands.OpenClose;

public sealed class OpenMuxTracksCmd(
    ModalNavS modalNavS,
    Func<string[]> getSourcePaths,
    Func<string, IReadOnlyList<MuxTrackM>> getTracks,
    Func<string?> getFFmpegPath,
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
            .Select(path => path!)];
        if (paths.Length == 0) return;

        if (HasDuplicatePaths(paths))
        {
            ShowCannotMuxSubtitleError(MuxLangProvider.Current["MuxTracks.DuplicateSourcePaths"]);
            return;
        }

        string? ffmpegPath = getFFmpegPath();
        if (string.IsNullOrWhiteSpace(ffmpegPath) || !File.Exists(ffmpegPath))
        {
            ShowCannotMuxSubtitleError(MuxLangProvider.Current["MuxTracks.MissingFfmpeg"]);
            return;
        }

        string ffprobePath = getFfprobePath();
        if (string.IsNullOrWhiteSpace(ffprobePath) || !File.Exists(ffprobePath))
        {
            ShowCannotMuxSubtitleError(MuxLangProvider.Current["MuxTracks.MissingFfprobe"]);
            return;
        }

        _isExecuting = true;
        OnCanExecuteChanged();
        try
        {
            // Required: subtitle tracks come from a full ffprobe result for each source,
            // rather than from the general source-analysis lifecycle.
            Task<string>[] analyses = [.. paths.Select(path =>
                FFProbeVideoAnalysis.AnalyzeAsync(ffprobePath, path))];
            string[] ffprobeJson = await Task.WhenAll(analyses);
            Dictionary<string, string?> ffprobeJsonByPath =
                paths.Zip(ffprobeJson, (path, json) => (path, json))
                    .ToDictionary(item => item.path, item => (string?)item.json, StringComparer.OrdinalIgnoreCase);

            MuxTracksConfModal window = new();
            Action<string> showError = description =>
                new OpenErrModalCmd(ModalNavS, MuxLangProvider.WindowTitle, description).Execute(null);
            Func<string, bool> confirmNoDefaultSubtitle = message =>
            {
                OpenWarnModalCmd cmd = new(ModalNavS, MuxLangProvider.WindowTitle, message);
                cmd.Execute(null);
                return cmd.DialogResult == true;
            };
            MuxTracksConfVM vm = new(window.Close, paths, getTracks, ffprobeJsonByPath, applyTracks, showError, confirmNoDefaultSubtitle);
            ShowModal(window, vm, showDialog: true);
        }
        catch (Exception ex)
        {
            ShowCannotMuxSubtitleError(ex.Message);
        }
        finally
        {
            _isExecuting = false;
            OnCanExecuteChanged();
        }
    }

    private void ShowCannotMuxSubtitleError(string reason) =>
        new OpenErrModalCmd(
            ModalNavS,
            MuxLangProvider.WindowTitle,
            string.Join(Environment.NewLine, MuxLangProvider.Current["MuxTracks.CannotMuxSubtitle"], reason)).Execute(null);

    private static bool HasDuplicatePaths(IReadOnlyList<string> paths)
    {
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        foreach (string path in paths)
        {
            if (!seen.Add(path)) return true;
        }

        return false;
    }
}
