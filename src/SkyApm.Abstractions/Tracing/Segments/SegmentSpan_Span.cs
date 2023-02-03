namespace SkyApm.Tracing.Segments
{
    partial class SegmentSpan
    {
        public const string TAG_INCOMPLETE = "incomplete";

        private TraceSegment _segment;

        /// <summary>
        /// Should only be set by first span of segment
        /// </summary>
        public TraceSegment Segment
        {
            get => _segment ?? Parent.Segment;
            set => _segment = value;
        }

        public SegmentReferenceCollection References { get; } = new SegmentReferenceCollection();

        /// <summary>
        /// A lot of logic uses Parent, think carefully before changing this value
        /// </summary>
        public SegmentSpan Parent { get; set; }

        public void AddLog(SpanLog log)
        {
            Logs.AddLog(log);
        }
    }
}
