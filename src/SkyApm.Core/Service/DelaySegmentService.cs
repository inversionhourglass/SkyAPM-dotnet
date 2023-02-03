using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Transport;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Service
{
    public class DelaySegmentService : ExecutionService
    {
        private readonly SpanStructureConfig _config;
        private readonly ISegmentDispatcher _dispatcher;

        public DelaySegmentService(IConfigAccessor configAccessor, ISegmentDispatcher dispatcher,
            IRuntimeEnvironment runtimeEnvironment, ILoggerFactory loggerFactory)
            : base(runtimeEnvironment, loggerFactory)
        {
            _dispatcher = dispatcher;
            _config = configAccessor.Get<SpanStructureConfig>();
            Period = TimeSpan.FromMilliseconds(_config.DelayInspectInterval);
        }

        protected override TimeSpan DueTime { get; } = TimeSpan.FromSeconds(3);

        protected override TimeSpan Period { get; }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _dispatcher.DelayInspect(cancellationToken);

            return Task.CompletedTask;
        }

        protected override Task Stopping(CancellationToken cancellationToke)
        {
            return Task.CompletedTask;
        }
    }
}
