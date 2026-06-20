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
    /// <summary>
    /// A split-pane UserControl that displays left and right text side-by-side.
    /// Supports both plain TextBox mode and rich-text mode with ANSI color rendering.
    /// </summary>
    public partial class SplitTextCard : UserControl
    {
        // --- Dependency Properties ---

        /// <summary>Title displayed at the top of the card.</summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(SplitTextCard), new PropertyMetadata(string.Empty));

        /// <summary>Text content for the left pane. Supports two-way binding.</summary>
        public static readonly DependencyProperty LeftTextProperty =
            DependencyProperty.Register(
                nameof(LeftText),
                typeof(string),
                typeof(SplitTextCard),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextPropertyChanged));

        /// <summary>Text content for the right pane.</summary>
        public static readonly DependencyProperty RightTextProperty =
            DependencyProperty.Register(
                nameof(RightText),
                typeof(string),
                typeof(SplitTextCard),
                new PropertyMetadata(string.Empty, OnTextPropertyChanged));

        /// <summary>Determines whether the text panes are read-only.</summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(SplitTextCard), new PropertyMetadata(true));

        /// <summary>Font size applied to the text in both panes.</summary>
        public static readonly DependencyProperty TextFontSizeProperty =
            DependencyProperty.Register(nameof(TextFontSize), typeof(double), typeof(SplitTextCard), new PropertyMetadata(12.0));

        /// <summary>
        /// When true, renders text through RichTextBox with ANSI escape-code color support.
        /// When false, uses plain TextBox controls.
        /// </summary>
        public static readonly DependencyProperty IsRichTextModeProperty =
            DependencyProperty.Register(nameof(IsRichTextMode), typeof(bool), typeof(SplitTextCard), new PropertyMetadata(false, OnIsRichTextModeChanged));

        // --- CLR Property Wrappers (allow XAML / code-behind binding) ---

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

        // --- Constructor ---

        public SplitTextCard()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        // --- Dependency-Property Change Callbacks ---

        /// <summary>
        /// Called whenever LeftText or RightText changes via the dependency property system.
        /// Delegates to the instance handler to refresh the rich-text documents.
        /// </summary>
        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SplitTextCard card)
                card.OnTextUpdated();
        }

        /// <summary>
        /// Called when IsRichTextMode toggles. Forces a full re-render of both panes.
        /// </summary>
        private static void OnIsRichTextModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SplitTextCard card)
                card.RefreshRichTextDocuments();
        }

        // --- Event Handlers ---

        /// <summary>Initializes rich-text documents after the control has been loaded into the visual tree.</summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshRichTextDocuments();
        }

        /// <summary>
        /// Handles TextChanged events from the plain-TextBox panes.
        /// In plain mode, auto-scrolls if the user was already at the bottom.
        /// In rich-text mode, rebuilds the FlowDocument and then auto-scrolls.
        /// </summary>
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            bool isLeftPane = textBox == LeftTextBox;

            if (!IsRichTextMode)
            {
                // Plain mode: only auto-scroll if already at the bottom
                if (IsTextAtBottom(textBox))
                    ScrollToBottom(textBox);
                return;
            }

            // Rich-text mode: rebuild documents, then scroll if needed
            RefreshRichTextDocuments();
            if (isLeftPane ? IsScrollAtBottom(LeftRichTextBox) : IsScrollAtBottom(RightRichTextBox))
                ScrollToBottom(isLeftPane ? LeftRichTextBox : RightRichTextBox);
        }

        /// <summary>
        /// Called when either LeftText or RightText dependency property value changes.
        /// Refreshes the rich-text documents if the control is already loaded.
        /// </summary>
        private void OnTextUpdated()
        {
            if (!IsLoaded) return;

            if (IsRichTextMode)
                RefreshRichTextDocuments();
        }

        // --- Scrolling Helpers ---

        /// <summary>
        /// Scrolls the given TextBoxBase control to the very end asynchronously.
        /// Uses Dispatcher.BeginInvoke with Background priority so layout completes first.
        /// </summary>
        private static void ScrollToBottom(TextBoxBase? control)
        {
            if (control == null) return;
            control.Dispatcher.BeginInvoke(control.ScrollToEnd, System.Windows.Threading.DispatcherPriority.Background);
        }

        // --- Rich-Text Rendering ---

        /// <summary>
        /// Rebuilds the FlowDocument content for both RichTextBox panes from the
        /// current LeftText / RightText values, preserving scroll position when possible.
        /// </summary>
        private void RefreshRichTextDocuments()
        {
            if (!IsRichTextMode) return;

            // Capture scroll state before rebuilding documents
            bool leftAtBottom = IsScrollAtBottom(LeftRichTextBox);
            bool rightAtBottom = IsScrollAtBottom(RightRichTextBox);

            // Rebuild FlowDocuments with ANSI-colored inline runs
            SetRichTextDocument(LeftRichTextBox, LeftText);
            SetRichTextDocument(RightRichTextBox, RightText);

            // Restore scroll position
            if (leftAtBottom)
                ScrollToBottom(LeftRichTextBox);
            if (rightAtBottom)
                ScrollToBottom(RightRichTextBox);
        }

        // --- Scroll-Position Detection ---

        /// <summary>
        /// Returns true if the RichTextBox's ScrollViewer is scrolled to (or very near) the bottom.
        /// </summary>
        private static bool IsScrollAtBottom(RichTextBox? richTextBox)
        {
            if (richTextBox?.Document == null) return true;

            ScrollViewer? sv = FindScrollViewer(richTextBox);
            if (sv == null) return true;

            // Allow 1 pixel of tolerance for floating-point comparison
            return Math.Abs(sv.VerticalOffset + sv.ViewportHeight - sv.ExtentHeight) < 1d;
        }

        /// <summary>
        /// Returns true if the TextBox's ScrollViewer is scrolled to (or very near) the bottom.
        /// </summary>
        private static bool IsTextAtBottom(TextBox? textBox)
        {
            if (textBox == null) return true;

            ScrollViewer? sv = FindScrollViewer(textBox);
            if (sv == null) return true;

            return Math.Abs(sv.VerticalOffset + sv.ViewportHeight - sv.ExtentHeight) < 1d;
        }

        /// <summary>
        /// Recursively walks the visual tree to find the first ScrollViewer ancestor/descendant
        /// of the given element.
        /// </summary>
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

        /// <summary>
        /// Builds a FlowDocument for the given RichTextBox, parsing the input text for
        /// ANSI escape sequences and rendering them as colored Run inlines.
        /// </summary>
        private static void SetRichTextDocument(RichTextBox richTextBox, string text)
        {
            FlowDocument document = richTextBox.Document;
            if (document == null)
            {
                document = new FlowDocument();
                richTextBox.Document = document;
            }

            // Configure the FlowDocument for monospace, single-column, edge-to-edge rendering
            document.PagePadding = new Thickness(0);
            document.PageWidth = 100_000;
            document.ColumnWidth = double.PositiveInfinity;
            document.FontFamily = new FontFamily("Consolas");
            document.FontSize = richTextBox.FontSize;
            document.Foreground = richTextBox.Foreground;
            document.Background = Brushes.Transparent;
            document.Blocks.Clear();

            Paragraph paragraph = new() { Margin = new Thickness(0) };

            // Parse ANSI escape codes and add colored Run inlines to the paragraph
            foreach (Inline inline in BuildAnsiRuns(text, richTextBox.Foreground))
                paragraph.Inlines.Add(inline);

            document.Blocks.Add(paragraph);
        }

        // --- ANSI Escape-Code Parsing ---

        /// <summary>
        /// Parses a string containing ANSI SGR escape sequences (e.g. "\x1B[31m") and yields
        /// a sequence of colored Run inlines. Text between escape codes uses the current brush.
        /// </summary>
        private static IEnumerable<Inline> BuildAnsiRuns(string text, Brush defaultBrush)
        {
            Regex regex = AnsiEscRegex();

            Brush currentBrush = defaultBrush;
            int lastIndex = 0;
            foreach (Match match in regex.Matches(text))
            {
                // Emit the plain text segment before this escape code
                if (match.Index > lastIndex)
                    yield return new Run(text[lastIndex..match.Index]) { Foreground = currentBrush };

                // Resolve the new brush from the ANSI code and advance the pointer
                currentBrush = ResolveAnsiBrush(match.Groups["code"].Value, defaultBrush, currentBrush);
                lastIndex = match.Index + match.Length;
            }

            // Emit any remaining text after the last escape code
            if (lastIndex < text.Length)
                yield return new Run(text[lastIndex..]) { Foreground = currentBrush };
        }

        /// <summary>
        /// Maps an ANSI SGR code string (e.g. "31", "0", "92") to a WPF Brush.
        /// Supports standard colors (30-37), bright colors (90-97), and reset (0).
        /// Returns the updated brush, or the default brush if the code is unrecognized.
        /// </summary>
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
                    case "0":   // SGR reset: restore default color
                        brush = defaultBrush;
                        break;
                    // Standard foreground colors (30-37)
                    case "30": brush = Brushes.Black; break;
                    case "31": brush = Brushes.Red; break;
                    case "32": brush = Brushes.LimeGreen; break;
                    case "33": brush = Brushes.Gold; break;
                    case "34": brush = Brushes.DodgerBlue; break;
                    case "35": brush = Brushes.Magenta; break;
                    case "36": brush = Brushes.Cyan; break;
                    case "37": brush = Brushes.Gainsboro; break;
                    // Bright foreground colors (90-97)
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

        [GeneratedRegex(@"\[(?<code>[0-9;]*)m", RegexOptions.CultureInvariant)]
        private static partial Regex AnsiEscRegex();
    }
}
