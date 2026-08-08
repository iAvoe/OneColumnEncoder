namespace OneColumnEncoder.ViewModels;

public class ValidationActionGroupVM : BaseVM
{
    private readonly Action<bool> _saveMiniState;
    private ValidationCardBaseVM _card;
    private bool _isMini;

    public ValidationActionGroupVM(
        ValidationCardBaseVM card,
        bool isMini,
        Action<bool> saveMiniState)
    {
        _card = card;
        _isMini = isMini;
        _saveMiniState = saveMiniState;
        ToggleMiniCommand = new ActionCmd(_ =>
        {
            IsMini = !IsMini;
            _saveMiniState(IsMini);
        });
    }

    public ValidationCardBaseVM Card
    {
        get => _card;
        set => SetProperty(ref _card, value);
    }

    public ActionCmd ToggleMiniCommand { get; }

    public bool IsMini
    {
        get => _isMini;
        set
        {
            if (!SetProperty(ref _isMini, value)) return;
            OnPropertyChanged(nameof(ToggleMiniText));
        }
    }

    public string ToggleMiniText => IsMini
        ? UILangProvider.Current["Expand"]
        : UILangProvider.Current["Collapse"];

    public void RefreshLanguage() => OnPropertyChanged(nameof(ToggleMiniText));
}
