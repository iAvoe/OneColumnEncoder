namespace OneColumnEncoder.Views
{
    public partial class VpyPreviewDialog : AdaptiveWindow
    {
        public VpyPreviewDialog(VspipePreviewVM vm, ModalNavS modalNavS, Window? owner = null)
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
}
