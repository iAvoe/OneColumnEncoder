namespace OneColumnEncoder.Commands;

public abstract class AsyncBaseCmd : BaseCmd
{
    // Re-entry blocker
    private bool _isExecuting;
    public bool IsExecuting
    {
        get => _isExecuting;
        set
        {
            _isExecuting = value;
            OnCanExecuteChanged();
        }
    }

    // Implement a method that is async, so that whenever the button is clicked,
    // run ExecuteAsync instead of Execute,
    // and ExecuteAsync will be implemented in the derived class
    protected abstract Task ExecuteAsync(object? parameter);
    public override async void Execute(object? parameter)
    {
        if (IsExecuting) return;
        IsExecuting = true;
        // Handle errors inside ExecuteAsync,
        // so that the UI doesn't crash when an exception is thrown
        try
        {
            await ExecuteAsync(parameter);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            IsExecuting = false;
        }
    }
}
