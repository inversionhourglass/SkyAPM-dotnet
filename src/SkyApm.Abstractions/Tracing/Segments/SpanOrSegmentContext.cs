﻿namespace SkyApm.Tracing.Segments
{
    public class SpanOrSegmentContext
    {
        public SpanOrSegmentContext(SegmentSpan span)
        {
            SegmentSpan = span;
        }

        public SpanOrSegmentContext(SegmentContext segment)
        {
            SegmentContext = segment;
        }

        public SpanOrSegmentContext(SegmentSpan span, SegmentContext segment)
        {
            SegmentSpan = span;
            SegmentContext = segment;
        }

        public SegmentSpan SegmentSpan { get; }

        public SegmentContext SegmentContext { get; }

        public SegmentSpan Span => SegmentContext?.Span ?? SegmentSpan;

        public static implicit operator SpanOrSegmentContext(SegmentSpan span)
        {
            if (span == null) return null;

            return new SpanOrSegmentContext(span);
        }

        public static implicit operator SpanOrSegmentContext(SegmentContext segment)
        {
            if (segment == null) return null;

            return new SpanOrSegmentContext(segment);
        }
    }
}
