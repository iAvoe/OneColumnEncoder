using System.IO;

namespace OneColumnEncoder.ViewModels;

public enum RepartSourceIndexState
{
    Idle,
    Loading,
    Ready,
    Failed
}

public sealed class RepartSrcItemVM(string filePath, long firstFrame, long lastFrame) : BaseVM
{
    private RepartSourceIndexState _indexState = RepartSourceIndexState.Idle;
    private bool _canMoveUp;
    private bool _canMoveDown;
    private bool _canRemove;

    public string FilePath { get; } = filePath;
    public long FirstFrame { get; } = firstFrame;
    public long LastFrame { get; } = lastFrame;
    public string Name => Path.GetFileName(FilePath);
    public string P1Text => LastFrame >= FirstFrame ? $"{FirstFrame:N0} - {LastFrame:N0}" : FilePath;
    public long FrameCount => LastFrame >= FirstFrame ? LastFrame - FirstFrame + 1 : 0;
    public bool CanMoveUp
    {
        get => _canMoveUp;
        set => SetProperty(ref _canMoveUp, value);
    }
    public bool CanMoveDown
    {
        get => _canMoveDown;
        set => SetProperty(ref _canMoveDown, value);
    }
    public bool CanRemove
    {
        get => _canRemove;
        set => SetProperty(ref _canRemove, value);
    }

    public RepartSourceIndexState IndexState
    {
        get => _indexState;
        private set => SetProperty(ref _indexState, value);
    }

    public void SetIndexState(RepartSourceIndexState state) => IndexState = state;
}
