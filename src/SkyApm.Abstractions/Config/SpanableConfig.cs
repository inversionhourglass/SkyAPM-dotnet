namespace SkyApm.Config
{
    [Config("SkyWalking", "Spanable")]
    public class SpanableConfig
    {
        public int DelaySeconds { get; set; } = 30;

        public int DelayInspectInterval { get; set; } = 3000;

        public bool IncompleteAsError { get; set; } = true;
    }
}
