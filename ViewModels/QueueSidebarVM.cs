using OneColumnEncoder.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace OneColumnEncoder.ViewModels
{
    public class QueueSidebarVM : BaseVM
    {
        private readonly QueueJobsStoreM _store;
        private readonly bool _isPersistent;
        private bool _isVisible;
        private QueueJobItemVM? _selectedJob;

        public QueueSidebarVM(bool loadFromDisk = true)
        {
            _isPersistent = loadFromDisk;
            _store = loadFromDisk ? QueueJobsStoreM.Load() : new QueueJobsStoreM();
            _isVisible = _store.Jobs.Count > 1;
            RefreshJobs();
        }

        public ObservableCollection<QueueJobItemVM> Jobs { get; } = [];

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public QueueJobItemVM? SelectedJob
        {
            get => _selectedJob;
            set
            {
                if (_selectedJob == value) return;
                if (_selectedJob != null)
                    _selectedJob.IsSidebarSelected = false;
                _selectedJob = value;
                if (_selectedJob != null)
                    _selectedJob.IsSidebarSelected = true;
                OnPropertyChanged();
            }
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

        public void ClearAllJobs()
        {
            _store.Jobs.Clear();
            Jobs.Clear();
            SelectedJob = null;
            RefreshBindings();
        }

        public void LoadFromDisk()
        {
            if (!_isPersistent) return;
            var loaded = QueueJobsStoreM.Load();
            _store.Jobs.Clear();
            foreach (var job in loaded.Jobs)
                _store.Jobs.Add(job);
            IsVisible = _store.Jobs.Count > 1;
            RefreshJobs();
        }

        public void SaveToDisk()
        {
            if (!_isPersistent) return;
            _store.Save();
        }

        public void AddJob(QueueJobItemM job)
        {
            _store.Jobs.Add(job);
            QueueJobItemVM jobVM = new(job);
            Jobs.Add(jobVM);
            SelectedJob ??= jobVM;
            RefreshBindings();
        }

        public void RemoveJob(QueueJobItemM job)
        {
            _store.Jobs.Remove(job);
            var vm = Jobs.FirstOrDefault(j => j.JobId == job.JobId);
            if (vm != null) Jobs.Remove(vm);
            RefreshBindings();
        }

        public void RemoveJob(QueueJobItemVM job)
        {
            int index = Jobs.IndexOf(job);
            if (index < 0) return;
            _store.Jobs.RemoveAt(index);
            Jobs.RemoveAt(index);
            if (SelectedJob == job) SelectedJob = Jobs.Count > 0 ? Jobs[Math.Min(index, Jobs.Count - 1)] : null;
            RefreshBindings();
        }

        public bool MoveJobUp(QueueJobItemVM job)
        {
            int index = Jobs.IndexOf(job);
            if (index <= 0) return false;
            (_store.Jobs[index], _store.Jobs[index - 1]) = (_store.Jobs[index - 1], _store.Jobs[index]);
            Jobs.Move(index, index - 1);
            RefreshBindings();
            return true;
        }

        public QueueJobItemVM? GetNextPending()
        {
            var next = _store.Jobs.FirstOrDefault(j => j.Status == "Pending");
            return next != null ? Jobs.FirstOrDefault(j => j.JobId == next.JobId) : null;
        }

        public void MarkJobEncoding(QueueJobItemVM job)
        {
            job.Status = "Encoding";
            SelectedJob = job;
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
            if (!_isPersistent)
            {
                base.Dispose();
                return;
            }
            SaveToDisk();
            base.Dispose();
        }
    }
}
