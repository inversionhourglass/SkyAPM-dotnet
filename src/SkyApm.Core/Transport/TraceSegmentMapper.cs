using System.Collections.Generic;
using SkyApm.Config;
using SkyApm.Tracing.Segments;

namespace SkyApm.Transport
{
    public class TraceSegmentMapper : ITraceSegmentMapper
    {
        private readonly bool _incompleteAsError;

        public TraceSegmentMapper(IConfigAccessor configAccessor)
        {
            _incompleteAsError = configAccessor.Get<SpanableConfig>().IncompleteAsError;
        }

        public SegmentRequest Map(TraceSegment traceSegment)
        {
            var segmentRequest = new SegmentRequest
            {
                TraceId = traceSegment.TraceId
            };
            var segmentObjectRequest = new SegmentObjectRequest
            {
                SegmentId = traceSegment.SegmentId,
                ServiceId = traceSegment.ServiceId,
                ServiceInstanceId = traceSegment.ServiceInstanceId
            };
            segmentRequest.Segment = segmentObjectRequest;
            foreach (var span in traceSegment.Spans)
            {
                var spanRequest = new SpanRequest
                {
                    SpanId = span.SpanId,
                    ParentSpanId = span.ParentSpanId,
                    OperationName = span.OperationName,
                    StartTime = span.StartTime,
                    EndTime = span.EndTime,
                    SpanType = (int)span.SpanType,
                    SpanLayer = (int)span.SpanLayer,
                    IsError = span.IsError,
                    Peer = span.Peer,
                    Component = span.Component
                };
                if (!spanRequest.IsError && _incompleteAsError && span.IsInComplete())
                {
                    spanRequest.IsError = true;
                }
                foreach (var reference in span.References)
                    spanRequest.References.Add(new SegmentReferenceRequest
                    {
                        TraceId = reference.TraceId,
                        ParentSegmentId = reference.ParentSegmentId,
                        ParentServiceId = reference.ParentServiceId,
                        ParentServiceInstanceId = reference.ParentServiceInstanceId,
                        ParentSpanId = reference.ParentSpanId,
                        ParentEndpointName = reference.ParentEndpoint,
                        EntryServiceInstanceId = reference.EntryServiceInstanceId,
                        EntryEndpointName = reference.EntryEndpoint,
                        NetworkAddress = reference.NetworkAddress,
                        RefType = (int)reference.Reference
                    });

                foreach (var tag in span.Tags)
                    spanRequest.Tags.Add(new KeyValuePair<string, string>(tag.Key, tag.Value));

                foreach (var log in span.Logs)
                {
                    var logData = new LogDataRequest { Timestamp = log.Timestamp };
                    foreach (var data in log.Data)
                        logData.Data.Add(new KeyValuePair<string, string>(data.Key, data.Value));
                    spanRequest.Logs.Add(logData);
                }

                segmentObjectRequest.Spans.Add(spanRequest);
            }

            return segmentRequest;
        }
    }
}