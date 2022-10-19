using SkyApm.Tracing.Segments;

namespace SkyApm.Transport
{
    partial interface ISegmentDispatcher
    {
        bool Dispatch(TraceSegment traceSegment);
    }
}
