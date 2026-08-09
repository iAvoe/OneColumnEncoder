using System.Collections.Specialized;

namespace OneColumnEncoder.ViewModels;

public sealed class BdPlaylistSelectVM : BaseVM
{
    public const string WindowTitleText = RepartLangProvider.BdPlaylistSelectWindowTitle;

    private readonly Action _cancelAction;
    private readonly Action _confirmAction;
    private readonly ButtonGroupVM _playlistButtons;
    private readonly ButtonGroupVM _finalPlaylistButtons1;
    private readonly ButtonGroupVM _finalPlaylistButtons2;
    private readonly ObservableCollection<BdPlaylistClusterM> _clusters = [];
    private readonly ObservableCollection<BdPlaylistM> _playlists = [];
    private readonly ObservableCollection<BdPlaylistM> _finalPlaylists = [];
    private readonly NotifyCollectionChangedEventHandler _finalPlaylistsChangedHandler;
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

        Clusters = new ReadOnlyObservableCollection<BdPlaylistClusterM>(_clusters);
        Playlists = new ReadOnlyObservableCollection<BdPlaylistM>(_playlists);
        FinalPlaylists = new ReadOnlyObservableCollection<BdPlaylistM>(_finalPlaylists);

        foreach (BdPlaylistClusterM cluster in scan.Clusters)
            _clusters.Add(cluster);

        _finalPlaylistsChangedHandler = (_, _) => RefreshFinalPlaylistState();
        _finalPlaylists.CollectionChanged += _finalPlaylistsChangedHandler;

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
    public ReadOnlyObservableCollection<BdPlaylistClusterM> Clusters { get; }
    public ReadOnlyObservableCollection<BdPlaylistM> Playlists { get; }
    public ReadOnlyObservableCollection<BdPlaylistM> FinalPlaylists { get; }
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
                PopulatePlaylists(value);

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

        _finalPlaylists.Add(SelectedPlaylist!);
        SelectedFinalPlaylist = SelectedPlaylist;
    }

    private void RemoveFromFinal()
    {
        if (SelectedFinalPlaylist == null)
            return;

        int index = _finalPlaylists.IndexOf(SelectedFinalPlaylist);
        _finalPlaylists.Remove(SelectedFinalPlaylist);
        SelectedFinalPlaylist = index < _finalPlaylists.Count ? _finalPlaylists[index] : (_finalPlaylists.Count > 0 ? _finalPlaylists[^1] : null);
    }

    private void Move(int delta)
    {
        if (SelectedFinalPlaylist == null)
            return;

        int index = _finalPlaylists.IndexOf(SelectedFinalPlaylist);
        int target = index + delta;
        if (target < 0 || target >= _finalPlaylists.Count)
            return;

        _finalPlaylists.Move(index, target);
        RefreshFinalPlaylistState();
    }

    private void ClearAll()
    {
        _finalPlaylists.Clear();
        SelectedFinalPlaylist = null;
    }

    private void RefreshFinalPlaylistState()
    {
        OnPropertyChanged(nameof(FinalPlaylistSummaryText));
        OnPropertyChanged(nameof(FinalPlaylistPaths));
        OnPropertyChanged(nameof(CanAddToFinal));
        OnPropertyChanged(nameof(CanRemoveFromFinal));
        OnPropertyChanged(nameof(CanMoveUp));
        OnPropertyChanged(nameof(CanMoveDown));
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

    private void PopulatePlaylists(BdPlaylistClusterM? cluster)
    {
        _playlists.Clear();
        if (cluster == null)
            return;

        foreach (BdPlaylistM playlist in cluster.Playlists)
            _playlists.Add(playlist);
    }

    public override void Dispose()
    {
        _finalPlaylists.CollectionChanged -= _finalPlaylistsChangedHandler;
        base.Dispose();
    }

    private static string FormatTimeSpan(TimeSpan value) => value.ToString(@"hh\:mm\:ss\.fff");
}
