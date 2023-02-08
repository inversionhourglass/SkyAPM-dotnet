using SkyApm.Tracing;
using System;

namespace SkyApm.Transport
{
    internal class DelaySegment
    {
        private readonly WeakReference<WideTraceSegment> _segment;

        public DelaySegment(WideTraceSegment segment, int delaySeconds)
        {
            _segment = new WeakReference<WideTraceSegment>(segment);
            Deadline = segment.FinishTimestamp + delaySeconds * 1000;
        }

        public long Deadline { get; }

        public WideTraceSegment Segment => _segment.TryGetTarget(out var target) ? target : null;
    }
}
