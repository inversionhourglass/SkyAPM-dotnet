using SkyApm.Tracing.Segments;
using System;
using System.Threading.Tasks;

namespace SkyApm.Tracing
{
    partial interface ITracingContext
    {
        string TraceId { get; }

        SpanOrSegmentContext CurrentEntry { get; }

        SpanOrSegmentContext CurrentLocal { get; }

        SpanOrSegmentContext CurrentExit { get; }

        SpanOrSegmentContext CreateEntry(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = default);

        SpanOrSegmentContext CreateLocal(string operationName, long startTimeMilliseconds = default);

        SpanOrSegmentContext CreateLocal(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = default);

        SpanOrSegmentContext CreateExit(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default);

        SpanOrSegmentContext CreateExit(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default);

        void Finish(SpanOrSegmentContext spanOrSegmentContext);

        Task ScopeSegmentAsync(Func<Task> func, string operationName = default);

        Task<T> ScopeSegmentAsync<T>(Func<Task<T>> func, string operationName = default);
    }
}
