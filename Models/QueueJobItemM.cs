using System;
using System.Text.Json.Serialization;

namespace OneColumnEncoder.Models
{
    public class QueueJobItemM
    {
        public string JobId { get; set; } = Guid.NewGuid().ToString();
        public string SourcePath { get; set; } = "";
        public string OutputPath { get; set; } = "";
        public string Status { get; set; } = "Pending";
        public int ProgressPercent { get; set; }
        public DateTime QueuedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public string EncoderExeName { get; set; } = "";
        public string SerializedRequest { get; set; } = "";
        public string SerializedCommand { get; set; } = "";
        public int UpstreamPid { get; set; }
        public int EncoderPid { get; set; }
    }
}
