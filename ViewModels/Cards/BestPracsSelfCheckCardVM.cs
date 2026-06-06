using OneColumnEncoder.Models;

namespace OneColumnEncoder.ViewModels.Cards
{
    public class BestPracsSelfCheckCardVM : ValidationCardBaseVM
    {
        public BestPracsSelfCheckCardVM()
        {
            FillCollection(Checklist1, ChecklistProviderM.GetBestPracticeChecklist1());
            FillCollection(Checklist2, ChecklistProviderM.GetBestPracticeChecklist2());
        }

        public void RefreshLanguage()
        {
            RefreshChecklist(Checklist1, ChecklistProviderM.GetBestPracticeChecklist1());
            RefreshChecklist(Checklist2, ChecklistProviderM.GetBestPracticeChecklist2());
        }
    }
}
