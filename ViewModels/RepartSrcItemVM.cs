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

    public string FilePath { get; } = filePath;
    public long FirstFrame { get; } = firstFrame;
    public long LastFrame { get; } = lastFrame;
    public string Name => Path.GetFileName(FilePath);
    public string P1Text => LastFrame >= FirstFrame ? $"{FirstFrame:N0} - {LastFrame:N0}" : FilePath;

    public RepartSourceIndexState IndexState
    {
        get => _indexState;
        private set => SetProperty(ref _indexState, value);
    }

    public void SetIndexState(RepartSourceIndexState state) => IndexState = state;
}
