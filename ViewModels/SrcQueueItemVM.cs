using System.IO;
using System.Windows.Threading;

namespace OneColumnEncoder.ViewModels;

public sealed class SrcQueueItemVM : BaseVM
{
    private bool _canMoveUp;
    private bool _canMoveDown;
    private bool _canRemove = true;
    private bool _isSelected;
    private string _name = "";
    private string _fullPath = "";
    private string _pathText = "";
    private string _displayR1Text = "";
    private string _r2Text = "";
    private string _r3Text = "";
    private bool _isRecentlyMoved;
    private DispatcherTimer? _moveFlashTimer;

    public SrcQueueItemVM(string filePath, ICommand? removeCmd, ICommand? moveUpCmd, ICommand? moveDownCmd)
    {
        FilePath = filePath;
        Name = Path.GetFileName(filePath);
        OnlyPath = Path.GetDirectoryName(filePath) ?? filePath;
        SizeBytes = GetFileSize(filePath);
        P1Text = SizeBytes >= 0
            ? $"{FormatFileSize(SizeBytes)} | {OnlyPath}"
            : OnlyPath;
        R1Command = removeCmd;
        R2Command = moveUpCmd;
        R3Command = moveDownCmd;
        RefreshLanguage();
    }

    public string FilePath { get; }
    public long SizeBytes { get; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string OnlyPath
    {
        get => _fullPath;
        set => SetProperty(ref _fullPath, value);
    }

    public string P1Text
    {
        get => _pathText;
        set => SetProperty(ref _pathText, value);
    }

    public string DisplayR1Text
    {
        get => _displayR1Text;
        set => SetProperty(ref _displayR1Text, value);
    }

    public string R2Text
    {
        get => _r2Text;
        set => SetProperty(ref _r2Text, value);
    }

    public string R3Text
    {
        get => _r3Text;
        set => SetProperty(ref _r3Text, value);
    }
    public bool R1IsEnabled => _canRemove;
    public bool R2IsEnabled => _canMoveUp;
    public bool R3IsEnabled => _canMoveDown;
    public static bool IsCancel => false;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool IsRecentlyMoved
    {
        get => _isRecentlyMoved;
        private set => SetProperty(ref _isRecentlyMoved, value);
    }

    public bool CanMoveUp
    {
        get => _canMoveUp;
        set
        {
            if (SetProperty(ref _canMoveUp, value))
            {
                OnPropertyChanged(nameof(R2IsEnabled));
            }
        }
    }

    public bool CanMoveDown
    {
        get => _canMoveDown;
        set
        {
            if (SetProperty(ref _canMoveDown, value))
            {
                OnPropertyChanged(nameof(R3IsEnabled));
            }
        }
    }

    public bool CanRemove
    {
        get => _canRemove;
        set
        {
            if (SetProperty(ref _canRemove, value))
            {
                OnPropertyChanged(nameof(R1IsEnabled));
            }
        }
    }

    public ICommand? R1Command { get; }
    public ICommand? R2Command { get; }
    public ICommand? R3Command { get; }

    public void RefreshLanguage()
    {
        DisplayR1Text = LangProviderBase.RemoveText;
        R2Text = LangProviderBase.MoveUpText;
        R3Text = LangProviderBase.MoveDownText;
    }

    public void FlashMovedHighlight()
    {
        IsRecentlyMoved = true;
        StopMoveFlashTimer();
        _moveFlashTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(600), DispatcherPriority.Normal, OnMoveFlashTimerTick, Dispatcher.CurrentDispatcher);
        _moveFlashTimer.Start();
    }

    private void OnMoveFlashTimerTick(object? sender, EventArgs e)
    {
        IsRecentlyMoved = false;
        StopMoveFlashTimer();
    }

    private void StopMoveFlashTimer()
    {
        if (_moveFlashTimer == null) return;

        _moveFlashTimer.Stop();
        _moveFlashTimer.Tick -= OnMoveFlashTimerTick;
        _moveFlashTimer = null;
    }

    private static long GetFileSize(string filePath)
    {
        try { return new FileInfo(filePath).Length; }
        catch { return -1; }
    }

    private static string FormatFileSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        int order = 0;
        while (value >= 1024 && order < units.Length - 1)
        {
            order++;
            value /= 1024;
        }
        return $"{value:0.##} {units[order]}";
    }

    public override void Dispose()
    {
        StopMoveFlashTimer();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
