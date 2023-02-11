namespace SkyApm.Config
{
    public static class ConfigExtensions
    {
        public static bool Spanable(this InstrumentConfig config)
        {
            return config.Scale == TraceScale.Span;
        }
    }
}
