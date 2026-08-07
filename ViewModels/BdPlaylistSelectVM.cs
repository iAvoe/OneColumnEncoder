using OneColumnEncoder.Commands;
using OneColumnEncoder.Models;
using OneColumnEncoder.Models.Lang;
using OneColumnEncoder.UI;
using System.Collections.ObjectModel;

namespace OneColumnEncoder.ViewModels;

public sealed class BdPlaylistSelectVM : BaseVM
{
    public const string WindowTitleText = "1cenc BD Playlist Selector";

    private readonly Action _cancelAction;
    private readonly Action _confirmAction;
    private readonly ButtonGroupVM _playlistButtons;
    private readonly ButtonGroupVM _finalPlaylistButtons1;
    private readonly ButtonGroupVM _finalPlaylistButtons2;
    private BdPlaylistClusterM? _selectedCluster;
    private BdPlaylistM? _selectedPlaylist;
    private BdPlaylistM? _selectedFinalPlaylist;

    public BdPlaylistSelectVM(
        BdPlaylistScanResult scan,
        Action cancelAction,
        Action confirmAction)
    {
        _cancelAction = cancelAction;
        _confirmAction = confirmAction;
        _playlistButtons = ButtonGroupVM.CreateTwoButton(
            CancelText,
            AddToFinalText,
            new ActionCmd(_ => _cancelAction()),
            new ActionCmd(_ => AddToFinal()));
        _playlistButtons.B2_1Icon = SvgIconProvider.GameXMark;
        _playlistButtons.B2_2Icon = SvgIconProvider.GamePlus;

        _finalPlaylistButtons1 = ButtonGroupVM.CreateTwoButton(
            MoveUpText,
            MoveDownText,
            new ActionCmd(_ => Move(-1)),
            new ActionCmd(_ => Move(+1)));

        _finalPlaylistButtons2 = ButtonGroupVM.CreateThreeButton(
            RemoveFromFinalText,
            ClearAllText,
            ConfirmText,
            new ActionCmd(_ => RemoveFromFinal()),
            new ActionCmd(_ => ClearAll()),
            new ActionCmd(_ => Confirm()));
        _finalPlaylistButtons2.B3_1Icon = SvgIconProvider.GameDelete;
        _finalPlaylistButtons2.B3_2Icon = SvgIconProvider.GameXMark;
        _finalPlaylistButtons2.B3_3Icon = SvgIconProvider.GameCorrectMark;

        foreach (BdPlaylistClusterM cluster in scan.Clusters)
            Clusters.Add(cluster);

        FinalPlaylists.CollectionChanged += (_, _) => RefreshFinalPlaylistState();

        SummaryText = string.Format(PlaylistSummaryFormat, scan.Clusters.Count, scan.Clusters.Sum(cluster => cluster.PlaylistCount));

        if (Clusters.Count > 0)
            SelectedCluster = Clusters[0];

        RefreshFinalPlaylistState();
    }

    public static string ClustersTitle => RepartLangProvider.Current["PlaylistClusters"];
    public static string PlaylistsTitle => RepartLangProvider.Current["PlaylistPlaylists"];
    public static string FinalPlaylistsTitle => RepartLangProvider.Current["PlaylistFinalPlaylists"];
    public static string AddToFinalText => RepartLangProvider.Current["PlaylistAddToFinal"];
    public static string RemoveFromFinalText => RepartLangProvider.Current["PlaylistRemoveFromFinal"];
    public static string ClearAllText => RepartLangProvider.Current["PlaylistClearAll"];
    public static string MoveUpText => RepartLangProvider.MoveUp;
    public static string MoveDownText => RepartLangProvider.MoveDown;
    public static string CancelText => RepartLangProvider.Current["Cancel"];
    public static string ConfirmText => RepartLangProvider.Current["PlaylistConfirm"];
    public static string PlaylistSummaryFormat => "{0} cluster(s) / {1} playlist(s)";
    public static string FinalPlaylistSummaryFormat => "{0} playlist(s) | {1}";
    public static string SelectedClusterFormat => "{0} playlist(s) | {1} | {2} clips | {3} chapters";
    public static string SelectedPlaylistFormat => "{0} | {1} clips | {2} chapters";
    public static string SelectedFinalPlaylistFormat => "{0} | {1} clips | {2} chapters";

    public static string WindowTitle => WindowTitleText;
    public string SummaryText { get; }
    public string FinalPlaylistSummaryText => FinalPlaylists.Count == 0
        ? string.Empty
        : string.Format(
            FinalPlaylistSummaryFormat,
            FinalPlaylists.Count,
            FormatTimeSpan(FinalPlaylists.Aggregate(TimeSpan.Zero, (sum, playlist) => sum + playlist.Duration)));
    public ObservableCollection<BdPlaylistClusterM> Clusters { get; } = [];
    public ObservableCollection<BdPlaylistM> Playlists { get; } = [];
    public ObservableCollection<BdPlaylistM> FinalPlaylists { get; } = [];
    public ButtonGroupVM PlaylistButtons => _playlistButtons;
    public ButtonGroupVM FinalPlaylistButtons1 => _finalPlaylistButtons1;
    public ButtonGroupVM FinalPlaylistButtons2 => _finalPlaylistButtons2;

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
                OnPropertyChanged(nameof(SelectedPlaylistSummaryText));
                RefreshFinalPlaylistState();
            }
        }
    }

    public BdPlaylistM? SelectedFinalPlaylist
    {
        get => _selectedFinalPlaylist;
        set
        {
            if (SetProperty(ref _selectedFinalPlaylist, value))
            {
                OnPropertyChanged(nameof(SelectedFinalPlaylistSummaryText));
                RefreshFinalPlaylistState();
            }
        }
    }

    public bool CanAddToFinal => SelectedPlaylist != null
        && !FinalPlaylists.Any(playlist => string.Equals(playlist.FilePath, SelectedPlaylist.FilePath, StringComparison.OrdinalIgnoreCase));
    public bool CanRemoveFromFinal => SelectedFinalPlaylist != null;
    public bool CanMoveUp => SelectedFinalPlaylist != null && FinalPlaylists.IndexOf(SelectedFinalPlaylist) > 0;
    public bool CanMoveDown => SelectedFinalPlaylist != null
        && FinalPlaylists.Count > 1
        && FinalPlaylists.IndexOf(SelectedFinalPlaylist) < FinalPlaylists.Count - 1;

    public IReadOnlyList<string> FinalPlaylistPaths => [.. FinalPlaylists.Select(playlist => playlist.FilePath)];
    public string SelectedClusterSummaryText => SelectedCluster == null
        ? string.Empty
        : string.Format(SelectedClusterFormat, SelectedCluster.PlaylistCount, SelectedCluster.DurationText, SelectedCluster.ClipCount, SelectedCluster.ChapterCount);
    public string SelectedPlaylistSummaryText => SelectedPlaylist == null
        ? string.Empty
        : string.Format(SelectedPlaylistFormat, SelectedPlaylist.DurationText, SelectedPlaylist.ClipCount, SelectedPlaylist.ChapterCount);
    public string SelectedFinalPlaylistSummaryText => SelectedFinalPlaylist == null
        ? string.Empty
        : string.Format(SelectedFinalPlaylistFormat, SelectedFinalPlaylist.DurationText, SelectedFinalPlaylist.ClipCount, SelectedFinalPlaylist.ChapterCount);

    private void AddToFinal()
    {
        if (!CanAddToFinal)
            return;

        FinalPlaylists.Add(SelectedPlaylist!);
        SelectedFinalPlaylist = SelectedPlaylist;
    }

    private void RemoveFromFinal()
    {
        if (SelectedFinalPlaylist == null)
            return;

        int index = FinalPlaylists.IndexOf(SelectedFinalPlaylist);
        FinalPlaylists.Remove(SelectedFinalPlaylist);
        SelectedFinalPlaylist = index < FinalPlaylists.Count ? FinalPlaylists[index] : (FinalPlaylists.Count > 0 ? FinalPlaylists[^1] : null);
    }

    private void Move(int delta)
    {
        if (SelectedFinalPlaylist == null)
            return;

        int index = FinalPlaylists.IndexOf(SelectedFinalPlaylist);
        int target = index + delta;
        if (target < 0 || target >= FinalPlaylists.Count)
            return;

        FinalPlaylists.Move(index, target);
        RefreshFinalPlaylistState();
    }

    private void ClearAll()
    {
        FinalPlaylists.Clear();
        SelectedFinalPlaylist = null;
    }

    private void RefreshFinalPlaylistState()
    {
        OnPropertyChanged(nameof(FinalPlaylistSummaryText));
        _playlistButtons.B2_2IsEnabled = CanAddToFinal;

        _finalPlaylistButtons1.B2_1IsEnabled = CanMoveUp;
        _finalPlaylistButtons1.B2_2IsEnabled = CanMoveDown;

        _finalPlaylistButtons2.B3_1IsEnabled = CanRemoveFromFinal;
        _finalPlaylistButtons2.B3_2IsEnabled = FinalPlaylists.Count > 0;
        _finalPlaylistButtons2.B3_3IsEnabled = FinalPlaylists.Count > 0;
    }

    private void Confirm()
    {
        if (FinalPlaylists.Count == 0)
            return;

        _confirmAction();
    }

    private static string FormatTimeSpan(TimeSpan value) => value.ToString(@"hh\:mm\:ss\.fff");
}
