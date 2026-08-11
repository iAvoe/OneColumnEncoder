using System.IO;

namespace OneColumnEncoder.Commands.SaveLoad;

public class ClearOldQueueJsonCmd : BaseCmd
{
    private readonly ModalNavS _modalNavS;

    public ClearOldQueueJsonCmd(ModalNavS modalNavS)
    {
        _modalNavS = modalNavS;
    }

    public override void Execute(object? parameter)
    {
        AppConfLangProvider lang = AppConfLangProvider.Current;
        string directory = SaveLoadBase<AppConfM>.GetConfigDirectory();
        if (!Directory.Exists(directory))
        {
            new OpenInfoModalCmd(_modalNavS, lang["AppConf.ClearOldQueueJsonTitle"], lang["AppConf.NoOldQueueJson"])
                .Execute(null);
            return;
        }

        string[] files = Directory.GetFiles(directory, "source_queue_*.json");
        DateTime cutoff = DateTime.Now.AddDays(-7);
        int deletedCount = 0;

        foreach (string file in files)
        {
            try
            {
                FileInfo fi = new(file);
                if (fi.LastWriteTime < cutoff)
                {
                    fi.Delete();
                    deletedCount++;
                }
            }
            catch
            {
            }
        }

        new OpenInfoModalCmd(_modalNavS, lang["AppConf.ClearOldQueueJsonTitle"],
                string.Format(lang["AppConf.ClearOldQueueJsonResult"], deletedCount))
            .Execute(null);
    }
}
