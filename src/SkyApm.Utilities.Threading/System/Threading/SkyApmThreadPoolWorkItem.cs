using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using SkyApm.Utilities.StaticAccessor;

namespace System.Threading
{
    [Obsolete("Use Delegates.Extensions.Diagnostics instead")]
    public class SkyApmThreadPoolWorkItem : IThreadPoolWorkItem
    {
        private readonly string _operationName;
        private readonly IThreadPoolWorkItem _item;
        private readonly CrossThreadCarrier _carrier;

        public SkyApmThreadPoolWorkItem(IThreadPoolWorkItem item, string operationName = SkyApmDelegateExtensions.WRAP_NAME)
        {
            _operationName = operationName;
            var prepare = SkyApmInstances.TracingContext.CreateLocal(SkyApmDelegateExtensions.P(operationName));
            _carrier = prepare.GetCrossThreadCarrier();

            _item = item;

            SkyApmInstances.TracingContext.Finish(prepare);
        }

        public void Execute()
        {
            var local = SkyApmInstances.TracingContext.CreateLocal(_operationName, _carrier);
            try
            {
                _item.Execute();
            }
            catch (Exception ex)
            {
                local.Span.ErrorOccurred(ex, SkyApmInstances.TracingConfig);
                throw ex;
            }
            finally
            {
                SkyApmInstances.TracingContext.Finish(local);
            }
        }
    }
}
