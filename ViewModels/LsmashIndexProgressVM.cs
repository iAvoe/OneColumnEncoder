using OneColumnEncoder.ScriptGeneration;

namespace OneColumnEncoder.ViewModels;

/// <summary>
/// Progress modal for pre-building .lwi indexes after source import.
/// </summary>
public class LsmashIndexProgressVM : BaseVM
{
    private readonly ModalNavS _modalNavS;
    private readonly Action _closeAction;
    private CancellationTokenSource? _cancellationTokenSource;

    private string _progressTitle = string.Empty;
    private double _progressValue;
    private string _progressText = string.Empty;
    private string _hintText = string.Empty;

    public static string WindowTitle => LsmashIndexLangProvider.WindowTitle;

    public string ProgressTitle
    {
        get => _progressTitle;
        set => SetProperty(ref _progressTitle, value);
    }

    public double ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, value);
    }

    public string ProgressText
    {
        get => _progressText;
        set => SetProperty(ref _progressText, value);
    }

    public string HintText
    {
        get => _hintText;
        set => SetProperty(ref _hintText, value);
    }

    public bool IsProgressTrackingAvailable => true;
    public bool IsEncodingActive => true;

    public CloseModalCmd CloseCmd { get; }

    public LsmashIndexProgressVM(ModalNavS modalNavS, Action closeAction)
    {
        _modalNavS = modalNavS;
        _closeAction = closeAction;
        CloseCmd = new CloseModalCmd(closeAction);
    }

    public async Task RunIndexingAsync(IReadOnlyList<string> videoPaths)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        LsmashIndexLangProvider lang = LsmashIndexLangProvider.Current;

        try
        {
            Progress<LsmashIndexProgress> progress = new(p =>
            {
                ProgressTitle = string.Format(lang["LsmashIndex.CountFormat"], p.CompletedVideos, p.TotalVideos);
                ProgressValue = p.FilePercent;
                ProgressText = $"{p.FilePercent}%";
                HintText = string.IsNullOrWhiteSpace(p.CapturedLine)
                    ? string.Format(lang["LsmashIndex.CurrentFile"], p.CurrentFileName)
                    : p.CapturedLine;
            });

            HintText = lang["LsmashIndex.Preparing"];
            await LsmashIndexRunner.RunAsync(videoPaths, progress, _cancellationTokenSource.Token);
            HintText = lang["LsmashIndex.Done"];
        }
        catch (OperationCanceledException)
        {
            // User cancelled or window closed
        }
        finally
        {
            _closeAction();
        }
    }

    public override void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        base.Dispose();
    }
}
