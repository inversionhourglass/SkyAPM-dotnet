using System;

namespace SkyApm.Tracing.Segments
{
    partial class SegmentSpan
    {
        private const string TAG_INCOMPLETE = "incomplete";
        private const string TAG_ASYNCLINK = "alink";

        private TraceSegment _segment;
        private readonly object _finishLocker = new object();

        /// <summary>
        /// Should only be set by first span of segment
        /// </summary>
        public TraceSegment Segment
        {
            get => _segment ?? Parent.Segment;
            set => _segment = value;
        }

        public SegmentReferenceCollection References { get; } = new SegmentReferenceCollection();

        /// <summary>
        /// A lot of logic uses Parent, think carefully before changing this value
        /// </summary>
        public SegmentSpan Parent { get; set; }

        public void AddLog(SpanLog log)
        {
            Logs.AddLog(log);
        }

        public bool IsAsyncLink()
        {
            return Tags.HasTag(TAG_ASYNCLINK);
        }

        /// <summary>
        /// use this tag can find spans which parent span has been finished
        /// </summary>
        public void AsyncLink()
        {
            Tags.AddTag(TAG_ASYNCLINK, "true");
        }

        public bool IsInComplete()
        {
            return Tags.HasTag(TAG_INCOMPLETE);
        }

        public void InCompleteFinish(long endTimeMilliseconds = default)
        {
            if (EndTime != default) return;
            lock (_finishLocker)
            {
                if (EndTime != default) return;
                EndTime = endTimeMilliseconds == default ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() : endTimeMilliseconds;
                Tags.AddTag(TAG_INCOMPLETE, "true");
            }
        }
    }

    partial class TagCollection
    {
        public void RemoveTag(string key)
        {
            tags.Remove(key);
        }

        public bool HasTag(string key)
        {
            return tags.ContainsKey(key);
        }
    }
}
