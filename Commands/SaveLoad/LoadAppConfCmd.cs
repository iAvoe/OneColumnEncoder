namespace OneColumnEncoder.Commands.SaveLoad;

public class LoadAppConfCmd(AppConfM appConfS) : AppConfCmdBase(appConfS)
{
    protected override async Task ExecuteAsync(object? parameter)
    {
        LoadAppConf();
        await Task.CompletedTask;
    }
}
