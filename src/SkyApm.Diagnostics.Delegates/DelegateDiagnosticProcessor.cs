using Delegates.Extensions.Diagnostics.Contexts;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using static Delegates.Extensions.Diagnostics.Diagnostician;

namespace SkyApm.Diagnostics.Delegates
{
    public class DelegateDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        private readonly ITracingContext _tracingContext;
        private readonly TracingConfig _tracingConfig;

        public string ListenerName => LISTENER_NAME;

        public DelegateDiagnosticProcessor(ITracingContext tracingContext, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        [DiagnosticName($"{LISTENER_NAME}.{SkyApmCategory.CLEAN_CONTEXT}.{Ids.EXECUTING}")]
        public void OnClearContextExecuting([Object] ExecutingContext context)
        {
            _tracingContext.ClearContext();
        }

        [DiagnosticName($"{LISTENER_NAME}.{SkyApmCategory.WEAKEN_CONTEXT}.{Ids.EXECUTING}")]
        public void OnWeakenContextExecuting([Object] ExecutingContext context)
        {
            _tracingContext.WeakenContext();
        }

        [DiagnosticName($"{LISTENER_NAME}.{SkyApmCategory.FORK}.{Ids.PREPARING}")]
        public void OnForkPreparing([Object] PreparingContext context)
        {
            var prepare = _tracingContext.CreateLocal($"{SkyApmCategory.FORK}-Prepare");
            var carrier = prepare.GetCrossThreadCarrier();

            context.Cabinet["prepare"] = prepare;
            context.Cabinet["carrier"] = carrier;
        }

        [DiagnosticName($"{LISTENER_NAME}.{SkyApmCategory.FORK}.{Ids.PREPARED}")]
        public void OnForkPrepared([Object] PreparedContext context)
        {
            var prepare = (SpanOrSegmentContext)context.Cabinet["prepare"];
            _tracingContext.Finish(prepare);
        }

        [DiagnosticName($"{LISTENER_NAME}.{SkyApmCategory.FORK}.{Ids.EXECUTING}")]
        public void OnForkExecuting([Object] ExecutingContext context)
        {
            var carrier = (CrossThreadCarrier)context.Cabinet["carrier"];
            var local = _tracingContext.CreateLocal(SkyApmCategory.FORK, carrier);
            context.Cabinet["local"] = local;
        }

        [DiagnosticName($"{LISTENER_NAME}.{SkyApmCategory.FORK}.{Ids.EXCEPTIONAL}")]
        public void OnForkExceptional([Object] ExceptionalContext context)
        {
            var local = (SpanOrSegmentContext)context.Cabinet["local"];
            local.Span.ErrorOccurred(context.Exception, _tracingConfig);
        }

        [DiagnosticName($"{LISTENER_NAME}.{SkyApmCategory.FORK}.{Ids.FINISHED}")]
        public void OnForkFinished([Object] FinishedContext context)
        {
            var local = (SpanOrSegmentContext)context.Cabinet["local"];
            _tracingContext.Finish(local);
        }
    }
}
