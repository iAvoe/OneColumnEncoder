using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace OneColumnEncoder.Components.Cards
{
    public partial class SplitTextCard : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(SplitTextCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LeftTextProperty =
            DependencyProperty.Register(
                nameof(LeftText),
                typeof(string),
                typeof(SplitTextCard),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty RightTextProperty =
            DependencyProperty.Register(
                nameof(RightText),
                typeof(string),
                typeof(SplitTextCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(SplitTextCard), new PropertyMetadata(true));

        public static readonly DependencyProperty TextFontSizeProperty =
            DependencyProperty.Register(nameof(TextFontSize), typeof(double), typeof(SplitTextCard), new PropertyMetadata(12.0));

        public static readonly DependencyProperty IsRichTextModeProperty =
            DependencyProperty.Register(nameof(IsRichTextMode), typeof(bool), typeof(SplitTextCard), new PropertyMetadata(false, OnIsRichTextModeChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string LeftText
        {
            get => (string)GetValue(LeftTextProperty);
            set => SetValue(LeftTextProperty, value);
        }

        public string RightText
        {
            get => (string)GetValue(RightTextProperty);
            set => SetValue(RightTextProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public double TextFontSize
        {
            get => (double)GetValue(TextFontSizeProperty);
            set => SetValue(TextFontSizeProperty, value);
        }

        public bool IsRichTextMode
        {
            get => (bool)GetValue(IsRichTextModeProperty);
            set => SetValue(IsRichTextModeProperty, value);
        }

        private ScrollViewer? _leftScrollViewer;
        private ScrollViewer? _rightScrollViewer;
        private ScrollViewer? _leftRichScrollViewer;
        private ScrollViewer? _rightRichScrollViewer;

        public SplitTextCard()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private static void OnIsRichTextModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SplitTextCard card)
                card.RefreshRichTextDocuments();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _leftScrollViewer = FindScrollViewer(LeftTextBox);
            _rightScrollViewer = FindScrollViewer(RightTextBox);
            _leftRichScrollViewer = FindScrollViewer(LeftRichTextBox);
            _rightRichScrollViewer = FindScrollViewer(RightRichTextBox);
            RefreshRichTextDocuments();
        }

        private static ScrollViewer? FindScrollViewer(DependencyObject visual)
        {
            if (visual is ScrollViewer sv) return sv;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                ScrollViewer? child = FindScrollViewer(VisualTreeHelper.GetChild(visual, i));
                if (child != null) return child;
            }
            return null;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            ScrollViewer? sv = textBox == LeftTextBox ? _leftScrollViewer : _rightScrollViewer;
            if (sv == null) return;

            bool isAtBottom = Math.Abs(sv.VerticalOffset + sv.ViewportHeight - sv.ExtentHeight) < 1d;
            if (isAtBottom)
            {
                Dispatcher.BeginInvoke(sv.ScrollToEnd, System.Windows.Threading.DispatcherPriority.Background);
            }

            if (IsRichTextMode)
                RefreshRichTextDocuments();
        }

        private void RefreshRichTextDocuments()
        {
            if (!IsRichTextMode) return;

            SetRichTextDocument(LeftRichTextBox, LeftText);
            SetRichTextDocument(RightRichTextBox, RightText);
            ScrollRichTextToEndIfNeeded(LeftRichTextBox, _leftRichScrollViewer);
            ScrollRichTextToEndIfNeeded(RightRichTextBox, _rightRichScrollViewer);
        }

        private static void ScrollRichTextToEndIfNeeded(RichTextBox richTextBox, ScrollViewer? scrollViewer)
        {
            if (scrollViewer == null) return;

            bool isAtBottom = Math.Abs(scrollViewer.VerticalOffset + scrollViewer.ViewportHeight - scrollViewer.ExtentHeight) < 1d;
            if (isAtBottom)
                richTextBox.Dispatcher.BeginInvoke(scrollViewer.ScrollToEnd, System.Windows.Threading.DispatcherPriority.Background);
        }

        private static void SetRichTextDocument(RichTextBox richTextBox, string text)
        {
            FlowDocument document = new()
            {
                PagePadding = new Thickness(0),
                ColumnWidth = double.PositiveInfinity,
                FontFamily = new FontFamily("Consolas"),
                FontSize = richTextBox.FontSize,
                Foreground = richTextBox.Foreground,
                Background = Brushes.Transparent
            };

            Paragraph paragraph = new(new Run())
            {
                Margin = new Thickness(0)
            };

            foreach (Inline inline in BuildAnsiRuns(text, richTextBox.Foreground))
                paragraph.Inlines.Add(inline);

            document.Blocks.Add(paragraph);
            richTextBox.Document = document;
        }

        private static IEnumerable<Inline> BuildAnsiRuns(string text, Brush defaultBrush)
        {
            const string ansiPattern = "\\x1B\\[(?<code>[0-9;]*)m";
            Regex regex = new(ansiPattern, RegexOptions.CultureInvariant);

            Brush currentBrush = defaultBrush;
            int lastIndex = 0;
            foreach (Match match in regex.Matches(text))
            {
                if (match.Index > lastIndex)
                    yield return new Run(text[lastIndex..match.Index]) { Foreground = currentBrush };

                currentBrush = ResolveAnsiBrush(match.Groups["code"].Value, defaultBrush, currentBrush);
                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < text.Length)
                yield return new Run(text[lastIndex..]) { Foreground = currentBrush };
        }

        private static Brush ResolveAnsiBrush(string codeText, Brush defaultBrush, Brush currentBrush)
        {
            if (string.IsNullOrWhiteSpace(codeText)) return defaultBrush;

            string[] codes = codeText.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (codes.Length == 0) return defaultBrush;

            Brush brush = currentBrush;
            foreach (string code in codes)
            {
                switch (code)
                {
                    case "0":
                        brush = defaultBrush;
                        break;
                    case "30": brush = Brushes.Black; break;
                    case "31": brush = Brushes.Red; break;
                    case "32": brush = Brushes.LimeGreen; break;
                    case "33": brush = Brushes.Gold; break;
                    case "34": brush = Brushes.DodgerBlue; break;
                    case "35": brush = Brushes.Magenta; break;
                    case "36": brush = Brushes.Cyan; break;
                    case "37": brush = Brushes.Gainsboro; break;
                    case "90": brush = Brushes.DarkGray; break;
                    case "91": brush = Brushes.IndianRed; break;
                    case "92": brush = Brushes.LightGreen; break;
                    case "93": brush = Brushes.Khaki; break;
                    case "94": brush = Brushes.LightSkyBlue; break;
                    case "95": brush = Brushes.Plum; break;
                    case "96": brush = Brushes.LightCyan; break;
                    case "97": brush = Brushes.White; break;
                }
            }

            return brush;
        }
    }
}
