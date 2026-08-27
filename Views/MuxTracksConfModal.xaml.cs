using System.Collections.Specialized;
using OneColumnEncoder.ViewModels.MuxTracks;

namespace OneColumnEncoder.Views;

public partial class MuxTracksConfModal : AdaptiveWindow
{
    private MuxTracksConfVM? _viewModel;
    private bool _heightRecalcQueued;

    public MuxTracksConfModal()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MuxTracksConfVM viewModel)
        {
            WindowState = WindowState.Normal;
            Width = viewModel.ShowSidebar ? 760 : 540;
            HookViewModel(viewModel);
        }

        QueueHeightRecalculation();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => UnhookViewModel();

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        UnhookViewModel();
        if (e.NewValue is MuxTracksConfVM viewModel && IsLoaded)
            HookViewModel(viewModel);

        QueueHeightRecalculation();
    }

    private void HookViewModel(MuxTracksConfVM viewModel)
    {
        if (ReferenceEquals(_viewModel, viewModel)) return;

        _viewModel = viewModel;
        _viewModel.Tracks.CollectionChanged += Tracks_CollectionChanged;
    }

    private void UnhookViewModel()
    {
        if (_viewModel == null) return;

        _viewModel.Tracks.CollectionChanged -= Tracks_CollectionChanged;
        _viewModel = null;
    }

    private void Tracks_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        QueueHeightRecalculation();
    }

    private void QueueHeightRecalculation()
    {
        if (_heightRecalcQueued) return;

        _heightRecalcQueued = true;
        Dispatcher.BeginInvoke(new Action(() =>
        {
            _heightRecalcQueued = false;
            if (!IsLoaded) return;

            InvalidateMeasure();
            UpdateLayout();
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }
}
