namespace SkyApm.Config
{
    partial class InstrumentConfig
    {
        public TraceScale Scale { get; set; } = TraceScale.Segment;
    }

    public enum TraceScale
    {
        Segment,
        Span
    }
}
