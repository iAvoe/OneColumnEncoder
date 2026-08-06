using System.IO;

namespace OneColumnEncoder.ViewModels;

public enum RepartSourceIndexState
{
    Idle,
    Loading,
    Ready,
    Failed
}

public sealed class RepartSourceItemVM : BaseVM
{
    private RepartSourceIndexState _indexState = RepartSourceIndexState.Idle;

    public RepartSourceItemVM(string filePath, long firstFrame, long lastFrame)
    {
        FilePath = filePath;
        FirstFrame = firstFrame;
        LastFrame = lastFrame;
    }

    public string FilePath { get; }
    public long FirstFrame { get; }
    public long LastFrame { get; }
    public string Name => Path.GetFileName(FilePath);
    public string P1Text => LastFrame >= FirstFrame ? $"{FirstFrame:N0} - {LastFrame:N0}" : FilePath;

    public RepartSourceIndexState IndexState
    {
        get => _indexState;
        private set => SetProperty(ref _indexState, value);
    }

    public void SetIndexState(RepartSourceIndexState state) => IndexState = state;
}
