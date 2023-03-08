using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using SkyApm.Utilities.StaticAccessor;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use Delegates.Extensions.Diagnostics instead")]
    public static class SkyApmDelegateExtensions
    {
        internal const string WRAP_NAME = "LocalFork";

        #region Action
        public static Action SkySegmentWrap(this Action action, string operationName = WRAP_NAME)
        {
            if (action == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(P(operationName));
            var carrier = prepare.GetCrossThreadCarrier();

            Action apmAction = () =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal(operationName, carrier);
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    local.Span.ErrorOccurred(ex, SkyApmInstances.TracingConfig);
                    throw;
                }
                finally
                {
                    SkyApmInstances.TracingContext.Finish(local);
                }
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmAction;
        }

        public static Action<object> SkySegmentWrap(this Action<object> action, string operationName = WRAP_NAME)
        {
            if (action == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(P(operationName));
            var carrier = prepare.GetCrossThreadCarrier();

            Action<object> apmAction = obj =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal(operationName, carrier);
                try
                {
                    action(obj);
                }
                catch (Exception ex)
                {
                    local.Span.ErrorOccurred(ex, SkyApmInstances.TracingConfig);
                    throw;
                }
                finally
                {
                    SkyApmInstances.TracingContext.Finish(local);
                }
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmAction;
        }

        public static Action<TState> SkySegmentWrap<TState>(this Action<TState> action, string operationName = WRAP_NAME)
        {
            if (action == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(P(operationName));
            var carrier = prepare.GetCrossThreadCarrier();

            Action<TState> apmAction = obj =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal(operationName, carrier);
                try
                {
                    action(obj);
                }
                catch (Exception ex)
                {
                    local.Span.ErrorOccurred(ex, SkyApmInstances.TracingConfig);
                    throw;
                }
                finally
                {
                    SkyApmInstances.TracingContext.Finish(local);
                }
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmAction;
        }
        #endregion Action

        #region Func
        public static Func<TResult> SkySegmentWrap<TResult>(this Func<TResult> func, string operationName = WRAP_NAME)
        {
            if (func == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(P(operationName));
            var carrier = prepare.GetCrossThreadCarrier();

            Func<TResult> apmAction = () =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal(operationName, carrier);
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    local.Span.ErrorOccurred(ex, SkyApmInstances.TracingConfig);
                    throw;
                }
                finally
                {
                    SkyApmInstances.TracingContext.Finish(local);
                }
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmAction;
        }

        public static Func<object, TResult> SkySegmentWrap<TResult>(this Func<object, TResult> func, string operationName = WRAP_NAME)
        {
            if (func == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(P(operationName));
            var carrier = prepare.GetCrossThreadCarrier();

            Func<object, TResult> apmAction = obj =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal(operationName, carrier);
                try
                {
                    return func(obj);
                }
                catch (Exception ex)
                {
                    local.Span.ErrorOccurred(ex, SkyApmInstances.TracingConfig);
                    throw;
                }
                finally
                {
                    SkyApmInstances.TracingContext.Finish(local);
                }
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmAction;
        }

        public static Func<Task> SkySegmentWrap(this Func<Task> func, string operationName = WRAP_NAME)
        {
            if (func == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(P(operationName));
            var carrier = prepare.GetCrossThreadCarrier();

            Func<Task> apmAction = async () =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal(operationName, carrier);
                try
                {
                    await func();
                }
                catch (Exception ex)
                {
                    local.Span.ErrorOccurred(ex, SkyApmInstances.TracingConfig);
                    throw;
                }
                finally
                {
                    SkyApmInstances.TracingContext.Finish(local);
                }
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmAction;
        }

        public static Func<Task<TResult>> SkySegmentWrap<TResult>(this Func<Task<TResult>> func, string operationName = WRAP_NAME)
        {
            if (func == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(P(operationName));
            var carrier = prepare.GetCrossThreadCarrier();

            Func<Task<TResult>> apmAction = async () =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal(operationName, carrier);
                try
                {
                    return await func();
                }
                catch (Exception ex)
                {
                    local.Span.ErrorOccurred(ex, SkyApmInstances.TracingConfig);
                    throw;
                }
                finally
                {
                    SkyApmInstances.TracingContext.Finish(local);
                }
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmAction;
        }
        #endregion Func

        #region ThreadStart
        public static ThreadStart SkySegmentWrap(this ThreadStart threadStart, string operationName = WRAP_NAME)
        {
            if (threadStart == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(P(operationName));
            var carrier = prepare.GetCrossThreadCarrier();

            ThreadStart apmThreadStart = () =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal(operationName, carrier);
                try
                {
                    threadStart();
                }
                catch (Exception ex)
                {
                    local.Span.ErrorOccurred(ex, SkyApmInstances.TracingConfig);
                    throw;
                }
                finally
                {
                    SkyApmInstances.TracingContext.Finish(local);
                }
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmThreadStart;
        }
        #endregion ThreadStart

        #region ParameterizedThreadStart
        public static ParameterizedThreadStart SkySegmentWrap(this ParameterizedThreadStart threadStart, string operationName = WRAP_NAME)
        {
            if (threadStart == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(P(operationName));
            var carrier = prepare.GetCrossThreadCarrier();

            ParameterizedThreadStart apmThreadStart = obj =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal(operationName, carrier);
                try
                {
                    threadStart(obj);
                }
                catch (Exception ex)
                {
                    local.Span.ErrorOccurred(ex, SkyApmInstances.TracingConfig);
                    throw;
                }
                finally
                {
                    SkyApmInstances.TracingContext.Finish(local);
                }
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmThreadStart;
        }
        #endregion ParameterizedThreadStart

        #region WaitCallback
        public static WaitCallback SkySegmentWrap(this WaitCallback callback, string operationName = WRAP_NAME)
        {
            if (callback == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(P(operationName));
            var carrier = prepare.GetCrossThreadCarrier();

            WaitCallback apmCallback = obj =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal(operationName, carrier);
                try
                {
                    callback(obj);
                }
                catch (Exception ex)
                {
                    local.Span.ErrorOccurred(ex, SkyApmInstances.TracingConfig);
                    throw;
                }
                finally
                {
                    SkyApmInstances.TracingContext.Finish(local);
                }
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmCallback;
        }
        #endregion WaitCallback

        #region TimerCallback
        public static TimerCallback SkySegmentWrap(this TimerCallback callback, string operationName = WRAP_NAME)
        {
            if (callback == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(P(operationName));
            var carrier = prepare.GetCrossThreadCarrier();

            TimerCallback apmCallback = obj =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal(operationName, carrier);
                try
                {
                    callback(obj);
                }
                catch (Exception ex)
                {
                    local.Span.ErrorOccurred(ex, SkyApmInstances.TracingConfig);
                    throw;
                }
                finally
                {
                    SkyApmInstances.TracingContext.Finish(local);
                }
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmCallback;
        }
        #endregion TimerCallback

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string P(string operationName) => operationName + "-Prepare";
    }
}
