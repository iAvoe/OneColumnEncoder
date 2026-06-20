using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

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
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextPropertyChanged));

        public static readonly DependencyProperty RightTextProperty =
            DependencyProperty.Register(
                nameof(RightText),
                typeof(string),
                typeof(SplitTextCard),
                new PropertyMetadata(string.Empty, OnTextPropertyChanged));

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

        public SplitTextCard()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SplitTextCard card)
                card.OnTextUpdated();
        }

        private static void OnIsRichTextModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SplitTextCard card)
                card.RefreshRichTextDocuments();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshRichTextDocuments();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            bool isLeftPane = textBox == LeftTextBox;

            if (!IsRichTextMode)
            {
                if (IsTextAtBottom(textBox))
                    ScrollToBottom(textBox);
                return;
            }

            RefreshRichTextDocuments();
            if (isLeftPane ? IsScrollAtBottom(LeftRichTextBox) : IsScrollAtBottom(RightRichTextBox))
                ScrollToBottom(isLeftPane ? LeftRichTextBox : RightRichTextBox);
        }

        private void OnTextUpdated()
        {
            if (!IsLoaded) return;

            if (IsRichTextMode)
                RefreshRichTextDocuments();
        }

        private static void ScrollToBottom(TextBoxBase? control)
        {
            if (control == null) return;
            control.Dispatcher.BeginInvoke(control.ScrollToEnd, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void RefreshRichTextDocuments()
        {
            if (!IsRichTextMode) return;

            bool leftAtBottom = IsScrollAtBottom(LeftRichTextBox);
            bool rightAtBottom = IsScrollAtBottom(RightRichTextBox);

            SetRichTextDocument(LeftRichTextBox, LeftText);
            SetRichTextDocument(RightRichTextBox, RightText);

            if (leftAtBottom)
                ScrollToBottom(LeftRichTextBox);
            if (rightAtBottom)
                ScrollToBottom(RightRichTextBox);
        }

        private static bool IsScrollAtBottom(RichTextBox? richTextBox)
        {
            if (richTextBox?.Document == null) return true;

            ScrollViewer? sv = FindScrollViewer(richTextBox);
            if (sv == null) return true;

            return Math.Abs(sv.VerticalOffset + sv.ViewportHeight - sv.ExtentHeight) < 1d;
        }

        private static bool IsTextAtBottom(TextBox? textBox)
        {
            if (textBox == null) return true;

            ScrollViewer? sv = FindScrollViewer(textBox);
            if (sv == null) return true;

            return Math.Abs(sv.VerticalOffset + sv.ViewportHeight - sv.ExtentHeight) < 1d;
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

        private static void SetRichTextDocument(RichTextBox richTextBox, string text)
        {
            FlowDocument document = richTextBox.Document;
            if (document == null)
            {
                document = new FlowDocument();
                richTextBox.Document = document;
            }

            document.PagePadding = new Thickness(0);
            document.PageWidth = double.PositiveInfinity;
            document.ColumnWidth = double.PositiveInfinity;
            document.FontFamily = new FontFamily("Consolas");
            document.FontSize = richTextBox.FontSize;
            document.Foreground = richTextBox.Foreground;
            document.Background = Brushes.Transparent;
            document.Blocks.Clear();

            Paragraph paragraph = new()
            {
                Margin = new Thickness(0)
            };

            foreach (Inline inline in BuildAnsiRuns(text, richTextBox.Foreground))
                paragraph.Inlines.Add(inline);

            document.Blocks.Add(paragraph);
        }

        private static IEnumerable<Inline> BuildAnsiRuns(string text, Brush defaultBrush)
        {
            const string ansiPattern = "\x1B\\[(?<code>[0-9;]*)m";
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
