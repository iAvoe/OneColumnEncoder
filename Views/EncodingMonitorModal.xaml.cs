using OneColumnEncoder.Helpers;
using OneColumnEncoder.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace OneColumnEncoder.Views
{
    public partial class EncodingMonitorModal : AdaptiveWindow
    {
        private const int SidebarWidth = 290;
        private const int DefaultWidth = 1000;
        private const int MinWidthDefault = 860;
        private const int MinWidthSidebar = 860 + SidebarWidth;

        public EncodingMonitorModal()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is EncodingMonitorVM vm)
            {
                vm.QueueSidebar.PropertyChanged += OnQueueSidebarPropertyChanged;
                if (vm.QueueSidebar.IsVisible)
                    ExpandForSidebar();
                vm.Start();
            }
        }

        private void OnQueueSidebarPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(QueueSidebarVM.IsVisible))
            {
                if (sender is QueueSidebarVM sidebar)
                {
                    if (sidebar.IsVisible)
                        ExpandForSidebar();
                    else
                        CollapseSidebar();
                }
            }
        }

        private void ExpandForSidebar()
        {
            MinWidth = MinWidthSidebar;
            if (Width < MinWidthSidebar)
                Width = MinWidthSidebar;
        }

        private void CollapseSidebar()
        {
            MinWidth = MinWidthDefault;
            if (Width > DefaultWidth)
                Width = DefaultWidth;
        }
    }
}
