using SkyApm.Config;

namespace SkyApm.Utilities.StaticAccessor
{
    partial class SkyApmInstances
    {
        public static TracingConfig TracingConfig { get; internal set; } = new TracingConfig();
    }
}
