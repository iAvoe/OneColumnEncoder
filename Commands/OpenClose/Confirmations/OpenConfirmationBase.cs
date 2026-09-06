namespace OneColumnEncoder.Commands.OpenClose.Confirmations;

/// <summary>
/// Base command for ConfirmationModal dialogs.
/// Handles the shared dialog lifecycle: activating an existing dialog, wiring
/// the ViewModel to the window, and capturing the dialog result on close.
/// Subclasses only supply the ConfirmationVM factory and optional extra
/// configuration (e.g. context menu items).
/// </summary>
public abstract class OpenConfirmationBase(
    ModalNavS modalNavS,
    string windowTitle,
    string description) : OpenCloseBase(modalNavS)
{
    protected string WindowTitle { get; } = windowTitle;
    protected string Description { get; } = description;

    /// <summary>
    /// The dialog result, captured when the dialog closes. Falls back to the
    /// window's DialogResult when the dialog is dismissed without a button.
    /// </summary>
    public bool? DialogResult { get; protected set; }

    public override void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<ConfirmationModal>(
            w => w.DataContext is ConfirmationVM && w.Owner == GetSafeOwnerWindow()))
            return;

        ConfirmationModal window = new();
        ConfirmationVM vm = CreateViewModel(window);
        ConfigureViewModel(vm);
        ShowModal(window,
            vm,
            showDialog: true,
            onClosed: () => DialogResult ??= window.DialogResult == true);
    }

    protected abstract ConfirmationVM CreateViewModel(ConfirmationModal window);

    protected virtual void ConfigureViewModel(ConfirmationVM vm) { }
}
