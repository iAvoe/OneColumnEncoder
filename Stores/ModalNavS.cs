using OneColumnEncoder.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Stores
{
    // Switch between different modal view models (e.g., settings, checklists, etc.) in a centralized way.
    public class ModalNavS
    {
        private BaseVM? _currentModalVM;
        public BaseVM? CurrentModalVM
        {
            get => _currentModalVM;
            set
            {
                var previousViewModel = _currentModalVM; // Dispose previous VM if needed
                _currentModalVM = value;
                CurrentViewModelChanged?.Invoke();

                if (previousViewModel is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
        internal void Close() => CurrentModalVM = null;
        public event Action? CurrentViewModelChanged;
        public bool IsOpen => CurrentModalVM != null;
    }
}
