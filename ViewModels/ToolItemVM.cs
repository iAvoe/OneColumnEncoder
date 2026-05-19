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
    public class ToolItemVM : BaseVM
    {
        private readonly EncItemM _baseModel;
        public ToolItemVM(EncItemM baseModel) => _baseModel = baseModel;
        public string Name => _baseModel.Name;
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
                    Validate(); // Changes both versionText and isReal
                }
            }
        }

        private string _versionText = ""; // Non-nullable
        public string VersionText
        {
            get => _versionText;
            set
            {
                SetProperty(ref _versionText, value);
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

        public string P1Text => VersionText;

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
            set => SetProperty(ref _r1Text, value);
        }

        private string _r2Text = "";
        public string R2Text
        {
            get => _r2Text;
            set => SetProperty(ref _r2Text, value);
        }

        private ICommand? _r2Command;
        public ICommand? R2Command
        {
            get => _r2Command;
            set => SetProperty(ref _r2Command, value);
        }

        private void Validate()
        {
            IsReal = File.Exists(Path) && Path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
            VersionText = IsReal ? GetExeVersion(Path) : string.Empty;
        }

        private static string GetExeVersion(string path)
        {
            return "TODO (" + path + ")";
        }
    }
}