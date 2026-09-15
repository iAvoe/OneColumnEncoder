namespace OneColumnEncoder.Views;

public partial class VpyPreviewerDialog : AdaptiveWindow
{
    public VpyPreviewerDialog(VpyPreviewerVM vm, ModalNavS modalNavS, Window? owner = null)
    {
        InitializeComponent();
        DataContext = vm;
        Owner = owner ?? Application.Current.MainWindow;

        Closed += (_, _) =>
        {
            vm.Dispose();
            modalNavS.Close();
        };

        Loaded += (_, _) =>
        {
            modalNavS.CurrentModalVM = vm;
        };
    }
}
