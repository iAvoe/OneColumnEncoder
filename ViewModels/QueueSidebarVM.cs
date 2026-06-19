using OneColumnEncoder.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace OneColumnEncoder.ViewModels
{
    public class QueueSidebarVM : BaseVM
    {
        private readonly QueueJobsStoreM _store;
        private bool _isVisible;

        public QueueSidebarVM()
        {
            _store = QueueJobsStoreM.Load();
            _isVisible = _store.Jobs.Count > 1;
            RefreshJobs();
        }

        public ObservableCollection<QueueJobItemVM> Jobs { get; } = [];

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public string StatsText
        {
            get
            {
                int total = _store.Jobs.Count;
                int pending = _store.Jobs.Count(j => j.Status == "Pending");
                int completed = _store.Jobs.Count(j => j.Status == "Completed");
                int failed = _store.Jobs.Count(j => j.Status == "Failed");
                int encoding = _store.Jobs.Count(j => j.Status == "Encoding");
                return $"Total: {total}  |  Pending: {pending}  |  Encoding: {encoding}  |  Done: {completed}  |  Failed: {failed}";
            }
        }

        public void LoadFromDisk()
        {
            var loaded = QueueJobsStoreM.Load();
            _store.Jobs.Clear();
            foreach (var job in loaded.Jobs)
                _store.Jobs.Add(job);
            IsVisible = _store.Jobs.Count > 1;
            RefreshJobs();
        }

        public void SaveToDisk()
        {
            _store.Save();
        }

        public void AddJob(QueueJobItemM job)
        {
            _store.Jobs.Add(job);
            Jobs.Add(new QueueJobItemVM(job));
            RefreshBindings();
        }

        public void RemoveJob(QueueJobItemM job)
        {
            _store.Jobs.Remove(job);
            var vm = Jobs.FirstOrDefault(j => j.JobId == job.JobId);
            if (vm != null) Jobs.Remove(vm);
            RefreshBindings();
        }

        public QueueJobItemVM? GetNextPending()
        {
            var next = _store.Jobs.FirstOrDefault(j => j.Status == "Pending");
            return next != null ? Jobs.FirstOrDefault(j => j.JobId == next.JobId) : null;
        }

        public void MarkJobEncoding(QueueJobItemVM job)
        {
            job.Status = "Encoding";
            SaveToDisk();
            RefreshBindings();
        }

        public void MarkJobCompleted(QueueJobItemVM job)
        {
            job.Status = "Completed";
            job.Model.CompletedAt = System.DateTime.Now;
            SaveToDisk();
            RefreshBindings();
        }

        public void MarkJobFailed(QueueJobItemVM job, string? error = null)
        {
            job.Status = "Failed";
            job.Model.ErrorMessage = error;
            job.Model.CompletedAt = System.DateTime.Now;
            SaveToDisk();
            RefreshBindings();
        }

        public void MarkJobInterrupted(QueueJobItemVM job)
        {
            job.Status = "Interrupted";
            job.Model.CompletedAt = System.DateTime.Now;
            SaveToDisk();
            RefreshBindings();
        }

        private void RefreshJobs()
        {
            Jobs.Clear();
            foreach (var job in _store.Jobs)
                Jobs.Add(new QueueJobItemVM(job));
        }

        private void RefreshBindings()
        {
            OnPropertyChanged(nameof(StatsText));
        }

        public override void Dispose()
        {
            SaveToDisk();
            base.Dispose();
        }
    }
}
