using SkyApm.Tracing;
using System;

namespace SkyApm.Transport
{
    internal class TimeSequencedSegment : IComparable<TimeSequencedSegment>
    {
        private readonly WeakReference<WideTraceSegment> _segment;

        public TimeSequencedSegment(WideTraceSegment segment, int delaySeconds)
        {
            _segment = new WeakReference<WideTraceSegment>(segment);
            Deadline = segment.FinishTimestamp + delaySeconds * 1000;
        }

        public long Deadline { get; }

        public WideTraceSegment Segment => _segment.TryGetTarget(out var target) ? target : null;

        public int CompareTo(TimeSequencedSegment other)
        {
            return Deadline.CompareTo(other.Deadline);
        }
    }
}
