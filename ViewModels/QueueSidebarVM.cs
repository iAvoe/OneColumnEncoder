using OneColumnEncoder.Commands;
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
        private QueueJobItemVM? _runningJob;

        public QueueSidebarVM(bool loadFromDisk = true)
        {
            _isPersistent = loadFromDisk;
            _store = loadFromDisk ? QueueJobsStoreM.Load() : new QueueJobsStoreM();
            _isVisible = ShouldShowSidebar();
            SelectJobCommand = new ActionCmd(job =>
            {
                if (job is QueueJobItemVM jobVM) SelectedJob = jobVM;
            });
            RefreshJobs();
        }

        public ObservableCollection<QueueJobItemVM> WaitingJobs { get; } = [];
        public ObservableCollection<QueueJobItemVM> UnfinishedJobs { get; } = [];
        public ObservableCollection<QueueJobItemVM> CompletedJobs { get; } = [];
        public ActionCmd SelectJobCommand { get; }
        public int TotalCount => _store.Jobs.Count;

        public QueueJobItemVM? RunningJob
        {
            get => _runningJob;
            private set
            {
                if (!SetProperty(ref _runningJob, value)) return;
                OnPropertyChanged(nameof(HasRunningJob));
            }
        }

        public bool HasRunningJob => RunningJob != null;

        public QueueJobItemVM? SelectedWaitingJob
        {
            get => null;
            set
            {
                if (value != null) SelectedJob = value;
            }
        }

        public QueueJobItemVM? SelectedCompletedJob
        {
            get => null;
            set
            {
                if (value != null) SelectedJob = value;
            }
        }

        public QueueJobItemVM? SelectedUnfinishedJob
        {
            get => null;
            set
            {
                if (value != null) SelectedJob = value;
            }
        }

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
                int unfinished = _store.Jobs.Count(j => j.Status is "Failed" or "Interrupted");
                int encoding = _store.Jobs.Count(j => j.Status == "Encoding");
                return $"Ttl: {total} | Pnd: {pending} | Enc: {encoding} | Done: {completed} | Undone: {unfinished}";
            }
        }

        public void ClearAllJobs()
        {
            DisposeJobVMs();
            _store.Jobs.Clear();
            WaitingJobs.Clear();
            UnfinishedJobs.Clear();
            CompletedJobs.Clear();
            RunningJob = null;
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
            IsVisible = ShouldShowSidebar();
            RefreshJobs();
        }

        public void SaveToDisk()
        {
            if (!_isPersistent) return;
            _store.Save();
        }

        public void RefreshLanguage()
        {
            foreach (QueueJobItemVM job in EnumerateJobVMs().Distinct())
                job.RefreshBindings();
        }

        public void AddJob(QueueJobItemM job)
        {
            _store.Jobs.Add(job);
            QueueJobItemVM jobVM = new(job);
            AddJobToStatusCollection(jobVM);
            SelectedJob ??= jobVM;
            RefreshWaitingMoveStates();
            RefreshBindings();
        }

        public void RemoveJob(QueueJobItemM job)
        {
            _store.Jobs.Remove(job);
            var vm = FindJobVM(job.JobId);
            if (vm != null)
            {
                RemoveJobFromStatusCollection(vm);
                if (SelectedJob == vm) SelectedJob = RunningJob ?? WaitingJobs.LastOrDefault() ?? UnfinishedJobs.LastOrDefault() ?? CompletedJobs.LastOrDefault();
                vm.Dispose();
            }
            RefreshWaitingMoveStates();
            RefreshBindings();
        }

        public void RemoveJob(QueueJobItemVM job)
        {
            int index = WaitingJobs.IndexOf(job);
            if (index < 0) return;
            _store.Jobs.Remove(job.Model);
            WaitingJobs.RemoveAt(index);
            if (SelectedJob == job) SelectedJob = WaitingJobs.Count > 0 ? WaitingJobs[Math.Min(index, WaitingJobs.Count - 1)] : RunningJob ?? UnfinishedJobs.LastOrDefault() ?? CompletedJobs.LastOrDefault();
            job.Dispose();
            RefreshWaitingMoveStates();
            RefreshBindings();
        }

        public bool MoveJobUp(QueueJobItemVM job)
        {
            int index = WaitingJobs.IndexOf(job);
            if (index <= 0) return false;
            int storeIndex = GetStoreIndex(job);
            int previousStoreIndex = GetStoreIndex(WaitingJobs[index - 1]);
            if (storeIndex < 0 || previousStoreIndex < 0) return false;
            (_store.Jobs[storeIndex], _store.Jobs[previousStoreIndex]) = (_store.Jobs[previousStoreIndex], _store.Jobs[storeIndex]);
            WaitingJobs.Move(index, index - 1);
            job.FlashMovedHighlight();
            RefreshWaitingMoveStates();
            RefreshBindings();
            return true;
        }

        public bool MoveJobDown(QueueJobItemVM job)
        {
            int index = WaitingJobs.IndexOf(job);
            if (index < 0 || index >= WaitingJobs.Count - 1) return false;
            int storeIndex = GetStoreIndex(job);
            int nextStoreIndex = GetStoreIndex(WaitingJobs[index + 1]);
            if (storeIndex < 0 || nextStoreIndex < 0) return false;
            (_store.Jobs[storeIndex], _store.Jobs[nextStoreIndex]) = (_store.Jobs[nextStoreIndex], _store.Jobs[storeIndex]);
            WaitingJobs.Move(index, index + 1);
            job.FlashMovedHighlight();
            RefreshWaitingMoveStates();
            RefreshBindings();
            return true;
        }

        public QueueJobItemVM? GetNextPending()
        {
            var next = _store.Jobs.FirstOrDefault(j => j.Status == "Pending");
            return next != null ? WaitingJobs.FirstOrDefault(j => j.JobId == next.JobId) : null;
        }

        public void MarkJobEncoding(QueueJobItemVM job)
        {
            RemoveJobFromStatusCollection(job);
            job.Status = "Encoding";
            RunningJob = job;
            SelectedJob = job;
            RefreshWaitingMoveStates();
            SaveToDisk();
            RefreshBindings();
        }

        public void MarkJobCompleted(QueueJobItemVM job)
        {
            MoveJobToCompleted(job);
            job.Status = "Completed";
            job.Model.CompletedAt = System.DateTime.Now;
            RefreshWaitingMoveStates();
            SaveToDisk();
            RefreshBindings();
        }

        public void MarkJobFailed(QueueJobItemVM job, string? error = null)
        {
            MoveJobToUnfinished(job);
            job.Status = "Failed";
            job.Model.ErrorMessage = error;
            job.Model.CompletedAt = System.DateTime.Now;
            RefreshWaitingMoveStates();
            SaveToDisk();
            RefreshBindings();
        }

        public void MarkJobInterrupted(QueueJobItemVM job)
        {
            MoveJobToUnfinished(job);
            job.Status = "Interrupted";
            job.Model.CompletedAt = System.DateTime.Now;
            RefreshWaitingMoveStates();
            SaveToDisk();
            RefreshBindings();
        }

        public void CancelPendingJobs()
        {
            QueueJobItemVM[] pendingJobs = [.. WaitingJobs];
            foreach (QueueJobItemVM job in pendingJobs)
            {
                job.Status = "Interrupted";
                job.Model.CompletedAt = System.DateTime.Now;
                WaitingJobs.Remove(job);
                UnfinishedJobs.Add(job);
            }

            if (SelectedJob != null && pendingJobs.Any(job => job.JobId == SelectedJob.JobId))
                SelectedJob = RunningJob ?? UnfinishedJobs.LastOrDefault() ?? CompletedJobs.LastOrDefault();
            RefreshWaitingMoveStates();
            SaveToDisk();
            RefreshBindings();
        }

        private void RefreshJobs()
        {
            DisposeJobVMs();
            SelectedJob = null;
            WaitingJobs.Clear();
            UnfinishedJobs.Clear();
            CompletedJobs.Clear();
            RunningJob = null;
            foreach (var job in _store.Jobs)
                AddJobToStatusCollection(new QueueJobItemVM(job));
            RefreshWaitingMoveStates();
        }

        private void AddJobToStatusCollection(QueueJobItemVM job)
        {
            switch (job.Status)
            {
                case "Encoding":
                    RunningJob = job;
                    break;
                case "Completed":
                    CompletedJobs.Add(job);
                    break;
                case "Failed":
                case "Interrupted":
                    UnfinishedJobs.Add(job);
                    break;
                default:
                    WaitingJobs.Add(job);
                    break;
            }
        }

        #region Queue State Queries
        private QueueJobItemVM? FindJobVM(string jobId)
        {
            if (RunningJob?.JobId == jobId) return RunningJob;
            return WaitingJobs.FirstOrDefault(job => job.JobId == jobId)
                ?? UnfinishedJobs.FirstOrDefault(job => job.JobId == jobId)
                ?? CompletedJobs.FirstOrDefault(job => job.JobId == jobId);
        }

        private int GetStoreIndex(QueueJobItemVM job)
        {
            return _store.Jobs.FindIndex(item => item.JobId == job.JobId);
        }
        #endregion

        private void RemoveJobFromStatusCollection(QueueJobItemVM job)
        {
            if (RunningJob == job) RunningJob = null;
            WaitingJobs.Remove(job);
            UnfinishedJobs.Remove(job);
            CompletedJobs.Remove(job);
        }

        private void MoveJobToCompleted(QueueJobItemVM job)
        {
            RemoveJobFromStatusCollection(job);
            CompletedJobs.Add(job);
        }

        private void MoveJobToUnfinished(QueueJobItemVM job)
        {
            RemoveJobFromStatusCollection(job);
            UnfinishedJobs.Add(job);
        }

        private void RefreshWaitingMoveStates()
        {
            for (int i = 0; i < WaitingJobs.Count; i++)
                WaitingJobs[i].SetMoveButtonAvailability(i > 0, i < WaitingJobs.Count - 1);

            RunningJob?.SetMoveButtonAvailability(false, false);

            foreach (QueueJobItemVM job in UnfinishedJobs)
                job.SetMoveButtonAvailability(false, false);

            foreach (QueueJobItemVM job in CompletedJobs)
                job.SetMoveButtonAvailability(false, false);
        }

        private void RefreshBindings()
        {
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(StatsText));
            IsVisible = ShouldShowSidebar();
        }

        #region Sidebar Queries
        private bool ShouldShowSidebar() => _store.Jobs.Count > 1;

        private IEnumerable<QueueJobItemVM> EnumerateJobVMs()
        {
            foreach (QueueJobItemVM job in WaitingJobs) yield return job;
            foreach (QueueJobItemVM job in UnfinishedJobs) yield return job;
            foreach (QueueJobItemVM job in CompletedJobs) yield return job;
            if (RunningJob != null) yield return RunningJob;
        }
        #endregion

        private void DisposeJobVMs()
        {
            foreach (QueueJobItemVM job in EnumerateJobVMs().Distinct())
                job.Dispose();
        }

        public override void Dispose()
        {
            if (_isPersistent) SaveToDisk();
            DisposeJobVMs();
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
