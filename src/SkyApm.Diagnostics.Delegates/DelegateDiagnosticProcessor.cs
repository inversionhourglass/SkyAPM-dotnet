using Delegates.Extensions.Diagnostics;
using Delegates.Extensions.Diagnostics.Contexts;
using SkyApm.Tracing;

namespace SkyApm.Diagnostics.Delegates
{
    public class DelegateDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        private readonly ITracingContext _tracingContext;

        public string ListenerName => Diagnostician.LISTENER_NAME;

        public DelegateDiagnosticProcessor(ITracingContext tracingContext)
        {
            _tracingContext = tracingContext;
        }

        [DiagnosticName($"{Diagnostician.LISTENER_NAME}.{SkyApmCategory.CLEAN_CONTEXT}.{Diagnostician.Ids.EXECUTING}")]
        public void OnClearContextExecuting(ExecutingContext context)
        {
            _tracingContext.ClearContext();
        }

        [DiagnosticName($"{Diagnostician.LISTENER_NAME}.{SkyApmCategory.WEAKEN_CONTEXT}.{Diagnostician.Ids.EXECUTING}")]
        public void OnWeakenContextExecuting(ExecutingContext context)
        {
            _tracingContext.WeakenContext();
        }
    }
}
