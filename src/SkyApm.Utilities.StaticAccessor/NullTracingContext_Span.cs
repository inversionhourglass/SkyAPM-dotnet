using SkyApm.Tracing.Segments;
using SkyApm.Tracing;

namespace SkyApm.Utilities.StaticAccessor
{
    partial class NullTracingContext
    {
        public string? TraceId => null;

        public string? SegmentId => null;

        public SegmentSpan ActiveSpan => NullInstances.SegmentSpan;

        public SpanOrSegmentContext CurrentEntry => NullInstances.SpanOrSegmentContext;

        public SpanOrSegmentContext CurrentLocal => NullInstances.SpanOrSegmentContext;

        public SpanOrSegmentContext CurrentExit => NullInstances.SpanOrSegmentContext;

        public SpanOrSegmentContext CreateEntry(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = 0)
        {
            return NullInstances.SpanOrSegmentContext;
        }

        public SpanOrSegmentContext CreateExit(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            return NullInstances.SpanOrSegmentContext;
        }

        public SpanOrSegmentContext CreateExit(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            return NullInstances.SpanOrSegmentContext;
        }

        public SpanOrSegmentContext CreateLocal(string operationName, long startTimeMilliseconds = 0)
        {
            return NullInstances.SpanOrSegmentContext;
        }

        public SpanOrSegmentContext CreateLocal(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = 0)
        {
            return NullInstances.SpanOrSegmentContext;
        }

        public void Finish(SpanOrSegmentContext spanOrSegmentContext)
        {

        }
    }
}
