using OneColumnEncoder.Models;
using OneColumnEncoder.Pipeline;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace OneColumnEncoder.ViewModels;

public sealed class RepartFilterScribeOutputQueueVM : BaseVM
{
    private string[] _originalOutputIds = [];
    private bool _hasOriginalQueueChanges;

    public ObservableCollection<RepartFilterScribeOutputItemVM> Items { get; } = [];

    public static string OrderingTitle => RepartLangProvider.Current["FilterScribeOutputOrdering"];
    public static string RestoreOriginalQueueText => RepartLangProvider.Current["FilterScribeRestoreOutputQueue"];
    public bool HasOriginalQueueChanges
    {
        get => _hasOriginalQueueChanges;
        private set => SetProperty(ref _hasOriginalQueueChanges, value);
    }

    public ICommand? MoveItemUpCommand { get; set; }
    public ICommand? MoveItemDownCommand { get; set; }
    public ICommand? RestoreOriginalQueueCommand { get; set; }

    public void LoadItems(IEnumerable<RepartOutputSegmentM> outputs, int frameRateNumerator, int frameRateDenominator)
    {
        _originalOutputIds = [.. outputs.Select(output => output.Id.ToString("N"))];
        ReplaceItems(outputs, frameRateNumerator, frameRateDenominator);
        RefreshChangeState();
    }

    public bool MoveItemUp(RepartFilterScribeOutputItemVM item)
    {
        int index = Items.IndexOf(item);
        if (index <= 0) return false;
        Items.Move(index, index - 1);
        item.FlashMovedHighlight();
        RefreshItemStates();
        RefreshChangeState();
        return true;
    }

    public bool MoveItemDown(RepartFilterScribeOutputItemVM item)
    {
        int index = Items.IndexOf(item);
        if (index < 0 || index >= Items.Count - 1) return false;
        Items.Move(index, index + 1);
        item.FlashMovedHighlight();
        RefreshItemStates();
        RefreshChangeState();
        return true;
    }

    public bool RestoreOriginalQueue()
    {
        if (!HasOriginalQueueChanges) return false;

        Dictionary<string, RepartFilterScribeOutputItemVM> itemsById = Items.ToDictionary(
            item => item.Model.Id.ToString("N"), StringComparer.OrdinalIgnoreCase);
        ReplaceItems(_originalOutputIds.Select(id => itemsById[id].Model),
            Items.FirstOrDefault()?.FrameRateNumerator ?? 0,
            Items.FirstOrDefault()?.FrameRateDenominator ?? 1);
        RefreshChangeState();
        return true;
    }

    public Guid[] GetCurrentOutputIds() => [.. Items.Select(item => item.Model.Id)];

    public void RefreshLanguage()
    {
        OnPropertyChanged(nameof(OrderingTitle));
        OnPropertyChanged(nameof(RestoreOriginalQueueText));
    }

    private void ReplaceItems(IEnumerable<RepartOutputSegmentM> outputs, int frameRateNumerator, int frameRateDenominator)
    {
        DisposeItems();
        Items.Clear();
        int index = 0;
        foreach (RepartOutputSegmentM output in outputs)
            Items.Add(new RepartFilterScribeOutputItemVM(output, index++, frameRateNumerator, frameRateDenominator, MoveItemUpCommand, MoveItemDownCommand));
        RefreshItemStates();
    }

    private void RefreshChangeState() =>
        HasOriginalQueueChanges = !GetCurrentOutputIds().Select(id => id.ToString("N"))
            .SequenceEqual(_originalOutputIds, StringComparer.OrdinalIgnoreCase);

    private void RefreshItemStates()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].CanMoveUp = i > 0;
            Items[i].CanMoveDown = i < Items.Count - 1;
        }
    }

    private void DisposeItems()
    {
        foreach (RepartFilterScribeOutputItemVM item in Items)
            item.Dispose();
    }

    public override void Dispose()
    {
        DisposeItems();
        base.Dispose();
    }
}

public sealed class RepartFilterScribeOutputItemVM : BaseVM
{
    private bool _canMoveUp;
    private bool _canMoveDown;
    private bool _isSelected;
    private DispatcherTimer? _moveFlashTimer;

    public RepartFilterScribeOutputItemVM(
        RepartOutputSegmentM model,
        int index,
        int frameRateNumerator,
        int frameRateDenominator,
        ICommand? moveUpCommand,
        ICommand? moveDownCommand)
    {
        Model = model;
        FrameRateNumerator = frameRateNumerator;
        FrameRateDenominator = frameRateDenominator;
        Name = model.BaseName;
        double start = (double)model.FirstFrame * frameRateDenominator / frameRateNumerator;
        double end = (double)(model.LastFrame + 1) * frameRateDenominator / frameRateNumerator;
        P1Text = $"{EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(start))} - {EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(end))}  |  {model.FrameCount:N0}f";
        R2Command = moveUpCommand;
        R3Command = moveDownCommand;
    }

    public RepartOutputSegmentM Model { get; }
    public int FrameRateNumerator { get; }
    public int FrameRateDenominator { get; }
    public string Name { get; }
    public string P1Text { get; }
    public static string DisplayR1Text => string.Empty;
    public static string R2Text => RepartLangProvider.MoveUp;
    public static string R3Text => RepartLangProvider.MoveDown;
    public static bool R1IsEnabled => false;
    public bool R2IsEnabled => _canMoveUp;
    public bool R3IsEnabled => _canMoveDown;
    public static ICommand? R1Command => null;
    public ICommand? R2Command { get; }
    public ICommand? R3Command { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool IsRecentlyMoved { get; private set; }
    public bool CanMoveUp
    {
        get => _canMoveUp;
        set
        {
            if (SetProperty(ref _canMoveUp, value)) OnPropertyChanged(nameof(R2IsEnabled));
        }
    }

    public bool CanMoveDown
    {
        get => _canMoveDown;
        set
        {
            if (SetProperty(ref _canMoveDown, value)) OnPropertyChanged(nameof(R3IsEnabled));
        }
    }

    public void FlashMovedHighlight()
    {
        IsRecentlyMoved = true;
        OnPropertyChanged(nameof(IsRecentlyMoved));
        _moveFlashTimer?.Stop();
        _moveFlashTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(600), DispatcherPriority.Normal, (_, _) =>
        {
            IsRecentlyMoved = false;
            OnPropertyChanged(nameof(IsRecentlyMoved));
            _moveFlashTimer?.Stop();
        }, Dispatcher.CurrentDispatcher);
        _moveFlashTimer.Start();
    }

    public override void Dispose()
    {
        _moveFlashTimer?.Stop();
        _moveFlashTimer = null;
        base.Dispose();
    }
}
