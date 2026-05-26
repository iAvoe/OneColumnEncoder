using OneColumnEncoder.Commands;
using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class ToolItemVM(EncItemM baseModel) : BaseVM
    {
        public static string SeparatorText =>
            UILangProviderM.Current["ItemCard.Separator"];

        private readonly EncItemM _baseModel = baseModel;

        #region Properties

        public string Name
        {
            get => _baseModel.Name;
            set
            {
                if (_baseModel.Name != value)
                {
                    _baseModel.Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public string Path
        {
            get => _baseModel.Path;
            set
            {
                if (_baseModel.Path != value)
                {
                    _baseModel.Path = value;
                    OnPropertyChanged(nameof(Path));
                    OnPropertyChanged(nameof(P2Text));
                    OnPropertyChanged(nameof(DisplayR1Text));
                    Validate(); // Changes both versionText and isReal
                }
            }
        }

        private string _primaryValueText = string.Empty; // Non-nullable
        public string PrimaryValueText
        {
            get => _primaryValueText;
            set
            {
                SetProperty(ref _primaryValueText, value);
                OnPropertyChanged(nameof(P1Text));
            }
        }

        private bool _isReal;
        public bool IsReal
        {
            get => _isReal;
            set => SetProperty(ref _isReal, value);
        }

        private string _p1Name = "";
        public string P1Name
        {
            get => _p1Name;
            set => SetProperty(ref _p1Name, value);
        }

        public string P1Text => PrimaryValueText;

        private string _p2Name = "";
        public string P2Name
        {
            get => _p2Name;
            set => SetProperty(ref _p2Name, value);
        }
        public string P2Text => Path;

        private string _r1Text = "";
        public string R1Text // Maybe 'Edit' in some cases
        {
            get => _r1Text;
            set
            {
                if (SetProperty(ref _r1Text, value))
                    OnPropertyChanged(nameof(DisplayR1Text));
            }
        }

        public string DisplayR1Text =>
            UseAutoAddReplaceText
                ? string.IsNullOrWhiteSpace(Path)
                    ? UILangProviderM.Current["Buttons.Add"]
                    : UILangProviderM.Current["Buttons.Replace"]
                : R1Text;

        private string _r2Text = "";
        public string R2Text
        {
            get => _r2Text;
            set => SetProperty(ref _r2Text, value);
        }

        private bool _useAutoAddReplaceText;
        public bool UseAutoAddReplaceText
        {
            get => _useAutoAddReplaceText;
            set
            {
                if (SetProperty(ref _useAutoAddReplaceText, value))
                    OnPropertyChanged(nameof(DisplayR1Text));
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    // Notify to unselect other ItemCards
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private bool _isCancel;
        public bool IsCancel
        {
            get => _isCancel;
            set => SetProperty(ref _isCancel, value);
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private ICommand? _r1Command;
        public ICommand? R1Command
        {
            get => _r1Command;
            set => SetProperty(ref _r1Command, value);
        }

        private ICommand? _r2Command;
        public ICommand? R2Command
        {
            get => _r2Command;
            set => SetProperty(ref _r2Command, value);
        }

        #endregion

        #region Methods

        private void Validate()
        {
            bool exists = File.Exists(Path);
            bool isKnownBinary =
                Path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                Path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);

            IsReal = exists && isKnownBinary;
            if (!IsReal) PrimaryValueText = string.Empty;
        }

        public void ApplyDefinition(ToolDefinitionM definition)
        {
            Name = definition.DisplayName;
            R1Text = definition.R1Text;
            R2Text = definition.R2Text;
            P1Name = definition.P1Name;
            P2Name = definition.P2Name ?? string.Empty;
        }

        public void RefreshLanguage()
        {
            OnPropertyChanged(nameof(SeparatorText));
            OnPropertyChanged(nameof(DisplayR1Text));
        }

        #endregion
    }
}
