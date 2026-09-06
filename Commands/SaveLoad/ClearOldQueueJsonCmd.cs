using System.IO;

namespace OneColumnEncoder.Commands.SaveLoad;

/// <summary>
/// Old JSON cleanup command
/// </summary>
/// <param name="modalNavS">Navigation modal service</param>
public class ClearOldQueueJsonCmd(ModalNavS modalNavS) : BaseCmd
{
    private readonly ModalNavS _modalNavS = modalNavS;

    /// <summary>
    /// Try to delete all the source_queue_*.json files in the config (/1cenc) folder older than 7 days
    /// If no config folder exists, show a info modal to user
    /// </summary>
    /// <param name="parameter">No parameters are used, actually</param>
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
            catch { } // Probably disk IO fails, which does not really matter
        }

        new OpenInfoModalCmd(_modalNavS, lang["AppConf.ClearOldQueueJsonTitle"], string.Format(lang["AppConf.ClearOldQueueJsonResult"], deletedCount))
            .Execute(null);
    }
}
