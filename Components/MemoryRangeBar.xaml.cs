using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace OneColumnEncoder.Components
{
    public partial class MemoryRangeBar : UserControl
    {
        private const double DefaultBlockWidth = 5.25d;
        private const double BlockHorizontalMargin = 0.68d;

        private INotifyCollectionChanged? _itemsCollectionChanged;

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(IEnumerable), typeof(MemoryRangeBar), new PropertyMetadata(null, OnItemsChanged));

        public static readonly DependencyProperty BlockWidthProperty =
            DependencyProperty.Register(nameof(BlockWidth), typeof(double), typeof(MemoryRangeBar), new PropertyMetadata(DefaultBlockWidth));

        public IEnumerable? Items
        {
            get => (IEnumerable?)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public double BlockWidth
        {
            get => (double)GetValue(BlockWidthProperty);
            private set => SetValue(BlockWidthProperty, value);
        }

        public MemoryRangeBar()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SizeChanged += OnSizeChanged;
        }

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MemoryRangeBar bar = (MemoryRangeBar)d;
            bar.UnhookItems();
            bar.HookItems(e.NewValue as IEnumerable);
            bar.UpdateBlockWidth();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            HookItems(Items);
            Dispatcher.BeginInvoke(UpdateBlockWidth, DispatcherPriority.Loaded);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            UnhookItems();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBlockWidth();
        }

        private void HookItems(IEnumerable? items)
        {
            if (items is not INotifyCollectionChanged notify || ReferenceEquals(_itemsCollectionChanged, notify))
                return;

            UnhookItems();
            _itemsCollectionChanged = notify;
            notify.CollectionChanged += OnItemsCollectionChanged;
        }

        private void UnhookItems()
        {
            if (_itemsCollectionChanged == null) return;
            _itemsCollectionChanged.CollectionChanged -= OnItemsCollectionChanged;
            _itemsCollectionChanged = null;
        }

        private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateBlockWidth();
        }

        private void UpdateBlockWidth()
        {
            int itemCount = GetItemCount();
            double availableWidth = ActualWidth;
            if (itemCount <= 0 || availableWidth <= 0 || double.IsNaN(availableWidth) || double.IsInfinity(availableWidth))
            {
                BlockWidth = DefaultBlockWidth;
                return;
            }

            double totalMargins = itemCount * BlockHorizontalMargin * 2d;
            BlockWidth = Math.Max(1d, (availableWidth - totalMargins) / itemCount);
        }

        private int GetItemCount()
        {
            if (Items is ICollection collection)
                return collection.Count;

            return Items?.Cast<object>().Count() ?? 0;
        }
    }
}
