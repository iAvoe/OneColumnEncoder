using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class ConcatSourceItemVM : BaseVM
    {
        private bool _canMoveUp;
        private bool _canMoveDown;
        private bool _isSelected;
        private string _name = "";
        private string _pathText = "";
        private string _displayR1Text = "";
        private string _r2Text = "";
        private string _r3Text = "";

        public ConcatSourceItemVM(string filePath, int index, ICommand? removeCmd, ICommand? moveUpCmd, ICommand? moveDownCmd)
        {
            FilePath = filePath;
            UpdateDisplay(index);
            R1Command = removeCmd;
            R2Command = moveUpCmd;
            R3Command = moveDownCmd;
        }

        public string FilePath { get; }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string P1Text
        {
            get => _pathText;
            set => SetProperty(ref _pathText, value);
        }

        public string DisplayR1Text
        {
            get => _displayR1Text;
            set => SetProperty(ref _displayR1Text, value);
        }

        public string R2Text
        {
            get => _r2Text;
            set => SetProperty(ref _r2Text, value);
        }

        public string R3Text
        {
            get => _r3Text;
            set => SetProperty(ref _r3Text, value);
        }
        public bool R1IsEnabled => true;
        public bool R2IsEnabled => _canMoveUp;
        public bool R3IsEnabled => _canMoveDown;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool CanMoveUp
        {
            get => _canMoveUp;
            set
            {
                if (SetProperty(ref _canMoveUp, value))
                {
                    OnPropertyChanged(nameof(R2IsEnabled));
                }
            }
        }

        public bool CanMoveDown
        {
            get => _canMoveDown;
            set
            {
                if (SetProperty(ref _canMoveDown, value))
                {
                    OnPropertyChanged(nameof(R3IsEnabled));
                }
            }
        }

        public ICommand? R1Command { get; }
        public ICommand? R2Command { get; }
        public ICommand? R3Command { get; }

        public void UpdateDisplay(int index)
        {
            Name = System.IO.Path.GetFileName(FilePath);
            P1Text = FilePath;
        }
    }
}
