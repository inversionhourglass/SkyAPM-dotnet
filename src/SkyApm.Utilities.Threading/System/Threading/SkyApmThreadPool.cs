namespace System.Threading
{
    [Obsolete("Use Delegates.Extensions.Diagnostics instead")]
    public static class SkyApmThreadPool
    {
        /// <inheritdoc cref="ThreadPool.QueueUserWorkItem(WaitCallback)"/>
        public static bool QueueUserWorkItem(WaitCallback callBack)
        {
            return ThreadPool.QueueUserWorkItem(callBack.SkySegmentWrap());
        }

        /// <inheritdoc cref="ThreadPool.QueueUserWorkItem(WaitCallback, object?)"/>
        public static bool QueueUserWorkItem(WaitCallback callBack, object state)
        {
            return ThreadPool.QueueUserWorkItem(callBack.SkySegmentWrap(), state);
        }

        /// <inheritdoc cref="ThreadPool.QueueUserWorkItem{TState}(Action{TState}, TState, bool)"/>
        public static bool QueueUserWorkItem<TState>(Action<TState> callBack, TState state, bool preferLocal)
        {
            return ThreadPool.QueueUserWorkItem(callBack.SkySegmentWrap(), state, preferLocal);
        }

        /// <inheritdoc cref="ThreadPool.UnsafeQueueUserWorkItem(IThreadPoolWorkItem, bool)"/>
        public static bool UnsafeQueueUserWorkItem(IThreadPoolWorkItem callBack, bool preferLocal)
        {
            if (!(callBack is SkyApmThreadPoolWorkItem)) callBack = new SkyApmThreadPoolWorkItem(callBack);
            return ThreadPool.UnsafeQueueUserWorkItem(callBack, preferLocal);
        }

        /// <inheritdoc cref="ThreadPool.UnsafeQueueUserWorkItem(WaitCallback, object?)"/>
        public static bool UnsafeQueueUserWorkItem(WaitCallback callBack, object state)
        {
            return ThreadPool.UnsafeQueueUserWorkItem(callBack.SkySegmentWrap(), state);
        }

        /// <inheritdoc cref="ThreadPool.UnsafeQueueUserWorkItem{TState}(Action{TState}, TState, bool)"/>
        public static bool UnsafeQueueUserWorkItem<TState>(Action<TState> callBack, TState state, bool preferLocal)
        {
            return ThreadPool.UnsafeQueueUserWorkItem(callBack.SkySegmentWrap(), state, preferLocal);
        }
    }
}
