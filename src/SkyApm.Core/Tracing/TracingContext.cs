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
using SkyApm.Common;
using SkyApm.Logging;
using SkyApm.Tracing.Segments;
using SkyApm.Transport;

namespace SkyApm.Tracing
{
    public class TracingContext : ITracingContext
    {
        private readonly ISegmentContextFactory _segmentContextFactory;
        private readonly ICarrierPropagator _carrierPropagator;
        private readonly ISegmentDispatcher _segmentDispatcher;
        private readonly ILogger _logger;

        public TracingContext(ISegmentContextFactory segmentContextFactory,
            ICarrierPropagator carrierPropagator,
            ISegmentDispatcher segmentDispatcher,
            ILoggerFactory loggerFactory)
        {
            _segmentContextFactory = segmentContextFactory;
            _carrierPropagator = carrierPropagator;
            _segmentDispatcher = segmentDispatcher;
            _logger = loggerFactory.CreateLogger(typeof(TracingContext));
        }

        public SpanOrSegmentContext CurrentEntry => _segmentContextFactory.CurrentEntryContext;

        public SpanOrSegmentContext CurrentLocal => _segmentContextFactory.CurrentLocalContext;

        public SpanOrSegmentContext CurrentExit => _segmentContextFactory.CurrentExitContext;

        public SpanOrSegmentContext CreateEntry(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = default)
        {
            return CreateEntrySegmentContext(operationName, carrierHeader, startTimeMilliseconds);
        }

        public SpanOrSegmentContext CreateLocal(string operationName, long startTimeMilliseconds = default)
        {
            return CreateLocalSegmentContext(operationName, startTimeMilliseconds);
        }

        public SpanOrSegmentContext CreateLocal(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = default)
        {
            return CreateLocalSegmentContext(operationName, carrier, startTimeMilliseconds);
        }

        public SpanOrSegmentContext CreateExit(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default)
        {
            return CreateExitSegmentContext(operationName, networkAddress, carrierHeader, startTimeMilliseconds);
        }

        public SpanOrSegmentContext CreateExit(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default)
        {
            return CreateExitSegmentContext(operationName, networkAddress, carrier, carrierHeader, startTimeMilliseconds);
        }

        public void Finish(SpanOrSegmentContext spanOrSegmentContext)
        {
            if (CheckStructure(spanOrSegmentContext))
            {
                Release(spanOrSegmentContext.SegmentContext);
            }
        }

        private bool CheckStructure(SpanOrSegmentContext spanOrSegmentContext)
        {
            if (spanOrSegmentContext.SegmentContext != null) return true;

            if (spanOrSegmentContext.SegmentSpan != null)
            {
                _logger.Warning($"try to finish SegmentSpan in Segment structure mode");
                _logger.Debug($"try to finish SegmentSpan in Segment structure mode, operationName: {spanOrSegmentContext.SegmentSpan.OperationName}, stackTracing: {Environment.StackTrace}");
            }

            return false;
        }

        #region SegmentContext
        private SegmentContext CreateEntrySegmentContext(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = default)
        {
            if (operationName == null) throw new ArgumentNullException(nameof(operationName));
            var carrier = _carrierPropagator.Extract(carrierHeader);
            return _segmentContextFactory.CreateEntrySegment(operationName, carrier, startTimeMilliseconds);
        }

        private SegmentContext CreateLocalSegmentContext(string operationName, long startTimeMilliseconds = default)
        {
            if (operationName == null) throw new ArgumentNullException(nameof(operationName));
            return _segmentContextFactory.CreateLocalSegment(operationName, startTimeMilliseconds);
        }

        private SegmentContext CreateLocalSegmentContext(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = default)
        {
            if (operationName == null) throw new ArgumentNullException(nameof(operationName));
            return _segmentContextFactory.CreateLocalSegment(operationName, carrier, startTimeMilliseconds);
        }

        private SegmentContext CreateExitSegmentContext(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default)
        {
            var segmentContext = _segmentContextFactory.CreateExitSegment(operationName, new StringOrIntValue(networkAddress), startTimeMilliseconds);
            if (carrierHeader != null)
                _carrierPropagator.Inject(segmentContext, carrierHeader);
            return segmentContext;
        }

        private SegmentContext CreateExitSegmentContext(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = default, long startTimeMilliseconds = default)
        {
            var segmentContext = _segmentContextFactory.CreateExitSegment(operationName, new StringOrIntValue(networkAddress), carrier, startTimeMilliseconds);
            if (carrierHeader != null)
                _carrierPropagator.Inject(segmentContext, carrierHeader);
            return segmentContext;
        }

        private void Release(SegmentContext segmentContext, long endTimeMilliseconds = default)
        {
            if (segmentContext == null)
            {
                return;
            }

            _segmentContextFactory.Release(segmentContext, endTimeMilliseconds);
            if (segmentContext.Sampled)
                _segmentDispatcher.Dispatch(segmentContext);
        }
        #endregion SegmentContext
    }
}