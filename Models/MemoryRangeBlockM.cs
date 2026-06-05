using System;
using System.ComponentModel;

namespace OneColumnEncoder.Models
{
    public class MemoryRangeBlockM : INotifyPropertyChanged
    {
        private int _fillLevel;
        public int FillLevel
        {
            get => _fillLevel;
            set
            {
                int next = Math.Clamp(value, 0, 8);
                if (_fillLevel == next) return;
                _fillLevel = next;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FillLevel)));
            }
        }

        private MemoryCategory _category = MemoryCategory.Empty;
        public MemoryCategory Category
        {
            get => _category;
            set
            {
                if (_category == value) return;
                _category = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Category)));
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
