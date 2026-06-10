namespace OneColumnEncoder.Models
{
    public class EncItemM(string name)
    {
        public Guid Id = Guid.NewGuid();
        public string Name { get; set; } = name;
        public string Path { get; set; } = ""; // Only available in UpstreamTools, Encoders, AnalyticTools, Import zones
    }
}