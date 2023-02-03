using SkyApm.Tracing.Segments;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SkyApm.Tracing
{
    internal class WideTraceSegment : TraceSegment
    {
        private readonly AsyncLocal<int?> _spanTree = new AsyncLocal<int?>();
        private readonly ConcurrentDictionary<int, SegmentSpan> _spans = new ConcurrentDictionary<int, SegmentSpan>();
        private volatile int _spanId = -1;
        private volatile int _incompleteSpans;
        private volatile int _completed;

        public WideTraceSegment(string traceId, string segmentId, bool sampled, string serviceId, string serviceInstanceId)
            : base(traceId, segmentId, sampled, serviceId, serviceInstanceId)
        {
        }

        public SegmentSpan CurrentSpan
        {
            get
            {
                var spanId = _spanTree.Value;
                if (!spanId.HasValue) return null;

                return _spans.TryGetValue(spanId.Value, out var span) ? span : null;
            }
        }

        public int IncompleteSpans => _incompleteSpans;

        public long FinishTimestamp { get; private set; }

        /// <summary>
        /// return null if parent span has been completed.
        /// </summary>
        public SegmentSpan CreateEntrySpan(string operationName, long startTimeMilliseconds = default)
        {
            var spanId = NextSpanId();
            var spanType = spanId == 0 ? SpanType.Entry : SpanType.Local;

            return CreateSpan(spanId, operationName, spanType, startTimeMilliseconds);
        }

        /// <summary>
        /// return null if parent span has been completed.
        /// </summary>
        public SegmentSpan CreateLocalSpan(string operationName, long startTimeMilliseconds = default)
        {
            var spanId = NextSpanId();

            return CreateSpan(spanId, operationName, SpanType.Local, startTimeMilliseconds);
        }

        /// <summary>
        /// return null if parent span has been completed.
        /// </summary>
        public SegmentSpan CreateExitSpan(string operationName, long startTimeMilliseconds = default)
        {
            var spanId = NextSpanId();

            return CreateSpan(spanId, operationName, SpanType.Exit, startTimeMilliseconds);
        }

        private SegmentSpan CreateSpan(int spanId, string operationName, SpanType spanType, long startTimeMilliseconds = default)
        {
            if (_completed == 1) return null;

            SegmentSpan parentSpan = null;
            if (_spanTree.Value.HasValue)
            {
                var parentSpanId = _spanTree.Value.Value;
                if (!_spans.TryGetValue(parentSpanId, out parentSpan)) return null;
            }

            Interlocked.Increment(ref _incompleteSpans);
            var span = new SegmentSpan(operationName, spanType, startTimeMilliseconds)
            {
                SpanId = spanId
            };
            if (parentSpan != null)
            {
                span.Parent = parentSpan;
            }
            _spans.TryAdd(spanId, span);
            _spanTree.Value = spanId;
            if (spanId == 0) FirstSpan = span;

            return span;
        }

        public WideTraceSegment Finish(SegmentSpan span, long endTimeMilliseconds = default)
        {
            if (!_spans.TryGetValue(span.SpanId, out var storedSpan) || span != storedSpan) return null;
            if (!_spans.TryRemove(span.SpanId, out _)) return null;

            _spanTree.Value = span.Parent?.SpanId;
            span.Finish(endTimeMilliseconds);
            Interlocked.Decrement(ref _incompleteSpans);

            if (span.Parent == null || _completed == 1 && !_spans.TryGetValue(span.ParentSpanId, out _))
            {
                FinishTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return this;
            }
            return null;
        }

        public TraceSegment ToTraceSegment()
        {
            Interlocked.Exchange(ref _completed, 1);
            var segment = new TraceSegment(TraceId, SegmentId, Sampled, ServiceId, ServiceInstanceId);
            segment.Spans.AddRange(_spans.Values);
            return segment;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int NextSpanId() => Interlocked.Increment(ref _spanId);
    }
}
