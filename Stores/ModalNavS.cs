namespace OneColumnEncoder.Stores
{
    /// <summary>
    /// Centralized window navigator
    /// 
    /// # Usages
    /// 
    /// ## Commands/OpenClose/OpenXxxCmd.cs
    /// 
    ///     if (_modalNavS.IsOpen) _modalNavS.Close(); // ① Close other modals on the same stack height level
    ///     var window = new XxxModalWindow();
    ///     var vm = new XxxVM(modalNavS, window.Close, ...);
    ///     window.DataContext = vm;
    ///     window.Owner = Application.Current.MainWindow;
    ///     window.Closed += (_, _) => _modalNavS.Close(); // ② Combine Pop to Window Closed
    ///     _modalNavS.CurrentModalVM = vm; // ③ Push stack
    ///     window.Show();
    /// 
    /// ### ViewModals/XxxModal.cs
    /// 
    ///     public class XxxVM(ModalNavS modalNavS, Action closeAction) : BaseVM
    ///     {
    ///         public CloseModalCmd CloseCmd { get; } = new(closeAction); // ④ No constructor passing for modalNavS
    ///         private void YyyAction()
    ///         {
    ///             // ⑤ In case of opening new modal within (i.e., OpenInfoModalCmd / OpenDebugModalCmd), pass in modalNavS
    ///             new OpenInfoModalCmd(modalNavS, title, msg).Execute(null);
    ///         }
    ///         private void SaveAndClose() => closeAction(); // ⑥ Run combined close method
    ///     }
    /// 
    /// ## Modal dialog such as ShowDialog()
    /// 
    ///     public override void Execute(object? parameter)
    ///     {
    ///         // ① Optionlly check if other window is showing
    ///         var existing = Application.Current.Windows
    ///             .OfType<ConfirmationModal>()
    ///             .FirstOrDefault(w => w.Owner == Application.Current.MainWindow);
    ///         if (existing != null) { existing.Activate(); return; }
    ///     
    ///         ConfirmationModal window = new();
    ///         CloseModalCmd closeCmd = new(window.Close); // ② No constructor passing for modalNavS
    ///         
    ///         // Both buttons in VM shares the same closeCmd
    ///         window.DataContext = vm;
    ///         window.Owner = Application.Current.MainWindow;
    ///         window.Closed += (_, _) => _modalNavS.Close(); // ③ Combine Pop to Window Closed
    ///         _modalNavS.CurrentModalVM = vm; // ④ Push stack
    ///         window.ShowDialog();
    ///     }
    ///     
    /// </summary>
    public class ModalNavS
    {
        // Using Stack to make sure all modal other than MainUI are closed,
        // to enable/disable MainUI's blocking overlay only when its the sole Window
        private readonly Stack<BaseVM> _modalStack = new();

        public BaseVM? CurrentModalVM
        {
            get => _modalStack.Count > 0 ? _modalStack.Peek() : null;
            set
            {
                if (value != null)
                {
                    _modalStack.Push(value);
                    CurrentViewModelChanged?.Invoke();
                }
            }
        }

        internal void Close()
        {
            if (_modalStack.Count > 0)
            {
                BaseVM popped = _modalStack.Pop();
                popped.Dispose();
                CurrentViewModelChanged?.Invoke();
            }
        }

        internal void CloseAll()
        {
            if (_modalStack.Count == 0) return;

            while (_modalStack.Count > 0)
            {
                BaseVM popped = _modalStack.Pop();
                popped.Dispose();
            }

            CurrentViewModelChanged?.Invoke();
        }

        public event Action? CurrentViewModelChanged;
        public bool IsOpen => _modalStack.Count > 0;
        public T? GetModal<T>() where T : BaseVM => _modalStack.OfType<T>().FirstOrDefault();
        public bool HasModal<T>() where T : BaseVM => GetModal<T>() != null;
    }
}
