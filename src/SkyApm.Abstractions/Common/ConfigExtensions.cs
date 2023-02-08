using System;

namespace SkyApm.Config
{
    public static class ConfigExtensions
    {
        public static bool Spanable(this InstrumentConfig config)
        {
            return "span".Equals(config.Scale, StringComparison.OrdinalIgnoreCase);
        }
    }
}
