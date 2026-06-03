using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;

namespace OneColumnEncoder.ViewModels
{
    public class SampleClipModalVM : BaseVM
    {
        private readonly ModalNavS _modalNavS;
        private readonly Action _closeAction;
        private readonly Func<EncodingPipelineRequest?> _buildRequest;
        private readonly double _totalSeconds;
        private readonly double _frameRate;
        private bool _isDraggingSelection;

        private bool _isSyncing;

        public static string WindowTitle => "1cenc Sample Clip";
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

        private int _clipLengthSeconds = 30;
        public int ClipLengthSeconds
        {
            get => _clipLengthSeconds;
            set
            {
                int next = Math.Max(10, Math.Min(600, value));
                if (!SetProperty(ref _clipLengthSeconds, next)) return;
                ApplyClipLengthToSelection();
            }
        }

        private string _startTimeText = "00:00:00.000";
        public string StartTimeText
        {
            get => _startTimeText;
            set => SetProperty(ref _startTimeText, value);
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
            set => SetProperty(ref _endTimeText, value);
        }

        private string _startFrameText = "0";
        public string StartFrameText
        {
            get => _startFrameText;
            set => SetProperty(ref _startFrameText, value);
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
            set => SetProperty(ref _endFrameText, value);
        }

        public SampleClipModalVM(ModalNavS modalNavS, Action closeAction, Func<EncodingPipelineRequest?> buildRequest, VideoAnalysisM srcVideoAnalysis)
        {
            _modalNavS = modalNavS;
            _closeAction = closeAction;
            _buildRequest = buildRequest;
            (_totalSeconds, _frameRate, long totalFrames, bool progressive, bool constantFrameRate) = ReadSourceStats(srcVideoAnalysis.RawJson);

            BuildSummary(totalFrames, progressive, constantFrameRate);
            BuildAxisLabels();

            CloseCmd = new CloseModalCmd(closeAction);
            RunSampleCmd = new ActionCmd(_ => RunSample());
            FinishButtons = ButtonGroupVM.CreateTwoButton("取消", "开始打样", CloseCmd, RunSampleCmd);

            int initialLength = (int)Math.Round(Math.Min(30d, Math.Max(10d, _totalSeconds * 0.04d)));
            _clipLengthSeconds = initialLength;
            ApplyClipLengthToSelection();
            SyncFromSelection(updateClipLength: false);
        }

        private void BuildSummary(long totalFrames, bool progressive, bool constantFrameRate)
        {
            SummaryColumns.Clear();
            SummaryColumns.Add(new ColumnTextItemM
            {
                TopText = "总时长",
                MainText = $"{Math.Round(_totalSeconds, 1).ToString("0.#", CultureInfo.InvariantCulture)} s",
                BottomText = "秒"
            });
            SummaryColumns.Add(new ColumnTextItemM
            {
                TopText = "总帧数",
                MainText = $"{totalFrames} f",
                BottomText = progressive ? "逐行扫描" : "隔行/未知"
            });
            SummaryColumns.Add(new ColumnTextItemM
            {
                TopText = "帧率",
                MainText = $"{_frameRate.ToString("0.###", CultureInfo.InvariantCulture)} fps",
                BottomText = constantFrameRate ? "恒定帧率" : "可变/未知帧率"
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
            if (end < start) (start, end) = (end, start);

            double startSeconds = start * _totalSeconds;
            double endSeconds = end * _totalSeconds;
            double durationSeconds = Math.Max(0d, endSeconds - startSeconds);

            StartTimeText = EncodingPipelineH.FormatTimestamp(TimeSpan.FromSeconds(startSeconds));
            ClipDurationText = EncodingPipelineH.FormatTimestamp(TimeSpan.FromSeconds(durationSeconds));
            EndTimeText = EncodingPipelineH.FormatTimestamp(TimeSpan.FromSeconds(endSeconds));

            long startFrame = SecondsToFirstFrame(startSeconds);
            long endFrame = Math.Max(startFrame, SecondsToLastFrame(endSeconds));
            StartFrameText = startFrame.ToString(CultureInfo.InvariantCulture);
            ClipFrameCountText = Math.Max(1, endFrame - startFrame + 1).ToString(CultureInfo.InvariantCulture);
            EndFrameText = endFrame.ToString(CultureInfo.InvariantCulture);

            if (updateClipLength)
            {
                int seconds = Math.Max(10, Math.Min(600, (int)Math.Round(durationSeconds)));
                SetProperty(ref _clipLengthSeconds, seconds, nameof(ClipLengthSeconds));
            }

            _isSyncing = false;
        }

        private void RunSample()
        {
            try
            {
                EncodingPipelineRequest? request = _buildRequest();
                if (request == null) return;

                EncodingClipRequest clip = BuildClipRequest();
                EncodingPipelineCommand command = EncodingPipelineH.BuildY4mCommand(request with { Clip = clip });
                new OpenInfoOrDbgModalCmd(_modalNavS, "Sample Encoding Command", command.CommandLine).Execute(null);
            }
            catch (Exception ex)
            {
                new OpenInfoOrDbgModalCmd(_modalNavS, "Sample Clip Error", ex.Message).Execute(null);
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

        private static (double DurationSeconds, double FrameRate, long TotalFrames, bool Progressive, bool ConstantFrameRate) ReadSourceStats(string rawJson)
        {
            const double fallbackDuration = 600d;
            const double fallbackFrameRate = 30d;

            if (string.IsNullOrWhiteSpace(rawJson))
                return (fallbackDuration, fallbackFrameRate, (long)(fallbackDuration * fallbackFrameRate), true, true);

            try
            {
                using JsonDocument document = JsonDocument.Parse(rawJson);
                JsonElement root = document.RootElement;
                JsonElement stream = root.GetProperty("streams")[0];

                double duration = TryGetDouble(stream, "duration")
                    ?? (root.TryGetProperty("format", out JsonElement format) ? TryGetDouble(format, "duration") : null)
                    ?? fallbackDuration;

                double frameRate = ParseFrameRate(TryGetString(stream, "avg_frame_rate"))
                    ?? ParseFrameRate(TryGetString(stream, "r_frame_rate"))
                    ?? fallbackFrameRate;

                long totalFrames = TryGetLong(stream, "nb_frames")
                    ?? Math.Max(0L, (long)Math.Round(duration * frameRate));

                string? fieldOrder = TryGetString(stream, "field_order");
                bool progressive = string.IsNullOrWhiteSpace(fieldOrder)
                    || fieldOrder.Equals("progressive", StringComparison.OrdinalIgnoreCase)
                    || fieldOrder.Equals("unknown", StringComparison.OrdinalIgnoreCase);

                string? avg = TryGetString(stream, "avg_frame_rate");
                string? r = TryGetString(stream, "r_frame_rate");
                bool constantFrameRate = !string.IsNullOrWhiteSpace(avg)
                    && !avg.Equals("0/0", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(avg, r, StringComparison.OrdinalIgnoreCase);

                return (duration, frameRate, totalFrames, progressive, constantFrameRate);
            }
            catch
            {
                return (fallbackDuration, fallbackFrameRate, (long)(fallbackDuration * fallbackFrameRate), true, true);
            }
        }

        private static string FormatAxisTimestamp(double seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(Math.Max(0d, seconds));
            return $"{(long)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
        }

        private static string? TryGetString(JsonElement element, string propertyName) =>
            element.TryGetProperty(propertyName, out JsonElement property)
                ? property.GetString()
                : null;

        private static double? TryGetDouble(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement property)) return null;
            if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out double value)) return value;
            return double.TryParse(property.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                ? value
                : null;
        }

        private static long? TryGetLong(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement property)) return null;
            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out long value)) return value;
            return long.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                ? value
                : null;
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
