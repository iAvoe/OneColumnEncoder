using OneColumnEncoder.Models;
using System.IO;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class QueueJobItemVM : BaseVM
    {
        private readonly QueueJobItemM _model;

        public QueueJobItemVM(QueueJobItemM model)
        {
            _model = model;
        }

        public QueueJobItemM Model => _model;
        public string JobId => _model.JobId;
        public string Name => Path.GetFileName(_model.SourcePath) ?? _model.SourcePath;

        public string P1Name => Status;
        public string P1Text
        {
            get
            {
                string status = _model.Status;
                return status == "Encoding"
                    ? $"{status} ({_model.ProgressPercent}%)"
                    : status;
            }
        }

        public string P1TooltipText => _model.ErrorMessage ?? P1Text;
        public string SeparatorText => ": ";

        public string P2Name => "Output";
        public string P2Text => Path.GetFileName(_model.OutputPath) ?? _model.OutputPath;

        public string DisplayR1Text => "Cancel";
        public string R2Text => "Retry";
        public bool R1IsEnabled => _model.Status == "Encoding";
        public bool R2IsEnabled => _model.Status == "Failed";

        public bool IsSelected => _model.Status == "Encoding";
        public bool IsCancel => _model.Status == "Failed";
        public bool IsReal => true;
        public bool EnableRealCheck => false;
        public bool IsEnabled => true;

        public ICommand? R1Command { get; set; }
        public ICommand? R2Command { get; set; }

        public string Status
        {
            get => _model.Status;
            set
            {
                if (_model.Status != value)
                {
                    _model.Status = value;
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(P1Name));
                    OnPropertyChanged(nameof(P1Text));
                    OnPropertyChanged(nameof(DisplayR1Text));
                    OnPropertyChanged(nameof(R2Text));
                    OnPropertyChanged(nameof(R1IsEnabled));
                    OnPropertyChanged(nameof(R2IsEnabled));
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
                    OnPropertyChanged(nameof(P1Text));
                }
            }
        }

        public void RefreshBindings()
        {
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(P1Name));
            OnPropertyChanged(nameof(P1Text));
            OnPropertyChanged(nameof(P1TooltipText));
            OnPropertyChanged(nameof(P2Text));
            OnPropertyChanged(nameof(DisplayR1Text));
            OnPropertyChanged(nameof(R2Text));
            OnPropertyChanged(nameof(R1IsEnabled));
            OnPropertyChanged(nameof(R2IsEnabled));
            OnPropertyChanged(nameof(IsSelected));
            OnPropertyChanged(nameof(IsCancel));
            OnPropertyChanged(nameof(Name));
        }
    }
}
