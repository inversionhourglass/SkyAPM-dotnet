using SkyApm.Diagnostics.Delegates;
using System.Threading.Tasks;

namespace System.Threading
{
    [Obsolete("Use Delegates.Extensions.Diagnostics instead")]
    public class SkyApmTimer : MarshalByRefObject, IAsyncDisposable, IDisposable
    {
        private readonly Timer _timer;

        public SkyApmTimer(TimerCallback callback)
        {
            _timer = new Timer(callback.Diagnostic(SkyApmCategory.FORK));
        }

        public SkyApmTimer(TimerCallback callback, object state, int dueTime, int period)
        {
            _timer = new Timer(callback.Diagnostic(SkyApmCategory.FORK), state, dueTime, period);
        }
        
        public SkyApmTimer(TimerCallback callback, object state, long dueTime, long period)
        {
            _timer = new Timer(callback.Diagnostic(SkyApmCategory.FORK), state, dueTime, period);
        }

        public SkyApmTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            _timer = new Timer(callback.Diagnostic(SkyApmCategory.FORK), state, dueTime, period);
        }

        public SkyApmTimer(TimerCallback callback, object state, uint dueTime, uint period)
        {
            _timer = new Timer(callback.Diagnostic(SkyApmCategory.FORK), state, dueTime, period);
        }

        /// <inheritdoc cref="Timer.Change(int, int)"/>
        public bool Change(int dueTime, int period)
        {
            return _timer.Change(dueTime, period);
        }

        /// <inheritdoc cref="Timer.Change(long, long)"/>
        public bool Change(long dueTime, long period)
        {
            return _timer.Change(dueTime, period);
        }

        /// <inheritdoc cref="Timer.Change(TimeSpan, TimeSpan)"/>
        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            return _timer.Change(dueTime, period);
        }

        /// <inheritdoc cref="Timer.Change(uint, uint)"/>
        public bool Change(uint dueTime, uint period)
        {
            return _timer.Change(dueTime, period);
        }

        /// <inheritdoc cref="Timer.Dispose()"/>
        public void Dispose()
        {
            _timer.Dispose();
        }

        /// <inheritdoc cref="Timer.Dispose(WaitHandle)"/>
        public bool Dispose(WaitHandle notifyObject)
        {
            return _timer.Dispose(notifyObject);
        }

        /// <inheritdoc cref="Timer.DisposeAsync()"/>
        public ValueTask DisposeAsync()
        {
            return _timer.DisposeAsync();
        }
    }
}
