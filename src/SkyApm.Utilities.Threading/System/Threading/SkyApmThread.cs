using System.Globalization;

namespace System.Threading
{
    public class SkyApmThread
    {
        private readonly Thread _thread;

        public SkyApmThread(ParameterizedThreadStart start)
        {
            _thread = new Thread(start.SkySegmentWrap());
        }

        public SkyApmThread(ParameterizedThreadStart start, int maxStackSize)
        {
            _thread = new Thread(start.SkySegmentWrap(), maxStackSize);
        }

        public SkyApmThread(ThreadStart start)
        {
            _thread = new Thread(start.SkySegmentWrap());
        }

        public SkyApmThread(ThreadStart start, int maxStackSize)
        {
            _thread = new Thread(start.SkySegmentWrap(), maxStackSize);
        }

        /// <inheritdoc cref="Thread.ApartmentState"/>
        [Obsolete("The ApartmentState property has been deprecated.  Use GetApartmentState, SetApartmentState or TrySetApartmentState instead.", false)]
        public ApartmentState ApartmentState
        {
            get => _thread.ApartmentState;
            set => _thread.ApartmentState = value;
        }

        /// <inheritdoc cref="Thread.CurrentCulture"/>
        public CultureInfo CurrentCulture
        {
            get => _thread.CurrentCulture;
            set => _thread.CurrentCulture = value;
        }

        /// <inheritdoc cref="Thread.CurrentUICulture"/>
        public CultureInfo CurrentUICulture
        {
            get => _thread.CurrentCulture;
            set => _thread.CurrentCulture = value;
        }

        /// <inheritdoc cref="Thread.ExecutionContext"/>
        public ExecutionContext ExecutionContext => _thread.ExecutionContext;

        /// <inheritdoc cref="Thread.IsAlive"/>
        public bool IsAlive => _thread.IsAlive;

        /// <inheritdoc cref="Thread.IsBackground"/>
        public bool IsBackground
        {
            get => _thread.IsBackground;
            set => _thread.IsBackground = value;
        }

        /// <inheritdoc cref="Thread.IsThreadPoolThread"/>
        public bool IsThreadPoolThread => _thread.IsThreadPoolThread;

        /// <inheritdoc cref="Thread.ManagedThreadId"/>
        public int ManagedThreadId => _thread.ManagedThreadId;

        /// <inheritdoc cref="Thread.Name"/>
        public string Name
        {
            get => _thread.Name;
            set => _thread.Name = value;
        }

        /// <inheritdoc cref="Thread.Priority"/>
        public ThreadPriority Priority
        {
            get => _thread.Priority;
            set => _thread.Priority = value;
        }

        /// <inheritdoc cref="Thread.ThreadState"/>
        public ThreadState ThreadState => _thread.ThreadState;

        /// <inheritdoc cref="Thread.Abort()"/>
        public void Abort()
        {
            _thread.Abort();
        }

        /// <inheritdoc cref="Thread.Abort(object?)"/>
        public void Abort(object stateInfo)
        {
            _thread.Abort(stateInfo);
        }

        /// <inheritdoc cref="Thread.DisableComObjectEagerCleanup()"/>
        public void DisableComObjectEagerCleanup()
        {
            _thread.DisableComObjectEagerCleanup();
        }

        /// <inheritdoc cref="Thread.GetApartmentState()"/>
        public ApartmentState GetApartmentState()
        {
            return _thread.GetApartmentState();
        }

        /// <inheritdoc cref="Thread.GetCompressedStack()"/>
        [Obsolete("Thread.GetCompressedStack is no longer supported. Please use the System.Threading.CompressedStack class")]
        public CompressedStack GetCompressedStack()
        {
            return _thread.GetCompressedStack();
        }

        /// <inheritdoc cref="Thread.GetHashCode()"/>
        public override int GetHashCode()
        {
            return _thread.GetHashCode();
        }

        /// <inheritdoc cref="Thread.Interrupt()"/>
        public void Interrupt()
        {
            _thread.Interrupt();
        }

        /// <inheritdoc cref="Thread.Join()"/>
        public void Join()
        {
            _thread.Join();
        }

        /// <inheritdoc cref="Thread.Join(int)"/>
        public bool Join(int millisecondsTimeout)
        {
            return _thread.Join(millisecondsTimeout);
        }

        /// <inheritdoc cref="Thread.Join(TimeSpan)"/>
        public bool Join(TimeSpan timeout)
        {
            return _thread.Join(timeout);
        }

        /// <inheritdoc cref="Thread.Resume()"/>
        [Obsolete("Thread.Resume has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  https://go.microsoft.com/fwlink/?linkid=14202", false)]
        public void Resume()
        {
            _thread.Resume();
        }

        /// <inheritdoc cref="Thread.SetApartmentState(ApartmentState)"/>
        public void SetApartmentState(ApartmentState state)
        {
            _thread.SetApartmentState(state);
        }

        /// <inheritdoc cref="Thread.SetCompressedStack(CompressedStack)"/>
        [Obsolete("Thread.SetCompressedStack is no longer supported. Please use the System.Threading.CompressedStack class")]
        public void SetCompressedStack(CompressedStack stack)
        {
            _thread.SetCompressedStack(stack);
        }

        /// <inheritdoc cref="Thread.Start()"/>
        public void Start()
        {
            _thread.Start();
        }

        /// <inheritdoc cref="Thread.Start(object?)"/>
        public void Start(object parameter)
        {
            _thread.Start(parameter);
        }

        /// <inheritdoc cref="Thread.Suspend()"/>
        [Obsolete("Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  https://go.microsoft.com/fwlink/?linkid=14202", false)]
        public void Suspend()
        {
            _thread.Suspend();
        }

        /// <inheritdoc cref="Thread.TrySetApartmentState(ApartmentState)"/>
        public bool TrySetApartmentState(ApartmentState state)
        {
            return _thread.TrySetApartmentState(state);
        }
    }
}
