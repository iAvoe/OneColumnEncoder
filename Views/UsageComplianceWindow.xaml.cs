using OneColumnEncoder.Commands;
using OneColumnEncoder.ViewModels;
using System.Windows;

namespace OneColumnEncoder.Views
{
    public partial class UsageComplianceWindow : Window
    {
        public UsageComplianceWindow()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is UsageComplianceVM vm)
            {
                vm.CloseCmd = new CloseWindowCmd(this);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class CloseWindowCmd(Window window) : BaseCmd
    {
        public override void Execute(object? parameter) => window.Close();
    }
}