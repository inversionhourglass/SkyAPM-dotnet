using SkyApm.Common;
using SkyApm.Tracing.Segments;

namespace SkyApm.Tracing
{
    partial interface ISegmentContextFactory
    {
        SegmentContext CurrentEntryContext { get; }

        SegmentContext CurrentLocalContext { get; }

        SegmentContext CurrentExitContext { get; }

        SegmentContext CreateLocalSegment(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = default);

        SegmentContext CreateExitSegment(string operationName, StringOrIntValue networkAddress, CrossThreadCarrier carrier, long startTimeMilliseconds = default);
    }
}
