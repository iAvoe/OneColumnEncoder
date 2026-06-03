using System.ComponentModel;

namespace OneColumnEncoder.Models
{
    public class ColumnTextItemM : INotifyPropertyChanged
    {
        private string _topText = string.Empty;
        public string TopText
        {
            get => _topText;
            set
            {
                if (_topText == value) return;
                _topText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TopText)));
            }
        }

        private string _mainText = string.Empty;
        public string MainText
        {
            get => _mainText;
            set
            {
                if (_mainText == value) return;
                _mainText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainText)));
            }
        }

        private string _bottomText = string.Empty;
        public string BottomText
        {
            get => _bottomText;
            set
            {
                if (_bottomText == value) return;
                _bottomText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BottomText)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
