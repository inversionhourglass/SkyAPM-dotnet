/*
 * Licensed to the SkyAPM under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The SkyAPM licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using SkyApm.Transport;

namespace SkyApm.Diagnostics.MSLogging
{
    public class SkyApmLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ISkyApmLogDispatcher _skyApmLogDispatcher;
        private readonly ITracingContext _tracingContext;
        private readonly TracingConfig _tracingConfig;
        private readonly DiagnosticsLoggingConfig _logCollectorConfig;

        public SkyApmLogger(string categoryName, ISkyApmLogDispatcher skyApmLogDispatcher,
            ITracingContext tracingContext,
            IConfigAccessor configAccessor)
        {
            _categoryName = categoryName;
            _skyApmLogDispatcher = skyApmLogDispatcher;
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
            _logCollectorConfig = configAccessor.Get<DiagnosticsLoggingConfig>();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var tags = new Dictionary<string, object>
            {
                { "logger", _categoryName },
                { "level", logLevel },
                { "thread", Thread.CurrentThread.ManagedThreadId },
            };
            if (exception != null)
            {
                tags["errorType"] = exception.GetType().ToString();
            }
            var message = state.ToString();
            if (exception != null)
            {
                message += "\r\n" + (exception.HasInnerExceptions() ? exception.ToDemystifiedString(_tracingConfig.ExceptionMaxDepth) : exception.ToString());
            }
            var logContext = new LoggerRequest()
            {
                Message = message ?? string.Empty,
                Tags = tags,
                SegmentReference = GetReference(),
            };
            if (_tracingContext.First != null && _tracingContext.First.Span.SpanType == SpanType.Entry)
            {
                logContext.Endpoint = _tracingContext.First.Span.OperationName.ToString();
            }
            _skyApmLogDispatcher.Dispatch(logContext);
        }

        public bool IsEnabled(LogLevel logLevel) => (int)logLevel >= (int)_logCollectorConfig.CollectLevel;


        public IDisposable BeginScope<TState>(TState state) => default!;

        private LoggerSegmentReference GetReference()
        {
            return _tracingContext.TraceId == null || _tracingContext.SegmentId == null ?
                null :
                new LoggerSegmentReference
                {
                    TraceId = _tracingContext.TraceId,
                    SegmentId = _tracingContext.SegmentId,
                    SpanId = _tracingContext.Current.Span.SpanId,
                };
        }
    }
}