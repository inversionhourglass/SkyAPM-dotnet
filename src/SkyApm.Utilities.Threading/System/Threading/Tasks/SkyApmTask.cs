using SkyApm.Diagnostics.Delegates;

namespace System.Threading.Tasks
{
    [Obsolete("Use Delegates.Extensions.Diagnostics instead")]
    public class SkyApmTask : Task
    {
        public SkyApmTask(Action action) : base(action.Diagnostic(SkyApmCategory.FORK))
        {
        }

        public SkyApmTask(Action action, CancellationToken cancellationToken) : base(action.Diagnostic(SkyApmCategory.FORK), cancellationToken)
        {
        }

        public SkyApmTask(Action action, TaskCreationOptions creationOptions) : base(action.Diagnostic(SkyApmCategory.FORK), creationOptions)
        {
        }

        public SkyApmTask(Action<object> action, object state) : base(action.SkySegmentWrap<object>(), state)
        {
        }

        public SkyApmTask(Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(action.Diagnostic(SkyApmCategory.FORK), cancellationToken, creationOptions)
        {
        }

        public SkyApmTask(Action<object> action, object state, CancellationToken cancellationToken) : base(action.SkySegmentWrap<object>(), state, cancellationToken)
        {
        }

        public SkyApmTask(Action<object> action, object state, TaskCreationOptions creationOptions) : base(action.SkySegmentWrap<object>(), state, creationOptions)
        {
        }

        public SkyApmTask(Action<object> action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(action.SkySegmentWrap<object>(), state, cancellationToken, creationOptions)
        {
        }

        /// <inheritdoc cref="Task.Run(Action)"/>
        public static new Task Run(Action action)
        {
            return Task.Run(action.Diagnostic(SkyApmCategory.FORK));
        }

        /// <inheritdoc cref="Task.Run(Action, CancellationToken)"/>
        public static new Task Run(Action action, CancellationToken cancellationToken)
        {
            return Task.Run(action.Diagnostic(SkyApmCategory.FORK), cancellationToken);
        }

        /// <inheritdoc cref="Task.Run(Func{Task?})"/>
        public static new Task Run(Func<Task> function)
        {
            return Task.Run(function.Diagnostic(SkyApmCategory.FORK));
        }

        /// <inheritdoc cref="Task.Run(Func{Task?}, CancellationToken)"/>
        public static new Task Run(Func<Task> function, CancellationToken cancellationToken)
        {
            return Task.Run(function.Diagnostic(SkyApmCategory.FORK), cancellationToken);
        }

        /// <inheritdoc cref="Task.Run{TResult}(Func{Task{TResult}?})"/>
        public static new Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            return Task.Run(function.Diagnostic(SkyApmCategory.FORK));
        }

        /// <inheritdoc cref="Task.Run{TResult}(Func{Task{TResult}?}, CancellationToken)"/>
        public static new Task<TResult> Run<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken)
        {
            return Task.Run(function.Diagnostic(SkyApmCategory.FORK), cancellationToken);
        }

        /// <inheritdoc cref="Task.Run{TResult}(Func{TResult})"/>
        public static new Task<TResult> Run<TResult>(Func<TResult> function)
        {
            return Task.Run(function.Diagnostic(SkyApmCategory.FORK));
        }

        /// <inheritdoc cref="Task.Run{TResult}(Func{TResult}, CancellationToken)"/>
        public static new Task<TResult> Run<TResult>(Func<TResult> function, CancellationToken cancellationToken)
        {
            return Task.Run(function.Diagnostic(SkyApmCategory.FORK), cancellationToken);
        }
    }
}
