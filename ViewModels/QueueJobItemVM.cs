using OneColumnEncoder.Models;
using OneColumnEncoder.Helpers;
using System.IO;
using System.Text.Json;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class QueueJobItemVM : BaseVM
    {
        private readonly EncodingPipelineRequest? _request;
        private readonly QueueJobItemM _model;
        private readonly EncodingMonitorModalLangProviderM _queueLang;
        private bool _isSidebarSelected;
        private bool _canMoveUp;
        private bool _canMoveDown;

        public QueueJobItemVM(QueueJobItemM model)
        {
            _model = model;
            _request = DeserializeRequest(model.SerializedRequest);
            _queueLang = new EncodingMonitorModalLangProviderM(UILangProviderM.Current.LanguageCode);
        }

        public QueueJobItemM Model => _model;
        public EncodingPipelineRequest? Request => _request;
        public EncodingPipelineCommand? Command => DeserializeCommand(_model.SerializedCommand);
        public string JobId => _model.JobId;
        public string Name => Path.GetFileName(_model.SourcePath) ?? _model.SourcePath;
        public string P1Text
        {
            get => GetFrameCountText();
        }

        public string P1TooltipText => _model.ErrorMessage ?? P1Text;

        public string DisplayR1Text => _queueLang.QueueItemRemoveText;
        public string R2Text => _queueLang.QueueItemMoveUpText;
        public string R3Text => _queueLang.QueueItemMoveDownText;
        public bool R1IsEnabled => _model.Status == "Pending";
        public bool R2IsEnabled => _model.Status == "Pending" && _canMoveUp;
        public bool R3IsEnabled => _model.Status == "Pending" && _canMoveDown;

        public bool IsSelected => _isSidebarSelected || _model.Status == "Encoding";
        public bool IsCancel => _model.Status == "Failed";
        public bool IsReal => true;
        public bool EnableRealCheck => false;
        public bool IsEnabled => true;

        public ICommand? R1Command { get; set; }
        public ICommand? R2Command { get; set; }
        public ICommand? R3Command { get; set; }

        public int UpstreamPid
        {
            get => _model.UpstreamPid;
            set { _model.UpstreamPid = value; OnPropertyChanged(); }
        }

        public int EncoderPid
        {
            get => _model.EncoderPid;
            set { _model.EncoderPid = value; OnPropertyChanged(); }
        }

        public bool IsSidebarSelected
        {
            get => _isSidebarSelected;
            set
            {
                if (!SetProperty(ref _isSidebarSelected, value)) return;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public string Status
        {
            get => _model.Status;
            set
            {
                if (_model.Status != value)
                {
                    _model.Status = value;
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(DisplayR1Text));
                    OnPropertyChanged(nameof(R2Text));
                    OnPropertyChanged(nameof(R3Text));
                    OnPropertyChanged(nameof(R1IsEnabled));
                    OnPropertyChanged(nameof(R2IsEnabled));
                    OnPropertyChanged(nameof(R3IsEnabled));
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(IsCancel));
                }
            }
        }

        public int ProgressPercent
        {
            get => _model.ProgressPercent;
            set
            {
                if (_model.ProgressPercent != value)
                {
                    _model.ProgressPercent = value;
                    OnPropertyChanged(nameof(ProgressPercent));
                }
            }
        }

        public void RefreshBindings()
        {
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(P1Text));
            OnPropertyChanged(nameof(P1TooltipText));
            OnPropertyChanged(nameof(DisplayR1Text));
            OnPropertyChanged(nameof(R2Text));
            OnPropertyChanged(nameof(R3Text));
            OnPropertyChanged(nameof(R1IsEnabled));
            OnPropertyChanged(nameof(R2IsEnabled));
            OnPropertyChanged(nameof(R3IsEnabled));
            OnPropertyChanged(nameof(IsSelected));
            OnPropertyChanged(nameof(IsCancel));
            OnPropertyChanged(nameof(Name));
        }

        public void SetMoveButtonAvailability(bool canMoveUp, bool canMoveDown)
        {
            if (_canMoveUp != canMoveUp)
            {
                _canMoveUp = canMoveUp;
                OnPropertyChanged(nameof(R2IsEnabled));
            }

            if (_canMoveDown != canMoveDown)
            {
                _canMoveDown = canMoveDown;
                OnPropertyChanged(nameof(R3IsEnabled));
            }
        }

        private static EncodingPipelineRequest? DeserializeRequest(string serializedRequest)
        {
            if (string.IsNullOrWhiteSpace(serializedRequest)) return null;
            try
            {
                return JsonSerializer.Deserialize<EncodingPipelineRequest>(serializedRequest);
            }
            catch
            {
                return null;
            }
        }

        private static EncodingPipelineCommand? DeserializeCommand(string serializedCommand)
        {
            if (string.IsNullOrWhiteSpace(serializedCommand)) return null;
            try
            {
                return JsonSerializer.Deserialize<EncodingPipelineCommand>(serializedCommand);
            }
            catch
            {
                return null;
            }
        }

        private string GetFrameCountText()
        {
            long? frameCount = _request?.SourceFfprobeJson is { Length: > 0 }
                ? EncodingPipelineH.GetSourceTotalFrames(_request.SourceFfprobeJson)
                : null;

            if (frameCount is > 0)
                return $"{new ClipRangeSelectorLangProviderM(UILangProviderM.Current.LanguageCode).SummaryTotalFramesLabel}: {frameCount:N0}";

            return "N/A";
        }
    }
}
