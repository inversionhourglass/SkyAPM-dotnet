using SkyApm.Tracing.Segments;

namespace SkyApm.Utilities.StaticAccessor
{
    partial class NullInstances
    {

        public static readonly TraceSegment TraceSegment = new TraceSegment(string.Empty, string.Empty, false, string.Empty, string.Empty);

        public static readonly SpanOrSegmentContext SpanOrSegmentContext = new SpanOrSegmentContext(SegmentSpan, SegmentContext);
    }
}
