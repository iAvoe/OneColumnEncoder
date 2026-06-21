using OneColumnEncoder.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Threading;

namespace OneColumnEncoder.Components
{
    public partial class ImageABPreviewViewer : UserControl
    {
        private bool _isPanning;
        private bool _isDraggingSplit;
        private Point _lastPanPoint;
        private double _splitRatio = 0.5d;
        private double _zoom = 1d;
        private double _offsetX;
        private double _offsetY;
        private ImageABPreviewVM? _subscribedVm;

        public ImageABPreviewViewer()
        {
            InitializeComponent();
            Loaded += (_, _) => ApplyView();
            DataContextChanged += OnDataContextChanged;
            Unloaded += (_, _) => UnsubscribeViewModel();
        }

        public void ZoomIn() => SetZoom(_zoom + 0.01d, GetViewportCenter());
        public void ZoomOut() => SetZoom(_zoom - 0.01d, GetViewportCenter());
        public void ZoomFineIn() => SetZoom(_zoom + 0.1d, GetViewportCenter());
        public void ZoomFineOut() => SetZoom(_zoom - 0.1d, GetViewportCenter());
        public void Fit() => FitImage();
        public void SetActualSize() => SetZoom(1d, GetViewportCenter());
        public void SetDoubleSize() => SetZoom(2d, GetViewportCenter());

        private ImageABPreviewVM? ViewModel => DataContext as ImageABPreviewVM;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UnsubscribeViewModel();

            _subscribedVm = e.NewValue as ImageABPreviewVM;
            if (_subscribedVm != null)
                _subscribedVm.PropertyChanged += OnViewModelPropertyChanged;

            ApplyView();
        }

        private void UnsubscribeViewModel()
        {
            if (_subscribedVm != null)
                _subscribedVm.PropertyChanged -= OnViewModelPropertyChanged;
            _subscribedVm = null;
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ImageABPreviewVM.SourceImage) or nameof(ImageABPreviewVM.EncodedImage))
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (ViewModel?.IsFitMode == true)
                        FitImage();
                    else
                        ApplyView();
                }, DispatcherPriority.Loaded);
            }
        }

        private void Viewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSplitVisual();
            if (ViewModel?.IsFitMode == true)
                FitImage();
            else
                ApplyView();
        }

        private void Viewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ViewModel?.IsBusy == true) return;
            Point mousePoint = e.GetPosition(Viewport);
            SetZoom(_zoom * (e.Delta > 0 ? 1.15d : 1d / 1.15d), mousePoint);
            e.Handled = true;
        }

        private void Viewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingSplit) return;
            Focus();
            _isPanning = true;
            _lastPanPoint = e.GetPosition(Viewport);
            Cursor = Cursors.SizeAll;
            CaptureMouse();
        }

        private void Viewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isPanning) return;
            _isPanning = false;
            Cursor = Cursors.Arrow;
            ReleaseMouseCapture();
        }

        private void Viewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isPanning) return;
            Point current = e.GetPosition(Viewport);
            _offsetX += current.X - _lastPanPoint.X;
            _offsetY += current.Y - _lastPanPoint.Y;
            _lastPanPoint = current;
            ViewModel?.SetFitMode(false);
            ApplyView();
        }

        private void SplitHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingSplit = true;
            SplitHandle.CaptureMouse();
            e.Handled = true;
        }

        private void SplitHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDraggingSplit) return;
            _isDraggingSplit = false;
            SplitHandle.ReleaseMouseCapture();
            e.Handled = true;
        }

        private void SplitHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingSplit) return;
            Point point = e.GetPosition(Viewport);
            double imageWidth = GetImagePixelWidth() * _zoom;
            if (imageWidth > 0d)
                _splitRatio = Math.Max(0d, Math.Min(1d, (point.X - _offsetX) / imageWidth));
            else
                _splitRatio = Math.Max(0d, Math.Min(1d, point.X / Math.Max(1d, Viewport.ActualWidth)));
            UpdateSplitVisual();
            e.Handled = true;
        }

        private void SetZoom(double zoom, Point anchor)
        {
            zoom = Math.Max(0.05d, Math.Min(16d, zoom));
            if (Math.Abs(zoom - _zoom) < 0.0001d) return;

            double imageX = (anchor.X - _offsetX) / _zoom;
            double imageY = (anchor.Y - _offsetY) / _zoom;
            _zoom = zoom;
            _offsetX = anchor.X - imageX * _zoom;
            _offsetY = anchor.Y - imageY * _zoom;
            ViewModel?.SetFitMode(false);
            ApplyView();
        }

        private void FitImage()
        {
            BitmapSource? bitmap = SourceImage.Source as BitmapSource ?? EncodedImage.Source as BitmapSource;
            double width = Viewport.ActualWidth;
            double height = Viewport.ActualHeight;
            if (bitmap == null || width <= 0 || height <= 0)
            {
                ApplyView();
                return;
            }

            double fitZoom = Math.Min(width / bitmap.PixelWidth, height / bitmap.PixelHeight);
            if (double.IsNaN(fitZoom) || double.IsInfinity(fitZoom) || fitZoom <= 0)
                fitZoom = 1d;

            _zoom = Math.Max(0.01d, fitZoom);
            _offsetX = (width - bitmap.PixelWidth * _zoom) / 2d;
            _offsetY = (height - bitmap.PixelHeight * _zoom) / 2d;
            ViewModel?.SetFitMode(true);
            ApplyView();
        }

        private Point GetViewportCenter() =>
            new(Math.Max(0d, Viewport.ActualWidth) / 2d, Math.Max(0d, Viewport.ActualHeight) / 2d);

        private void ApplyView()
        {
            SyncImageLayoutSize(SourceImage);
            SyncImageLayoutSize(EncodedImage);
            CenterImageIfSmallerThanViewport();
            SourceScale.ScaleX = _zoom;
            SourceScale.ScaleY = _zoom;
            EncodedScale.ScaleX = _zoom;
            EncodedScale.ScaleY = _zoom;
            SourceTranslate.X = _offsetX;
            SourceTranslate.Y = _offsetY;
            EncodedTranslate.X = _offsetX;
            EncodedTranslate.Y = _offsetY;
            ViewModel?.SetZoomPercent((int)Math.Round(_zoom * 100d));
            UpdateSplitVisual();
        }

        private static void SyncImageLayoutSize(Image image)
        {
            if (image.Source is not BitmapSource bitmap) return;
            image.Width = bitmap.PixelWidth;
            image.Height = bitmap.PixelHeight;
        }

        private void CenterImageIfSmallerThanViewport()
        {
            double imageWidth = GetImagePixelWidth() * _zoom;
            double imageHeight = GetImagePixelHeight() * _zoom;
            double viewportWidth = Viewport.ActualWidth;
            double viewportHeight = Viewport.ActualHeight;

            if (imageWidth > 0d && viewportWidth > 0d && imageWidth <= viewportWidth)
                _offsetX = (viewportWidth - imageWidth) / 2d;

            if (imageHeight > 0d && viewportHeight > 0d && imageHeight <= viewportHeight)
                _offsetY = (viewportHeight - imageHeight) / 2d;
        }

        private double GetImagePixelWidth() =>
            (SourceImage.Source as BitmapSource ?? EncodedImage.Source as BitmapSource)?.PixelWidth ?? 0d;

        private double GetImagePixelHeight() =>
            (SourceImage.Source as BitmapSource ?? EncodedImage.Source as BitmapSource)?.PixelHeight ?? 0d;

        private void UpdateSplitVisual()
        {
            double viewportWidth = Math.Max(0d, Viewport.ActualWidth);
            double imageWidth = GetImagePixelWidth() * _zoom;
            double splitX = imageWidth > 0d
                ? _offsetX + imageWidth * _splitRatio
                : viewportWidth * _splitRatio;
            splitX = Math.Max(0d, Math.Min(viewportWidth, splitX));
            SourceLayer.Width = splitX;
            SplitHandle.Height = Math.Max(0d, Viewport.ActualHeight);
            SplitHandle.Margin = new Thickness(Math.Max(0d, splitX - SplitHandle.Width / 2d), 0, 0, 0);
        }
    }
}
