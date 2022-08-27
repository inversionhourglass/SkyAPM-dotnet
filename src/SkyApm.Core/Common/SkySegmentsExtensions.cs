using SkyApm.Common;

namespace SkyApm.Tracing.Segments
{
    public static class SkySegmentsExtensions
    {

        public static CrossThreadCarrier GetCrossThreadCarrier(this SegmentContext segmentContext)
        {
            if (segmentContext == null) return null;

            return new CrossThreadCarrier
            {
                Reference = Reference.CrossThread,
                TraceId = segmentContext.TraceId,
                ParentSegmentId = segmentContext.SegmentId,
                ParentSpanId = segmentContext.Span.SpanId,
                ParentServiceId = segmentContext.ServiceId,
                ParentServiceInstanceId = segmentContext.ServiceInstanceId,
                ParentEndpoint = segmentContext.Span.OperationName,
                Sampled = segmentContext.Sampled,
                NetworkAddress = DnsHelpers.GetIpV4OrHostName()
            };
        }

        public static CrossThreadCarrier GetCrossThreadCarrier(this SpanOrSegmentContext spanOrSegmentContext)
        {
            if (spanOrSegmentContext == null) return null;

            return GetCrossThreadCarrier(spanOrSegmentContext.SegmentContext);
        }
    }
}
