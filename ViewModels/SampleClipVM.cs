using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using System.Windows;

namespace OneColumnEncoder.ViewModels
{
    public class SampleClipVM : BaseVM
    {
        private const int MinClipLengthSeconds = 10;
        private const int MaxClipLengthSeconds = 600;

        private readonly ModalNavS _modalNavS;
        private readonly Action _closeAction;
        private readonly Func<EncodingPipelineRequest?> _buildRequest;
        private readonly double _totalSeconds;
        private readonly double _frameRate;
        private readonly long _totalFrames;
        private readonly string _fieldOrderKind = "unknown";
        private readonly string _frameRateKind = "unknown";
        private ClipRangeSelectorLangProviderM _lang = null!;
        private bool _isDraggingSelection;

        private bool _isSyncing;

        public ClipRangeSelectorLangProviderM Lang
        {
            get => _lang;
            private set => SetProperty(ref _lang, value);
        }
        public static string WindowTitle => "1cenc Sample Clip";
        public string TimelineSectionTitle => Lang.TimelineSectionTitle;
        public string SelectionHintText => Lang.SelectionHintText;
        public string DurationSectionTitle => Lang.DurationSectionTitle;
        public string ClipLengthLabel => Lang.ClipLengthLabel;
        public string StartTimeLabel => Lang.StartTimeLabel;
        public string ClipDurationLabel => Lang.ClipDurationLabel;
        public string EndTimeLabel => Lang.EndTimeLabel;
        public string TimeFormatText => Lang.TimeFormatText;
        public string StartFrameLabel => Lang.StartFrameLabel;
        public string ClipFrameCountLabel => Lang.ClipFrameCountLabel;
        public string EndFrameLabel => Lang.EndFrameLabel;
        public string FrameFormatText => Lang.FrameFormatText;
        public string Note1Text => Lang.Note1Text;
        public string Note2Text => Lang.Note2Text;
        public ObservableCollection<ColumnTextItemM> SummaryColumns { get; } = [];
        public ObservableCollection<string> AxisLabels { get; } = [];
        public ButtonGroupVM FinishButtons { get; }
        public CloseModalCmd CloseCmd { get; }
        public ActionCmd RunSampleCmd { get; }

        public static string[] ClipLengthTickLabels => ["10", "30", "90", "150", "300", "600"];

        private double _selectionStart = 0.48d;
        public double SelectionStart
        {
            get => _selectionStart;
            set
            {
                if (!SetProperty(ref _selectionStart, value)) return;
                SyncFromSelection(updateClipLength: true);
            }
        }

        private double _selectionEnd = 0.52d;
        public double SelectionEnd
        {
            get => _selectionEnd;
            set
            {
                if (!SetProperty(ref _selectionEnd, value)) return;
                SyncFromSelection(updateClipLength: true);
            }
        }

        private int _clipLengthSeconds = 5;
        public int ClipLengthSeconds
        {
            get => _clipLengthSeconds;
            set
            {
                int next = Math.Max(MinClipLengthSeconds, Math.Min(MaxClipLengthSeconds, value));
                if (!SetProperty(ref _clipLengthSeconds, next)) return;
                ApplyClipLengthToSelection();
            }
        }

        private string _startTimeText = "00:00:00.000";
        public string StartTimeText
        {
            get => _startTimeText;
            set
            {
                if (!SetProperty(ref _startTimeText, value)) return;
                if (!_isSyncing) CommitStartTimeText();
            }
        }

        private string _clipDurationText = "00:00:30.000";
        public string ClipDurationText
        {
            get => _clipDurationText;
            set => SetProperty(ref _clipDurationText, value);
        }

        private string _endTimeText = "00:00:30.000";
        public string EndTimeText
        {
            get => _endTimeText;
            set
            {
                if (!SetProperty(ref _endTimeText, value)) return;
                if (!_isSyncing) CommitEndTimeText();
            }
        }

        private string _startFrameText = "0";
        public string StartFrameText
        {
            get => _startFrameText;
            set
            {
                if (!SetProperty(ref _startFrameText, value)) return;
                if (!_isSyncing) CommitStartFrameText();
            }
        }

        private string _clipFrameCountText = "0";
        public string ClipFrameCountText
        {
            get => _clipFrameCountText;
            set => SetProperty(ref _clipFrameCountText, value);
        }

        private string _endFrameText = "0";
        public string EndFrameText
        {
            get => _endFrameText;
            set
            {
                if (!SetProperty(ref _endFrameText, value)) return;
                if (!_isSyncing) CommitEndFrameText();
            }
        }

        public SampleClipVM(ModalNavS modalNavS, Action closeAction, Func<EncodingPipelineRequest?> buildRequest, VideoAnalysisM srcVideoAnalysis)
        {
            _modalNavS = modalNavS;
            _closeAction = closeAction;
            _buildRequest = buildRequest;
            (_totalSeconds, _frameRate, long totalFrames, _fieldOrderKind, _frameRateKind) = ReadSourceStats(srcVideoAnalysis.RawJson);
            _totalFrames = Math.Max(1L, totalFrames);

            Lang = new ClipRangeSelectorLangProviderM(UILangProviderM.Current.LanguageCode);

            BuildSummary();
            BuildAxisLabels();

            CloseCmd = new CloseModalCmd(closeAction);
            RunSampleCmd = new ActionCmd(_ => RunSample());
            Lang = new ClipRangeSelectorLangProviderM(UILangProviderM.Current.LanguageCode);
            FinishButtons = ButtonGroupVM.CreateTwoButton(Lang.CancelButtonText, Lang.ConfirmButtonText, CloseCmd, RunSampleCmd);

            int initialLength = (int)Math.Round(Math.Min(30d, Math.Max(10d, _totalSeconds * 0.04d)));
            _clipLengthSeconds = initialLength;
            ApplyClipLengthToSelection();
            SyncFromSelection(updateClipLength: false);
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        private void BuildSummary()
        {
            string frameBottomText = _fieldOrderKind switch
            {
                "progressive" => Lang.SummaryProgressive,
                "interlaced" => Lang.SummaryInterlaced,
                _ => Lang.SummaryUnknown,
            };


            SummaryColumns.Clear();
            // Total duration seconds
            SummaryColumns.Add(new ColumnTextItemM
            {
                TopText = Lang.SummaryDurationLabel,
                MainText = $"{Math.Round(_totalSeconds, 1).ToString("0.#", CultureInfo.InvariantCulture)}",
                BottomText = Lang.SummarySecondsUnit
            });
            // Total frames
            SummaryColumns.Add(new ColumnTextItemM
            {
                TopText = Lang.SummaryTotalFramesLabel,
                MainText = $"{_totalFrames} f",
                BottomText = frameBottomText
            });
            // Frame rate
            SummaryColumns.Add(new ColumnTextItemM
            {
                TopText = Lang.SummaryFrameRateLabel,
                MainText = $"{_frameRate.ToString("0.###", CultureInfo.InvariantCulture)} fps",
                BottomText = _frameRateKind switch
                {
                    "constant" => Lang.SummaryConstantFrameRate,
                    "variable" => Lang.SummaryVariableFrameRate,
                    _ => Lang.SummaryFrameRateUnknown,
                }
            });
        }

        private void BuildAxisLabels()
        {
            AxisLabels.Clear();
            for (int i = 0; i <= 4; i++)
            {
                double seconds = _totalSeconds * i / 4d;
                AxisLabels.Add(FormatAxisTimestamp(seconds));
            }
        }

        private void ApplyClipLengthToSelection()
        {
            if (_isSyncing || _totalSeconds <= 0) return;

            _isSyncing = true;
            double span = Math.Min(1d, ClipLengthSeconds / _totalSeconds);
            double start = Math.Max(0d, Math.Min(SelectionStart, 1d - span));
            SelectionStart = start;
            SelectionEnd = Math.Min(1d, start + span);
            _isSyncing = false;
            SyncFromSelection(updateClipLength: false);
        }

        internal void SetDraggingSelection(bool isDraggingSelection)
        {
            _isDraggingSelection = isDraggingSelection;
        }

        private void SyncFromSelection(bool updateClipLength)
        {
            if (_isSyncing || _totalSeconds <= 0) return;

            if (updateClipLength && _isDraggingSelection)
                updateClipLength = false;

            _isSyncing = true;
            double start = Math.Max(0d, Math.Min(1d, SelectionStart));
            double end = Math.Max(0d, Math.Min(1d, SelectionEnd));
            if (end < start) (start, end) = (end, start); // Fix overlap

            double startSeconds = start * _totalSeconds;
            double endSeconds = end * _totalSeconds;
            double durationSeconds = Math.Max(0d, endSeconds - startSeconds);

            StartTimeText =
                EncodingPipelineH.FormatTimestamp(TimeSpan.FromSeconds(startSeconds));
            ClipDurationText =
                EncodingPipelineH.FormatTimestamp(TimeSpan.FromSeconds(durationSeconds));
            EndTimeText =
                EncodingPipelineH.FormatTimestamp(TimeSpan.FromSeconds(endSeconds));

            long startFrame =
                Math.Min(_totalFrames - 1L, SecondsToFirstFrame(startSeconds));
            long endFrame =
                Math.Min(_totalFrames - 1L, Math.Max(startFrame, SecondsToLastFrame(endSeconds)));
            StartFrameText =
                startFrame.ToString(CultureInfo.InvariantCulture);
            ClipFrameCountText =
                Math.Max(1, endFrame - startFrame + 1).ToString(CultureInfo.InvariantCulture);
            EndFrameText =
                endFrame.ToString(CultureInfo.InvariantCulture);

            if (updateClipLength)
            {
                int seconds = Math.Max(
                    MinClipLengthSeconds,
                    Math.Min(MaxClipLengthSeconds,
                    (int)Math.Round(durationSeconds)));
                SetProperty(ref _clipLengthSeconds, seconds, nameof(ClipLengthSeconds));
            }

            _isSyncing = false;
        }

        private void CommitStartTimeText()
        {
            if (!TryParseSourceSeconds(StartTimeText, allowSourceEnd: false, out double startSeconds))
            {
                SyncFromSelection(updateClipLength: false);
                return;
            }

            double durationSeconds = GetCurrentClipDurationSeconds();
            ApplySelectionSeconds(startSeconds, startSeconds + durationSeconds, anchorEnd: false);
        }

        private void CommitEndTimeText()
        {
            if (!TryParseSourceSeconds(EndTimeText, allowSourceEnd: true, out double endSeconds) || endSeconds <= 0d)
            {
                SyncFromSelection(updateClipLength: false);
                return;
            }

            double durationSeconds = GetCurrentClipDurationSeconds();
            ApplySelectionSeconds(endSeconds - durationSeconds, endSeconds, anchorEnd: true);
        }

        private void CommitStartFrameText()
        {
            if (!TryParseSourceFrame(StartFrameText, out long startFrame))
            {
                SyncFromSelection(updateClipLength: false);
                return;
            }

            double startSeconds = startFrame / _frameRate;
            double durationSeconds = GetCurrentClipDurationSeconds();
            ApplySelectionSeconds(startSeconds, startSeconds + durationSeconds, anchorEnd: false);
        }

        private void CommitEndFrameText()
        {
            if (!TryParseSourceFrame(EndFrameText, out long endFrame))
            {
                SyncFromSelection(updateClipLength: false);
                return;
            }

            double endSeconds = Math.Min(_totalSeconds, (endFrame + 1d) / _frameRate);
            double durationSeconds = GetCurrentClipDurationSeconds();
            ApplySelectionSeconds(endSeconds - durationSeconds, endSeconds, anchorEnd: true);
        }

        private double GetCurrentClipDurationSeconds()
        {
            double durationSeconds = Math.Abs(SelectionEnd - SelectionStart) * _totalSeconds;
            if (durationSeconds <= 0d)
                durationSeconds = ClipLengthSeconds;

            double maxDurationSeconds = Math.Min(MaxClipLengthSeconds, _totalSeconds);
            double minDurationSeconds = Math.Min(MinClipLengthSeconds, maxDurationSeconds);
            return Clamp(durationSeconds, minDurationSeconds, maxDurationSeconds);
        }

        private void ApplySelectionSeconds(double startSeconds, double endSeconds, bool anchorEnd)
        {
            if (_totalSeconds <= 0d || double.IsNaN(startSeconds) || double.IsNaN(endSeconds) || double.IsInfinity(startSeconds) || double.IsInfinity(endSeconds))
            {
                SyncFromSelection(updateClipLength: false);
                return;
            }

            double maxDurationSeconds = Math.Min(MaxClipLengthSeconds, _totalSeconds);
            double minDurationSeconds = Math.Min(MinClipLengthSeconds, maxDurationSeconds);
            double durationSeconds = Clamp(endSeconds - startSeconds, minDurationSeconds, maxDurationSeconds);

            if (anchorEnd)
            {
                endSeconds = Clamp(endSeconds, 0d, _totalSeconds);
                startSeconds = endSeconds - durationSeconds;
            }
            else
            {
                startSeconds = Clamp(startSeconds, 0d, _totalSeconds);
                endSeconds = startSeconds + durationSeconds;
            }

            if (startSeconds < 0d)
            {
                startSeconds = 0d;
                endSeconds = Math.Min(_totalSeconds, durationSeconds);
            }

            if (endSeconds > _totalSeconds)
            {
                endSeconds = _totalSeconds;
                startSeconds = Math.Max(0d, endSeconds - durationSeconds);
            }

            double start = Clamp(startSeconds / _totalSeconds, 0d, 1d);
            double end = Clamp(endSeconds / _totalSeconds, 0d, 1d);
            if (end <= start)
            {
                SyncFromSelection(updateClipLength: false);
                return;
            }

            _isSyncing = true;
            SelectionStart = start;
            SelectionEnd = end;
            _isSyncing = false;
            SyncFromSelection(updateClipLength: true);
        }

        private bool TryParseSourceSeconds(string text, bool allowSourceEnd, out double seconds)
        {
            try
            {
                seconds = EncodingPipelineH.ParseTimestamp(text).TotalSeconds;
                return seconds >= 0d && (allowSourceEnd ? seconds <= _totalSeconds : seconds < _totalSeconds);
            }
            catch
            {
                seconds = 0d;
                return false;
            }
        }

        private bool TryParseSourceFrame(string text, out long frame)
        {
            frame = TryParseNonNegativeLong(text) ?? -1L;
            return frame >= 0L && frame < _totalFrames;
        }

        private static double Clamp(double value, double min, double max) =>
            Math.Max(min, Math.Min(max, value));

        private void RunSample()
        {
            try
            {
                EncodingPipelineRequest? request = _buildRequest();
                if (request == null)
                {
                    new OpenDebugModalCmd(_modalNavS, "Sample Clip Error", "Missing upstream input path. Make sure a video source or script source is selected for the chosen upstream tool.").Execute(null);
                    return;
                }

                EncodingClipRequest clip = BuildClipRequest();
                EncodingPipelineCommand command = EncodingPipelineH.BuildY4mCommand(request with { Clip = clip });

                ConfirmationModal? existing = Application.Current.Windows
                    .OfType<ConfirmationModal>()
                    .FirstOrDefault(w => w.DataContext is ConfirmationVM &&
                                    w.Owner == Application.Current.MainWindow);
                if (existing != null)
                {
                    existing.Activate();
                    return;
                }

                ConfirmationModal window = new();
                CloseModalCmd closeCmd = new(window.Close);
                ConfirmationVM vm = ConfirmationVM.CreateDebug(
                    "Sample Encoding Command", command.DisplayCommandLine,
                    closeCmd,
                    new ActionCmd(_ =>
                    {
                        window.DialogResult = true;
                        window.Close();
                        _closeAction();
                        new OpenEncodingMonitorCmd(_modalNavS, request with { Clip = clip }, command, isSample: true).Execute(null);
                    }));

                window.DataContext = vm;
                window.Owner = Application.Current.MainWindow;
                window.Closed += (_, _) => _modalNavS.Close();
                _modalNavS.CurrentModalVM = vm;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                new OpenDebugModalCmd(_modalNavS, "Sample Clip Error", ex.Message).Execute(null);
            }
        }

        private EncodingClipRequest BuildClipRequest()
        {
            string startTime = EncodingPipelineH.FormatTimestamp(EncodingPipelineH.ParseTimestamp(StartTimeText));
            string endTime = EncodingPipelineH.FormatTimestamp(EncodingPipelineH.ParseTimestamp(EndTimeText));
            long? firstFrame = TryParseNonNegativeLong(StartFrameText);
            long? lastFrame = TryParseNonNegativeLong(EndFrameText);

            return new EncodingClipRequest(startTime, endTime, firstFrame, lastFrame, _frameRate);
        }

        private long SecondsToFirstFrame(double seconds) =>
            Math.Max(0L, (long)Math.Ceiling(seconds * _frameRate));

        private long SecondsToLastFrame(double seconds) =>
            Math.Max(0L, (long)Math.Ceiling(seconds * _frameRate) - 1L);

        private static long? TryParseNonNegativeLong(string text)
        {
            if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value))
                return null;
            return Math.Max(0, value);
        }

        private static (double DurationSeconds, double FrameRate, long TotalFrames, string FieldOrderKind, string FrameRateKind) ReadSourceStats(string rawJson)
        {
            const double fallbackDuration = 600d;
            const double fallbackFrameRate = 30d;

            if (string.IsNullOrWhiteSpace(rawJson))
                return (fallbackDuration, fallbackFrameRate, (long)(fallbackDuration * fallbackFrameRate), "unknown", "unknown");

            try
            {
                using JsonDocument document = JsonDocument.Parse(rawJson);
                JsonElement root = document.RootElement;
                JsonElement stream = root.GetProperty("streams")[0];

                double duration = JsonElementHelper.TryGetDouble(stream, "duration")
                    ?? (root.TryGetProperty("format", out JsonElement format) ? JsonElementHelper.TryGetDouble(format, "duration") : null)
                    ?? fallbackDuration;

                double frameRate = ParseFrameRate(JsonElementHelper.TryGetString(stream, "avg_frame_rate"))
                    ?? ParseFrameRate(JsonElementHelper.TryGetString(stream, "r_frame_rate"))
                    ?? fallbackFrameRate;

                long totalFrames = JsonElementHelper.TryGetLong(stream, "nb_frames")
                    ?? Math.Max(0L, (long)Math.Round(duration * frameRate));

                string? fieldOrder = JsonElementHelper.TryGetString(stream, "field_order");
                string fieldOrderKind = string.IsNullOrWhiteSpace(fieldOrder) || fieldOrder.Equals("unknown", StringComparison.OrdinalIgnoreCase)
                    ? "unknown"
                    : fieldOrder.Equals("progressive", StringComparison.OrdinalIgnoreCase)
                        ? "progressive"
                        : "interlaced";

                string? avg = JsonElementHelper.TryGetString(stream, "avg_frame_rate");
                string? r = JsonElementHelper.TryGetString(stream, "r_frame_rate");
                string frameRateKind = !string.IsNullOrWhiteSpace(avg) && !avg.Equals("0/0", StringComparison.OrdinalIgnoreCase)
                    ? string.Equals(avg, r, StringComparison.OrdinalIgnoreCase) ? "constant" : "variable"
                    : "unknown";

                return (duration, frameRate, totalFrames, fieldOrderKind, frameRateKind);
            }
            catch
            {
                return (fallbackDuration, fallbackFrameRate, (long)(fallbackDuration * fallbackFrameRate), "unknown", "unknown");
            }
        }

        private static string FormatAxisTimestamp(double seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(Math.Max(0d, seconds));
            return $"{(long)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
        }

        private void OnLanguageChanged()
        {
            Lang = new ClipRangeSelectorLangProviderM(UILangProviderM.Current.LanguageCode);
            FinishButtons.B2_1Text = Lang.CancelButtonText;
            FinishButtons.B2_2Text = Lang.ConfirmButtonText;
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(TimelineSectionTitle));
            OnPropertyChanged(nameof(SelectionHintText));
            OnPropertyChanged(nameof(DurationSectionTitle));
            OnPropertyChanged(nameof(ClipLengthLabel));
            OnPropertyChanged(nameof(StartTimeLabel));
            OnPropertyChanged(nameof(ClipDurationLabel));
            OnPropertyChanged(nameof(EndTimeLabel));
            OnPropertyChanged(nameof(TimeFormatText));
            OnPropertyChanged(nameof(StartFrameLabel));
            OnPropertyChanged(nameof(ClipFrameCountLabel));
            OnPropertyChanged(nameof(EndFrameLabel));
            OnPropertyChanged(nameof(FrameFormatText));
            OnPropertyChanged(nameof(Note1Text));
            OnPropertyChanged(nameof(Note2Text));
            OnPropertyChanged(nameof(ClipLengthTickLabels));
            BuildSummary();
        }

        private static double? ParseFrameRate(string? text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Equals("0/0", StringComparison.OrdinalIgnoreCase))
                return null;

            string[] parts = text.Split('/');
            if (parts.Length == 2
                && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double n)
                && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double d)
                && d > 0)
                return n / d;

            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
                ? value
                : null;
        }
    }
}
