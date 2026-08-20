namespace OneColumnEncoder.ViewModels.Cards;

public class ValidationCardBaseVM : BaseVM
{
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _p1Name = string.Empty;
    public string P1Name
    {
        get => _p1Name;
        set => SetProperty(ref _p1Name, value);
    }

    private string _p3Name = string.Empty;
    public string P3Name
    {
        get => _p3Name;
        set => SetProperty(ref _p3Name, value);
    }

    private string _subtitle = string.Empty;
    public string Subtitle
    {
        get => _subtitle;
        set => SetProperty(ref _subtitle, value);
    }

    // P2, P4
    public ObservableCollection<ChecklistEntryVM> Checklist1 { get; } = [];
    public ObservableCollection<ChecklistEntryVM> Checklist2 { get; } = [];

    public ICommand? InspectColumn1Cmd { get; set; }
    public ICommand? InspectColumn2Cmd { get; set; }

    protected static void RefreshChecklist(ObservableCollection<ChecklistEntryVM> collection, List<ChecklistItemDefinitionM> definitions)
    {
        for (int i = 0; i < definitions.Count && i < collection.Count; i++)
            collection[i].Text = definitions[i].Text;
    }
}
