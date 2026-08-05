using OneColumnEncoder.Models;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels;

public sealed class ProgressVM(string windowTitle, string initialText, ICommand cancelCommand) : BaseVM
{
    public string WindowTitle { get; } = windowTitle;

    private string _p1Text = initialText;
    public string P1Text
    {
        get => _p1Text;
        set => SetProperty(ref _p1Text, value);
    }

    public string CancelText => RepartLangProvider.Current["Cancel"];
    public ICommand CancelCommand { get; } = cancelCommand;
}
