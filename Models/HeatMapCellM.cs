using System.ComponentModel;

namespace OneColumnEncoder.Models
{
    public class HeatMapCellM : INotifyPropertyChanged
    {
        private int _level;
        public int Level
        {
            get => _level;
            set
            {
                int next = Math.Clamp(value, 0, 8);
                if (_level == next) return;
                _level = next;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Level)));
            }
        }

        private string _tooltip = string.Empty;
        public string Tooltip
        {
            get => _tooltip;
            set
            {
                if (_tooltip == value) return;
                _tooltip = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tooltip)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
