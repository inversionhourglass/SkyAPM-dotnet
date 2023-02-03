namespace SkyApm.Config
{
    [Config("SkyWalking", "SpanStructure")]
    public class SpanStructureConfig
    {
        public int MergeDelay { get; set; } = 5000;

        public int MergeQueueSize { get; set; } = 50000;

        public int DelaySeconds { get; set; } = 10000;

        public int DelayInspectInterval { get; set; } = 3000;

        public bool ApplyIncompleteToName { get; set; } = true;
    }
}
