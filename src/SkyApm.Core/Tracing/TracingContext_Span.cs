using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Tracing.Segments;
using SkyApm.Transport;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SkyApm.Tracing
{
    partial class TracingContext
    {
        private readonly ITraceSegmentManager _traceSegmentManager;
        private readonly bool _spanable;
        private readonly ILogger _logger;

        public TracingContext(ISegmentContextFactory segmentContextFactory,
            ITraceSegmentManager traceSegmentManager,
            ICarrierPropagator carrierPropagator,
            ISegmentDispatcher segmentDispatcher,
            IConfigAccessor configAccessor,
            ILoggerFactory loggerFactory)
        {
            _segmentContextFactory = segmentContextFactory;
            _traceSegmentManager = traceSegmentManager;
            _carrierPropagator = carrierPropagator;
            _segmentDispatcher = segmentDispatcher;
            _logger = loggerFactory.CreateLogger(typeof(TracingContext));
            _spanable = configAccessor.Get<InstrumentConfig>().Spanable();
        }

        private SegmentSpan ActiveSpan => _traceSegmentManager.ActiveSpan;

        public SpanOrSegmentContext CurrentEntry => _spanable ? (SpanOrSegmentContext)ActiveSpan : _segmentContextFactory.CurrentEntryContext;

        public SpanOrSegmentContext CurrentLocal => _spanable ? (SpanOrSegmentContext)ActiveSpan : _segmentContextFactory.CurrentLocalContext;

        public SpanOrSegmentContext CurrentExit => _spanable ? (SpanOrSegmentContext)ActiveSpan : _segmentContextFactory.CurrentExitContext;

        public SpanOrSegmentContext CreateEntry(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = default)
        {
            return _spanable ?
                (SpanOrSegmentContext)CreateEntrySpan(operationName, carrierHeader, startTimeMilliseconds) :
                CreateEntrySegmentContext(operationName, carrierHeader, startTimeMilliseconds);
        }

        public SpanOrSegmentContext CreateLocal(string operationName, long startTimeMilliseconds = default)
        {
            return _spanable ?
                (SpanOrSegmentContext)CreateLocalSpan(operationName, startTimeMilliseconds) :
                CreateLocalSegmentContext(operationName, startTimeMilliseconds);
        }

        public SpanOrSegmentContext CreateLocal(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = default)
        {
            return _spanable ?
                (SpanOrSegmentContext)CreateLocalSpan(operationName, carrier, startTimeMilliseconds) :
                CreateLocalSegmentContext(operationName, carrier, startTimeMilliseconds);
        }

        public SpanOrSegmentContext CreateExit(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default)
        {
            return _spanable ?
                (SpanOrSegmentContext)CreateExitSpan(operationName, networkAddress, carrierHeader, startTimeMilliseconds) :
                CreateExitSegmentContext(operationName, networkAddress, carrierHeader, startTimeMilliseconds);
        }

        public SpanOrSegmentContext CreateExit(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default)
        {
            return _spanable ?
                (SpanOrSegmentContext)CreateExitSpan(operationName, networkAddress, carrier, carrierHeader, startTimeMilliseconds) :
                CreateExitSegmentContext(operationName, networkAddress, carrier, carrierHeader, startTimeMilliseconds);
        }

        public void Finish(SpanOrSegmentContext spanOrSegmentContext)
        {
            if (CheckStructure(spanOrSegmentContext))
            {
                if (_spanable)
                {
                    StopSpan(spanOrSegmentContext.SegmentSpan);
                }
                else
                {
                    Release(spanOrSegmentContext.SegmentContext);
                }
            }
        }

        public Task ScopeSegmentAsync(Func<Task> func, string operationName = default)
        {
            if (operationName == default) operationName = nameof(SegmentScope);

            var prepare = CreateLocalSpan(P(operationName));
            var carrier = prepare.GetCrossThreadCarrier();
            StopSpan(prepare);
            using (new SegmentScope(this))
            {
                var span = CreateLocalSpan(operationName, carrier);
                var task = func();
                task.ContinueWith(t => StopSpan(span));
                return task;
            }
        }

        public Task<T> ScopeSegmentAsync<T>(Func<Task<T>> func, string operationName = default)
        {
            if (operationName == default) operationName = nameof(SegmentScope);

            var prepare = CreateLocalSpan(P(operationName));
            var carrier = prepare.GetCrossThreadCarrier();
            StopSpan(prepare);
            using (new SegmentScope(this))
            {
                var span = CreateLocalSpan(operationName, carrier);
                var task = func();
                task.ContinueWith(t => StopSpan(span));
                return task;
            }
        }

        private bool CheckStructure(SpanOrSegmentContext spanOrSegmentContext)
        {
            if (_spanable)
            {
                if (spanOrSegmentContext.SegmentSpan != null) return true;

                if (spanOrSegmentContext.SegmentContext != null)
                {
                    _logger.Warning($"try to finish SegmentContext in Span structure mode");
                    _logger.Debug($"try to finish SegmentContext in Span structure mode, operationName: {spanOrSegmentContext.SegmentContext.Span.OperationName}, stackTracing: {Environment.StackTrace}");
                }
            }
            else
            {
                if (spanOrSegmentContext.SegmentContext != null) return true;

                if (spanOrSegmentContext.SegmentSpan != null)
                {
                    _logger.Warning($"try to finish SegmentSpan in Segment structure mode");
                    _logger.Debug($"try to finish SegmentSpan in Segment structure mode, operationName: {spanOrSegmentContext.SegmentSpan.OperationName}, stackTracing: {Environment.StackTrace}");
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string P(string operationName) => operationName + "-Prepare";
        #region SegmentContext

        private SegmentContext CreateLocalSegmentContext(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = default)
        {
            if (operationName == null) throw new ArgumentNullException(nameof(operationName));
            return _segmentContextFactory.CreateLocalSegment(operationName, carrier, startTimeMilliseconds);
        }

        private SegmentContext CreateExitSegmentContext(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default)
        {
            var segmentContext = _segmentContextFactory.CreateExitSegment(operationName, new StringOrIntValue(networkAddress), carrier, startTimeMilliseconds);
            if (carrierHeader != null)
                _carrierPropagator.Inject(segmentContext, carrierHeader);
            return segmentContext;
        }

        #endregion SegmentContext

        #region SegmentSpan

        public SegmentSpan CreateEntrySpan(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = 0)
        {
            if (operationName == null) throw new ArgumentNullException(nameof(operationName));
            var carrier = _carrierPropagator.Extract(carrierHeader);
            return _traceSegmentManager.CreateEntrySpan(operationName, carrier, startTimeMilliseconds);
        }

        public SegmentSpan CreateLocalSpan(string operationName, long startTimeMilliseconds = 0)
        {
            if (operationName == null) throw new ArgumentNullException(nameof(operationName));
            return _traceSegmentManager.CreateLocalSpan(operationName, startTimeMilliseconds);
        }

        public SegmentSpan CreateLocalSpan(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = 0)

        {
            if (operationName == null) throw new ArgumentNullException(nameof(operationName));
            return _traceSegmentManager.CreateLocalSpan(operationName, carrier, startTimeMilliseconds);

        }

        public SegmentSpan CreateExitSpan(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            var span = _traceSegmentManager.CreateExitSpan(operationName, new StringOrIntValue(networkAddress), startTimeMilliseconds);
            if (carrierHeader != null) _carrierPropagator.Inject(span, carrierHeader);
            return span;
        }

        public SegmentSpan CreateExitSpan(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            var span = _traceSegmentManager.CreateExitSpan(operationName, new StringOrIntValue(networkAddress), carrier, startTimeMilliseconds);
            if (carrierHeader != null) _carrierPropagator.Inject(span, carrierHeader);
            return span;
        }

        public void StopSpan(SegmentSpan span)
        {
            var segment = _traceSegmentManager.StopSpan(span);

            if (segment != null && segment.Sampled)
            {
                _segmentDispatcher.Dispatch(segment);
            }
        }

        public void StopSpan()
        {
            (var segment, _) = _traceSegmentManager.StopSpan();

            if (segment != null && segment.Sampled)
            {
                _segmentDispatcher.Dispatch(segment);
            }
        }
        #endregion SegmentSpan

        class SegmentScope : IDisposable
        {
            private readonly TracingContext _context;
            private readonly TraceSegmentManager.SegmentStash _stash;

            public SegmentScope(TracingContext context)
            {
                _context = context;
                _stash = ((TraceSegmentManager)_context._traceSegmentManager).CreateStash();
                _stash.Stash();
            }

            public void Dispose()
            {
                _stash.Restore();
            }
        }

        class NullDisposable : IDisposable
        {
            public void Dispose()
            {
                
            }
        }
    }
}
