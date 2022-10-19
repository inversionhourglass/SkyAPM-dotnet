using SkyApm.Tracing.Segments;

namespace SkyApm.Tracing
{
    partial class CarrierPropagator
    {
        public void Inject(SegmentSpan span, ICarrierHeaderCollection headerCollection)
        {
            var segment = span.Segment;
            var carrier = new Carrier(segment.TraceId, segment.SegmentId, span.SpanId,
                segment.ServiceInstanceId, null,
                segment.ServiceId)
            {
                NetworkAddress = span.Peer,
                EntryEndpoint = null,
                ParentEndpoint = segment.FirstSpan.OperationName,
                Sampled = segment.Sampled
            };

            foreach (var formatter in _carrierFormatters)
            {
                if (formatter.Enable)
                    headerCollection.Add(formatter.Key, formatter.Encode(carrier));
            }
        }
    }
}
