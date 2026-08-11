namespace OneColumnEncoder.Commands.SaveLoad;

public class SaveAppConfCmd(AppConfM appConfS, Action closeAction) : AppConfCmdBase(appConfS)
{
    protected override async Task ExecuteAsync(object? parameter)
    {
        SaveAppConf();
        await Task.CompletedTask;
        closeAction();
    }
}
