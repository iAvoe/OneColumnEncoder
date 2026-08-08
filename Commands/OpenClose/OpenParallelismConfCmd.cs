namespace OneColumnEncoder.Commands.OpenClose;

public class OpenParallelismConfCmd(ModalNavS modalNavS, ToolItemCardVM targetItem) : BaseCmd
{
    private readonly ModalNavS _modalNavS = modalNavS;
    private readonly ToolItemCardVM _targetItem = targetItem;

    public override void Execute(object? parameter)
    {
        var existingWindow = Application.Current.Windows
            .OfType<ParallelismConfModal>()
            .FirstOrDefault();

        if (existingWindow != null)
        {
            existingWindow.Activate();
            return;
        }

        if (_modalNavS.IsOpen)
            _modalNavS.Close();

        ParallelismConfModal window = new();
        ParallelismConfVM vm = new(window.Close, _targetItem);
        window.DataContext = vm;
        window.Owner = Application.Current.MainWindow;
        window.Closed += (_, _) => _modalNavS.Close();
        _modalNavS.CurrentModalVM = vm;
        window.Show();
    }
}
