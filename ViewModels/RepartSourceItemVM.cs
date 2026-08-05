using System.IO;

namespace OneColumnEncoder.ViewModels;

public sealed class RepartSourceItemVM : BaseVM
{
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
}
