﻿using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Threading;

namespace SkyApm.Transport
{
    partial class AsyncQueueSegmentDispatcher
    {
        private readonly SpanStructureConfig _spanConfig;
        private readonly ITraceSegmentMapper _traceSegmentMapper;

        private readonly ConcurrentPriorityQueue<TimeSequencedSegment> _segments = new ConcurrentPriorityQueue<TimeSequencedSegment>();

        public bool Dispatch(TraceSegment traceSegment)
        {
            if (!_runtimeEnvironment.Initialized || traceSegment == null || !traceSegment.Sampled)
                return false;

            // todo performance optimization for ConcurrentQueue
            if (_cancellation.IsCancellationRequested)
                return false;

            if (traceSegment is WideTraceSegment segment)
            {
                if (segment.IncompleteSpans > 0 && _spanConfig.DelaySeconds > 0)
                {
                    var sequencedSegment = new TimeSequencedSegment(segment, _spanConfig.DelaySeconds);
                    _segments.Enqueue(sequencedSegment);
                    return true;
                }
                traceSegment = segment.ToTraceSegment();
            }

            Enqueue(traceSegment);

            return true;
        }

        public void DelayInspect(CancellationToken token)
        {
            while (_segments.TryPeek(out var segmentItem) && segmentItem.Deadline <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                && !token.IsCancellationRequested)
            {
                _segments.TryDequeue(out _);
                var segment = segmentItem.Segment;
                if (segment == null) continue;

                var traceSegment = segment.ToTraceSegment();

                Enqueue(traceSegment);
            }
        }

        private void Enqueue(TraceSegment traceSegment)
        {
            if (_config.QueueSize < _offset) return;

            var segmentRequest = _traceSegmentMapper.Map(traceSegment);

            _segmentQueue.Enqueue(segmentRequest);

            Interlocked.Increment(ref _offset);

            _logger.Debug($"Dispatch trace segment. [SegmentId]={segmentRequest.Segment.SegmentId}.");
        }
    }
}
