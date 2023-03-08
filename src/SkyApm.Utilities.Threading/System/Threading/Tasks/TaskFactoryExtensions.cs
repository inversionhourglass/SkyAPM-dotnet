using SkyApm.Diagnostics.Delegates;

namespace System.Threading.Tasks
{
    [Obsolete("Use Delegates.Extensions.Diagnostics instead")]
    public static class TaskFactoryExtensions
    {
        /// <inheritdoc cref="TaskFactory.StartNew(Action)"/>
        public static Task StartSkyApmNew(this TaskFactory factory, Action action)
        {
            return factory.StartNew(action.Diagnostic(SkyApmCategory.FORK));
        }

        /// <inheritdoc cref="TaskFactory.StartNew(Action, CancellationToken)"/>
        public static Task StartSkyApmNew(this TaskFactory factory, Action action, CancellationToken cancellationToken)
        {
            return factory.StartNew(action.Diagnostic(SkyApmCategory.FORK), cancellationToken);
        }

        /// <inheritdoc cref="TaskFactory.StartNew(Action, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        public static Task StartSkyApmNew(this TaskFactory factory, Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return factory.StartNew(action.Diagnostic(SkyApmCategory.FORK), cancellationToken, creationOptions, scheduler);
        }

        /// <inheritdoc cref="TaskFactory.StartNew(Action, TaskCreationOptions)"/>
        public static Task StartSkyApmNew(this TaskFactory factory, Action action, TaskCreationOptions creationOptions)
        {
            return factory.StartNew(action.Diagnostic(SkyApmCategory.FORK), creationOptions);
        }

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object?}, object?)"/>
        public static Task StartSkyApmNew(this TaskFactory factory, Action<object> action, object state)
        {
            return factory.StartNew(action.SkySegmentWrap<object>(), state);
        }

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object?}, object?, CancellationToken)"/>
        public static Task StartSkyApmNew(this TaskFactory factory, Action<object> action, object state, CancellationToken cancellationToken)
        {
            return factory.StartNew(action.SkySegmentWrap<object>(), state, cancellationToken);
        }

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object?}, object?, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        public static Task StartSkyApmNew(this TaskFactory factory, Action<object> action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return factory.StartNew(action.SkySegmentWrap<object>(), state, cancellationToken, creationOptions, scheduler);
        }

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object?}, object?, TaskCreationOptions)"/>
        public static Task StartSkyApmNew(this TaskFactory factory, Action<object> action, object state, TaskCreationOptions creationOptions)
        {
            return factory.StartNew(action.SkySegmentWrap<object>(), state, creationOptions);
        }

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object?, TResult}, object?)"/>
        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<object, TResult> function, object state)
        {
            return factory.StartNew(function.Diagnostic(SkyApmCategory.FORK), state);
        }

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object?, TResult}, object?, CancellationToken)"/>
        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<object, TResult> function, object state, CancellationToken cancellationToken)
        {
            return factory.StartNew(function.Diagnostic(SkyApmCategory.FORK), state, cancellationToken);
        }

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object?, TResult}, object?, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return factory.StartNew(function.Diagnostic(SkyApmCategory.FORK), state, cancellationToken, creationOptions, scheduler);
        }

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object?, TResult}, object?, TaskCreationOptions)"/>
        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            return factory.StartNew(function.Diagnostic(SkyApmCategory.FORK), state, creationOptions);
        }

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult})"/>
        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<TResult> function)
        {
            return factory.StartNew(function.Diagnostic(SkyApmCategory.FORK));
        }

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken)"/>
        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<TResult> function, CancellationToken cancellationToken)
        {
            return factory.StartNew(function.Diagnostic(SkyApmCategory.FORK), cancellationToken);
        }

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return factory.StartNew(function.Diagnostic(SkyApmCategory.FORK), cancellationToken, creationOptions, scheduler);
        }

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, TaskCreationOptions)"/>
        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<TResult> function, TaskCreationOptions creationOptions)
        {
            return factory.StartNew(function.Diagnostic(SkyApmCategory.FORK), creationOptions);
        }
    }
}
