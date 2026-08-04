using System.IO;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels;

public sealed class RepartSourceItemVM : BaseVM
{
    private bool _isSelected;
    private bool _isRecentlyMoved;

    public RepartSourceItemVM(
        string filePath,
        long firstFrame,
        long lastFrame,
        ICommand removeCommand,
        ICommand moveUpCommand,
        ICommand moveDownCommand)
    {
        FilePath = filePath;
        FirstFrame = firstFrame;
        LastFrame = lastFrame;
        R1Command = removeCommand;
        R2Command = moveUpCommand;
        R3Command = moveDownCommand;
    }

    public string FilePath { get; }
    public long FirstFrame { get; }
    public long LastFrame { get; }
    public string Name => Path.GetFileName(FilePath);
    public string P1Text => LastFrame >= FirstFrame ? $"{FirstFrame:N0} - {LastFrame:N0}" : FilePath;
    public string DisplayR1Text => RepartLangProvider.Current["Remove"];
    public string R2Text => RepartLangProvider.Current["MoveUp"];
    public string R3Text => RepartLangProvider.Current["MoveDown"];
    public bool R1IsEnabled => true;
    public bool R2IsEnabled { get; set; }
    public bool R3IsEnabled { get; set; }
    public ICommand R1Command { get; }
    public ICommand R2Command { get; }
    public ICommand R3Command { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool IsRecentlyMoved
    {
        get => _isRecentlyMoved;
        set => SetProperty(ref _isRecentlyMoved, value);
    }
}
