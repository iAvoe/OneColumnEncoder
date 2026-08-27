using System.Collections.Specialized;
using OneColumnEncoder.ViewModels.MuxTracks;

namespace OneColumnEncoder.Views;

public partial class MuxTracksConfModal : AdaptiveWindow
{
    private MuxTracksConfVM? _viewModel;
    private bool _sizeRecalcQueued;

    public MuxTracksConfModal()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Normal;

        if (DataContext is MuxTracksConfVM viewModel)
            HookViewModel(viewModel);

        QueueSizeRecalculation();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => UnhookViewModel();

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        UnhookViewModel();
        if (e.NewValue is MuxTracksConfVM viewModel && IsLoaded)
            HookViewModel(viewModel);

        QueueSizeRecalculation();
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
        QueueSizeRecalculation();
    }

    private void QueueSizeRecalculation()
    {
        if (_sizeRecalcQueued) return;

        _sizeRecalcQueued = true;
        Dispatcher.BeginInvoke(new Action(() =>
        {
            _sizeRecalcQueued = false;
            if (!IsLoaded) return;

            if (_viewModel != null)
                Width = _viewModel.ShowSidebar ? 760 : 540;

            InvalidateMeasure();
            UpdateLayout();
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }
}
