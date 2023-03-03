using Microsoft.Extensions.DependencyInjection;
using SkyApm.Utilities.DependencyInjection;
using System;

namespace SkyApm.Diagnostics.Delegates
{
    public static class SkyWalkingBuilderExtensions
    {
        public static SkyApmExtensions AddDelegates(this SkyApmExtensions extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            extensions.Services.AddSingleton<ITracingDiagnosticProcessor, DelegateDiagnosticProcessor>();

            return extensions;
        }
    }
}
