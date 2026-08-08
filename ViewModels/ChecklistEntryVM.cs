namespace OneColumnEncoder.ViewModels;

public class ChecklistEntryVM : BaseVM
{
    private string _text = string.Empty;
    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    private StatusType _status = StatusType.Waiting;
    public StatusType Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    private bool _isEnabled = true;
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }
}
