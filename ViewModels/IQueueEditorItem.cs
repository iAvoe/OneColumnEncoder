namespace OneColumnEncoder.ViewModels;

/// <summary>
/// Common presentation contract for source and Repart-output queue entries.
/// </summary>
public interface IQueueEditorItem : IDisposable
{
    string Name { get; }
    string P1Text { get; }
    string DisplayR1Text { get; }
    string R2Text { get; }
    string R3Text { get; }
    bool R1IsEnabled { get; }
    bool R2IsEnabled { get; }
    bool R3IsEnabled { get; }
    bool IsCancel { get; }
    bool IsSelected { get; set; }
    bool IsRecentlyMoved { get; }
    bool CanMoveUp { get; set; }
    bool CanMoveDown { get; set; }
    bool CanRemove { get; set; }
    long SortSize { get; }
    string SortName { get; }
    Guid? OutputId { get; }
    ICommand? R1Command { get; set; }
    ICommand? R2Command { get; set; }
    ICommand? R3Command { get; set; }
    void FlashMovedHighlight();
    void RefreshLanguage();
}
