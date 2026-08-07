using OneColumnEncoder.Commands;
using OneColumnEncoder.Models;
using OneColumnEncoder.Models.Lang;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels;

public sealed class BdPlaylistSelectVM : BaseVM
{
    public const string WindowTitleText = "1cenc BD Playlist Selector";

    private readonly Action _cancelAction;
    private readonly Action _confirmAction;
    private readonly ButtonGroupVM _footerButtons;
    private BdPlaylistClusterM? _selectedCluster;
    private BdPlaylistM? _selectedPlaylist;

    public BdPlaylistSelectVM(
        BdPlaylistScanResult scan,
        Action cancelAction,
        Action confirmAction)
    {
        _cancelAction = cancelAction;
        _confirmAction = confirmAction;
        _footerButtons = ButtonGroupVM.CreateTwoButton(
            CancelText,
            ConfirmText,
            new ActionCmd(_ => _cancelAction()),
            new ActionCmd(_ => Confirm()));
        _footerButtons.B2_2IsEnabled = false;

        foreach (BdPlaylistClusterM cluster in scan.Clusters)
            Clusters.Add(cluster);

        SummaryText = string.Format(PlaylistSummaryFormat, scan.Clusters.Count, scan.Clusters.Sum(cluster => cluster.PlaylistCount));

        if (Clusters.Count > 0)
            SelectedCluster = Clusters[0];
    }

    public static string ClustersTitle => RepartLangProvider.Current["PlaylistClusters"];
    public static string PlaylistsTitle => RepartLangProvider.Current["PlaylistPlaylists"];
    public static string CancelText => RepartLangProvider.Current["Cancel"];
    public static string ConfirmText => RepartLangProvider.Current["PlaylistConfirm"];
    public static string PlaylistSummaryFormat => "{0} cluster(s) / {1} playlist(s)";

    public string WindowTitle => WindowTitleText;
    public string SummaryText { get; }
    public ObservableCollection<BdPlaylistClusterM> Clusters { get; } = [];
    public ObservableCollection<BdPlaylistM> Playlists { get; } = [];
    public ButtonGroupVM FooterButtons => _footerButtons;

    public BdPlaylistClusterM? SelectedCluster
    {
        get => _selectedCluster;
        set
        {
            if (SetProperty(ref _selectedCluster, value))
            {
                Playlists.Clear();
                if (value != null)
                {
                    foreach (BdPlaylistM playlist in value.Playlists)
                        Playlists.Add(playlist);
                }

                SelectedPlaylist = value?.Playlists.Count == 1 ? value.Playlists[0] : null;
                OnPropertyChanged(nameof(SelectedClusterSummaryText));
            }
        }
    }

    public BdPlaylistM? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set
        {
            if (SetProperty(ref _selectedPlaylist, value))
            {
                _footerButtons.B2_2IsEnabled = value != null;
                OnPropertyChanged(nameof(SelectedPlaylistSummaryText));
                OnPropertyChanged(nameof(SelectedPlaylistPath));
            }
        }
    }

    public string? SelectedPlaylistPath => SelectedPlaylist?.FilePath;
    public string SelectedClusterSummaryText => SelectedCluster == null
        ? string.Empty
        : string.Format(SelectedClusterFormat, SelectedCluster.PlaylistCount, SelectedCluster.DurationText, SelectedCluster.ClipCount, SelectedCluster.ChapterCount);
    public string SelectedPlaylistSummaryText => SelectedPlaylist == null
        ? string.Empty
        : string.Format(SelectedPlaylistFormat, SelectedPlaylist.DurationText, SelectedPlaylist.ClipCount, SelectedPlaylist.ChapterCount);

    public static string SelectedClusterFormat => "{0} playlist(s) | {1} | {2} clips | {3} chapters";
    public static string SelectedPlaylistFormat => "{0} | {1} clips | {2} chapters";

    private void Confirm()
    {
        if (SelectedPlaylist == null)
            return;

        _confirmAction();
    }
}
