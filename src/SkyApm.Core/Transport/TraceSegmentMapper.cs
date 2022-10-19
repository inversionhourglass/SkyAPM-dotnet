using System.Collections.Generic;
using SkyApm.Tracing.Segments;

namespace SkyApm.Transport
{
    public class TraceSegmentMapper : ITraceSegmentMapper
    {
        public SegmentRequest Map(TraceSegment traceSegment)
        {
            return Map(traceSegment, false);
        }

        public SegmentRequest MapIfNoAsync(TraceSegment traceSegment)
        {
            return Map(traceSegment, true);
        }

        private SegmentRequest Map(TraceSegment traceSegment, bool nullIfAsync)
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
                string operationName;
                if (span.AsyncDepth < 0)
                {
                    operationName = span.OperationName.ToString();
                }
                else
                {
                    if (nullIfAsync) return null;
                    operationName = $"[async-{span.AsyncDepth}]{span.OperationName}";
                }
                var spanId = span == traceSegment.FirstSpan ? 0 : span.SpanId;
                var parentSpanId = span.ParentSpanId == traceSegment.FirstSpan.SpanId ? 0 : span.ParentSpanId;
                var spanRequest = new SpanRequest
                {
                    SpanId = spanId,
                    ParentSpanId = parentSpanId,
                    OperationName = operationName,
                    StartTime = span.StartTime,
                    EndTime = span.EndTime,
                    SpanType = (int)span.SpanType,
                    SpanLayer = (int)span.SpanLayer,
                    IsError = span.IsError,
                    Peer = span.Peer,
                    Component = span.Component
                };
                if (span == traceSegment.FirstSpan)
                {
                    spanRequest.ParentSpanId = -1;
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