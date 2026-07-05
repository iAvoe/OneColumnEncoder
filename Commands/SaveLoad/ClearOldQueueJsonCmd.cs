using OneColumnEncoder.Models;
using OneColumnEncoder.Persistence;
using System.IO;

namespace OneColumnEncoder.Commands.SaveLoad
{
    public class ClearOldQueueJsonCmd : BaseCmd
    {
        public override void Execute(object? parameter)
        {
            string directory = SaveLoadBase<AppConfM>.GetConfigDirectory();
            if (!Directory.Exists(directory))
            {
                System.Windows.MessageBox.Show(
                    UILangProviderM.Current["AppConf.NoOldQueueJson"],
                    UILangProviderM.Current["AppConf.ClearOldQueueJsonTitle"],
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
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

            System.Windows.MessageBox.Show(
                string.Format(UILangProviderM.Current["AppConf.ClearOldQueueJsonResult"], deletedCount),
                UILangProviderM.Current["AppConf.ClearOldQueueJsonTitle"],
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
    }
}
