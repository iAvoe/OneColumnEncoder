using OneColumnEncoder.ViewModels.MuxTracks;
using System.IO;

namespace OneColumnEncoder.Commands.OpenClose;

public sealed class OpenMuxTracksCmd(
    ModalNavS modalNavS,
    Func<string[]> getSourcePaths,
    Func<string, IReadOnlyList<MuxTrackM>> getTracks,
    Func<string[], IReadOnlyDictionary<string, string?>> getFfprobeJsonBatch,
    Action<string, IReadOnlyList<MuxTrackM>> applyTracks,
    Func<bool> canOpen,
    Func<bool> hasSourceAnalysis) : OpenCloseBase(modalNavS)
{
    public override void Execute(object? parameter)
    {
        if (!canOpen() || TryActivateExistingWindow<MuxTracksConfModal>()) return;

        string[] paths = [.. getSourcePaths()
            .Where(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)];
        if (paths.Length == 0) return;

        IReadOnlyDictionary<string, string?> ffprobeJsonByPath = getFfprobeJsonBatch(paths);

        MuxTracksConfModal window = new();
        MuxTracksConfVM vm = new(window.Close, paths, getTracks, ffprobeJsonByPath, applyTracks, hasSourceAnalysis());
        ShowModal(window, vm, showDialog: true);
    }
}
