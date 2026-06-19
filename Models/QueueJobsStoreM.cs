using OneColumnEncoder.Helpers;
using System.Collections.Generic;
using System.IO;

namespace OneColumnEncoder.Models
{
    public class QueueJobsStoreM : SaveLoadBaseH<QueueJobsStoreM>
    {
        public List<QueueJobItemM> Jobs { get; set; } = [];
        public bool IsBatchActive { get; set; }
        public int MaxConcurrent { get; set; } = 1;

        protected override string FilePath =>
            Path.Combine(GetConfigDirectory(), "queue-store.json");
    }
}
