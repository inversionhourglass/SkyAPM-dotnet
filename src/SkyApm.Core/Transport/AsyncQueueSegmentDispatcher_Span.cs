using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Transport
{
    partial class AsyncQueueSegmentDispatcher
    {
        private readonly SpanStructureConfig _spanConfig;
        private readonly IAsyncSpanCombiner _asyncSpanCombiner;
        private readonly ITraceSegmentMapper _traceSegmentMapper;
        private readonly ConcurrentQueue<TraceSegment> _mergeQueue;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, TraceSegment>> _mergeDictionary;
        private Task _mergeTask;
        private int _mergeCount;

        public bool Dispatch(TraceSegment traceSegment)
        {
            if (!_runtimeEnvironment.Initialized || traceSegment == null || !traceSegment.Sampled)
                return false;

            // todo performance optimization for ConcurrentQueue
            if (_config.QueueSize < _offset || _cancellation.IsCancellationRequested)
                return false;

            var segment = _traceSegmentMapper.MapIfNoAsync(traceSegment);

            if (segment == null)
            {
                var mergeEnqueue = Merge(traceSegment);
                if (mergeEnqueue) return true;

                segment = _traceSegmentMapper.Map(traceSegment);
            }

            Enqueue(segment);

            return true;
        }

        private bool Merge(TraceSegment segment)
        {
            var count = Interlocked.Increment(ref _mergeCount);
            if (_cancellation.IsCancellationRequested || count >= _spanConfig.MergeQueueSize ||
                segment.FirstSpan.EndTime + _spanConfig.MergeDelay <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            {
                Interlocked.Decrement(ref _mergeCount);
                return false;
            }
            var segmentDictionary = _mergeDictionary.GetOrAdd(segment.TraceId, traceId =>
            {
                var dictionary = new ConcurrentDictionary<string, TraceSegment>();
                dictionary.TryAdd(segment.SegmentId, segment);
                return dictionary;
            });
            if (segmentDictionary.ContainsKey(segment.SegmentId))
            {
                _mergeQueue.Enqueue(segment);
            }
            else
            {
                segmentDictionary.TryAdd(segment.SegmentId, segment);
            }
            return true;
        }

        private async Task BackgroundMerge()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                if (!_mergeQueue.TryDequeue(out var segment))
                {
                    try
                    {
                        await Task.Delay(_spanConfig.MergeDelay, _cancellation.Token);
                        continue;
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }

                var deadline = segment.FirstSpan.EndTime + _spanConfig.MergeDelay;
                var current = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var delay = (int)(deadline - current);
                if (delay > 100)
                {
                    try
                    {
                        await Task.Delay(delay, _cancellation.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        _mergeQueue.Enqueue(segment);
                        break;
                    }
                }

                if (!_mergeDictionary.TryRemove(segment.TraceId, out var segments)) continue;

                MergeAndEnqueue(segments.Values);
            }

            foreach (var traceId in _mergeDictionary.Keys.ToArray())
            {
                if (_mergeDictionary.TryRemove(traceId, out var segments))
                {
                    MergeAndEnqueue(segments.Values);
                }
            }
        }

        private void MergeAndEnqueue(IEnumerable<TraceSegment> segments)
        {
            var mergedSegments = _asyncSpanCombiner.Merge(segments);
            foreach (var mergedSegment in mergedSegments)
            {
                if (mergedSegment == null) continue;

                var segmentRequest = _traceSegmentMapper.Map(mergedSegment);

                Enqueue(segmentRequest);
            }
        }

        private void Enqueue(SegmentRequest segment)
        {
            _segmentQueue.Enqueue(segment);

            Interlocked.Increment(ref _offset);

            _logger.Debug($"Dispatch trace segment. [SegmentId]={segment.Segment.SegmentId}.");
        }
    }
}
