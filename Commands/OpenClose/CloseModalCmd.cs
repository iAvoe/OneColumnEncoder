namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Command that closes an associated modal window by invoking its close action.
/// </summary>
public class CloseModalCmd(Action closeAction) : BaseCmd
{
    /// <summary>
    /// Invokes the window's close action.
    /// </summary>
    public override void Execute(object? parameter)
    {
        closeAction();
    }
}
