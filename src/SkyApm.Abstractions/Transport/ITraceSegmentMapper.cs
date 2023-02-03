using SkyApm.Tracing.Segments;

namespace SkyApm.Transport
{
    public interface ITraceSegmentMapper
    {
        SegmentRequest Map(TraceSegment traceSegment);
    }
}