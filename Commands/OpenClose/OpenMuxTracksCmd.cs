using OneColumnEncoder.ViewModels.MuxTracks;
using System.IO;

namespace OneColumnEncoder.Commands.OpenClose;

public sealed class OpenMuxTracksCmd(
    ModalNavS modalNavS,
    Func<string[]> getSourcePaths,
    Func<string, IReadOnlyList<MuxTrackM>> getTracks,
    Func<string, string?> getSourceFfprobeJson,
    Action<string, IReadOnlyList<MuxTrackM>> applyTracks,
    Func<bool> canOpen) : OpenCloseBase(modalNavS)
{
    public override void Execute(object? parameter)
    {
        if (!canOpen() || TryActivateExistingWindow<MuxTracksConfModal>()) return;

        string[] paths = getSourcePaths()
            .Where(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (paths.Length == 0) return;

        MuxTracksConfModal window = new();
        MuxTracksConfVM vm = new(window.Close, paths, getTracks, getSourceFfprobeJson, applyTracks);
        ShowModal(window, vm, showDialog: true);
    }
}
