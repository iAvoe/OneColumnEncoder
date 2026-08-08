namespace OneColumnEncoder.Views
{
    /// <summary>
    /// Interaction logic for WarnErrModal.xaml
    /// </summary>
    public partial class ConfirmationModal : AdaptiveWindow
    {
        public ConfirmationModal()
        {
            InitializeComponent();
        }

        private void CopyMessage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext is ConfirmationVM vm
                && !string.IsNullOrWhiteSpace(vm.P1Text);
        }

        private void CopyMessage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is ConfirmationVM vm)
                Clipboard.SetText(vm.P1Text);
        }
    }
}
