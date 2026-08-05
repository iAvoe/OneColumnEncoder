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

    private string _p2Text = string.Empty;
    public string P2Text
    {
        get => _p2Text;
        set => SetProperty(ref _p2Text, value);
    }

    private string _p3Text = string.Empty;
    public string P3Text
    {
        get => _p3Text;
        set => SetProperty(ref _p3Text, value);
    }

    public string CancelText => RepartLangProvider.Current["Cancel"];
    public ICommand CancelCommand { get; } = cancelCommand;
}
