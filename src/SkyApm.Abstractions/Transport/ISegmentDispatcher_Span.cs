using SkyApm.Tracing.Segments;
using System.Threading;

namespace SkyApm.Transport
{
    partial interface ISegmentDispatcher
    {
        bool Dispatch(TraceSegment traceSegment);

        void DelayInspect(CancellationToken token);
    }
}
