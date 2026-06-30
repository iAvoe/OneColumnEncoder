using OneColumnEncoder.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace OneColumnEncoder.Components
{
    public partial class HintPanel : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(HintPanel),
                new PropertyMetadata(null));

        // Opt in only for hints that need live language refresh.
        public static readonly DependencyProperty RefreshTextOnLanguageChangedProperty =
            DependencyProperty.Register(nameof(RefreshTextOnLanguageChanged), typeof(bool), typeof(HintPanel),
                new PropertyMetadata(false, OnRefreshTextOnLanguageChangedChanged));

        public static readonly DependencyProperty HintBrushProperty =
            DependencyProperty.Register(nameof(HintBrush), typeof(Brush), typeof(HintPanel),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public bool RefreshTextOnLanguageChanged
        {
            get => (bool)GetValue(RefreshTextOnLanguageChangedProperty);
            set => SetValue(RefreshTextOnLanguageChangedProperty, value);
        }

        public Brush HintBrush
        {
            get => (Brush)GetValue(HintBrushProperty);
            set => SetValue(HintBrushProperty, value);
        }

        public HintPanel()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            FontSize = 10d;
            if (ReadLocalValue(HintBrushProperty) == DependencyProperty.UnsetValue)
                SetCurrentValue(HintBrushProperty, TryFindResource("GlobalTertiary") as Brush);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SyncLanguageChangedSubscription();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
        }

        private static void OnRefreshTextOnLanguageChangedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HintPanel hintPanel = (HintPanel)d;
            hintPanel.SyncLanguageChangedSubscription();
        }

        // Keep the language subscription aligned with the opt-in flag.
        private void SyncLanguageChangedSubscription()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            if (RefreshTextOnLanguageChanged)
                UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        // Rebind the text so the localized value is pulled again.
        private void OnLanguageChanged()
        {
            BindingExpression? binding = GetBindingExpression(TextProperty);
            binding?.UpdateTarget();
        }

        private void CopyText_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Text))
                Clipboard.SetText(Text);
        }
    }
}
