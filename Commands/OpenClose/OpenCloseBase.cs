namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Base command for opening and closing modal windows.
/// Centralizes the shared window lifecycle: activating an already-open window,
/// wiring the ViewModel to the window, pushing it onto the ModalNavS stack,
/// and popping the stack when the window is closed.
/// </summary>
public abstract class OpenCloseBase(ModalNavS modalNavS) : BaseCmd
{
    protected ModalNavS ModalNavS { get; } = modalNavS;

    /// <summary>
    /// If a window of type <typeparamref name="TWindow"/> is already open,
    /// brings it to the front and returns true; otherwise returns false.
    /// </summary>
    protected static bool TryActivateExistingWindow<TWindow>(Func<TWindow, bool>? predicate = null)
        where TWindow : Window
    {
        TWindow? existingWindow = Application.Current.Windows
            .OfType<TWindow>()
            .FirstOrDefault(predicate ?? (_ => true));

        if (existingWindow == null) return false;

        existingWindow.Activate();
        return true;
    }

    /// <summary>
    /// Wires up the window and shows it, using the command's ModalNavS.
    /// </summary>
    protected void ShowModal<TWindow, TVm>(
        TWindow window,
        TVm viewModel,
        bool showDialog = false,
        bool closeOpenStack = false,
        Action? onClosed = null)
        where TWindow : Window
        where TVm : BaseVM
    {
        ShowModal(ModalNavS, window, viewModel, showDialog, closeOpenStack, onClosed);
    }

    /// <summary>
    /// Wires up the window without showing it, using the command's ModalNavS.
    /// The caller is responsible for calling Show/ShowDialog afterwards.
    /// </summary>
    protected void AttachModal<TWindow, TVm>(
        TWindow window,
        TVm viewModel,
        bool closeOpenStack = false,
        Action? onClosed = null)
        where TWindow : Window
        where TVm : BaseVM
    {
        AttachModal(ModalNavS, window, viewModel, closeOpenStack, onClosed);
    }

    /// <summary>
    /// Wires up the window and shows it, using the given ModalNavS.
    /// </summary>
    public static void ShowModal<TWindow, TVm>(
        ModalNavS modalNavS,
        TWindow window,
        TVm viewModel,
        bool showDialog = false,
        bool closeOpenStack = false,
        Action? onClosed = null)
        where TWindow : Window
        where TVm : BaseVM
    {
        if (closeOpenStack && modalNavS.IsOpen)
            modalNavS.Close();

        window.DataContext = viewModel;
        window.Owner = Application.Current.MainWindow;
        window.Closed += (_, _) =>
        {
            modalNavS.Close();
            onClosed?.Invoke();
        };
        modalNavS.CurrentModalVM = viewModel;

        if (showDialog) window.ShowDialog();
        else window.Show();
    }

    /// <summary>
    /// Wires up the window without showing it, using the given ModalNavS.
    /// The caller is responsible for calling Show/ShowDialog afterwards.
    /// </summary>
    public static void AttachModal<TWindow, TVm>(
        ModalNavS modalNavS,
        TWindow window,
        TVm viewModel,
        bool closeOpenStack = false,
        Action? onClosed = null)
        where TWindow : Window
        where TVm : BaseVM
    {
        if (closeOpenStack && modalNavS.IsOpen)
            modalNavS.Close();

        window.DataContext = viewModel;
        window.Owner = Application.Current.MainWindow;
        window.Closed += (_, _) =>
        {
            modalNavS.Close();
            onClosed?.Invoke();
        };
        modalNavS.CurrentModalVM = viewModel;
    }
}
