namespace OneColumnEncoder.ViewModels.Cards;

public class CPUNodeCardVM : BaseVM
{
    // Section 1: Text within card
    private int _nodeId;
    public int NodeId
    {
        get => _nodeId;
        set
        {
            if (!SetProperty(ref _nodeId, value)) return;
            OnPropertyChanged(nameof(NodeLabel));
        }
    }

    private int _groupId;
    public int GroupId
    {
        get => _groupId;
        set
        {
            if (!SetProperty(ref _groupId, value)) return;
            OnPropertyChanged(nameof(NodeLabel));
        }
    }

    public string NodeLabel => IsEnabled ? $"Node {NodeId} · Group {GroupId}" : $"Node {NodeId} · N/A";

    // Section 2: small gray text under the card
    private int _minThreadNum;
    public int MinThreadNum
    {
        get => _minThreadNum;
        set
        {
            if (!SetProperty(ref _minThreadNum, value)) return;
            OnPropertyChanged(nameof(ResourceLabel));
        }
    }
    private int _maxThreadNum;
    public int MaxThreadNum
    {
        get => _maxThreadNum;
        set
        {
            if (!SetProperty(ref _maxThreadNum, value)) return;
            OnPropertyChanged(nameof(ResourceLabel));
        }
    }
    private int _hasMemGB;
    public int HasMemGB
    {
        get => _hasMemGB;
        set
        {
            if (!SetProperty(ref _hasMemGB, value)) return;
            OnPropertyChanged(nameof(ResourceLabel));
        }
    }

    public string ResourceLabel => IsEnabled ? $"T{MinThreadNum}-{MaxThreadNum} · {HasMemGB}GB" : "N/A";

    public ICommand? SelectCommand { get; set; }

    // Card selection or disabling (no NUMA node on this range)
    private bool _isSelected = false;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
    private bool _isEnabled = false;
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (!SetProperty(ref _isEnabled, value)) return;
            OnPropertyChanged(nameof(NodeLabel));
            OnPropertyChanged(nameof(ResourceLabel));
        }
    }

}
