using SkyApm.Common;
using SkyApm.Tracing.Segments;

namespace SkyApm.Tracing
{
    partial class SegmentContextFactory
    {
        public SegmentContext CurrentEntryContext => _entrySegmentContextAccessor.Context;

        public SegmentContext CurrentLocalContext => _localSegmentContextAccessor.Context;

        public SegmentContext CurrentExitContext => _exitSegmentContextAccessor.Context;

        public SegmentContext CreateLocalSegment(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = default)
        {
            if (carrier == null) return CreateLocalSegment(operationName, startTimeMilliseconds);

            var traceId = GetTraceId(carrier);
            var segmentId = GetSegmentId();
            var sampled = GetSampled(carrier, operationName);
            var segmentContext = new SegmentContext(traceId, segmentId, sampled,
                _instrumentConfig.ServiceName ?? _instrumentConfig.ApplicationCode,
                _instrumentConfig.ServiceInstanceName,
                operationName, SpanType.Local, startTimeMilliseconds);

            segmentContext.References.Add(carrier);

            _localSegmentContextAccessor.Context = segmentContext;
            return segmentContext;
        }

        public SegmentContext CreateExitSegment(string operationName, StringOrIntValue networkAddress, CrossThreadCarrier carrier, long startTimeMilliseconds = default)
        {
            if (carrier == null) CreateExitSegment(operationName, networkAddress, startTimeMilliseconds);

            var traceId = GetTraceId(carrier);
            var segmentId = GetSegmentId();
            var sampled = GetSampled(carrier, operationName);
            var segmentContext = new SegmentContext(traceId, segmentId, sampled,
                _instrumentConfig.ServiceName ?? _instrumentConfig.ApplicationCode,
                _instrumentConfig.ServiceInstanceName,
                operationName, SpanType.Exit, startTimeMilliseconds);

            segmentContext.References.Add(carrier);

            segmentContext.Span.Peer = networkAddress;
            _exitSegmentContextAccessor.Context = segmentContext;
            return segmentContext;
        }
    }
}
