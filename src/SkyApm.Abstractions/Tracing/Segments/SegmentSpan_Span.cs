using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Tracing.Segments
{
    partial class SegmentSpan
    {
        private TraceSegment _segment;

        public int AsyncDepth { get; set; } = -1;

        public string SpanPath => Parent == null ? SpanId.ToString() : $"{Parent.SpanPath},{SpanId}";

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

        public ConcurrentDictionary<int, SegmentSpan> Children { get; } = new ConcurrentDictionary<int, SegmentSpan>();

        public void AddLog(SpanLog log)
        {
            Logs.AddLog(log);
        }
    }
}
