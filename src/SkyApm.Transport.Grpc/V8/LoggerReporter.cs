﻿/*
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

using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Transport.Grpc.Common;
using SkyWalking.NetworkProtocol.V3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Transport.Grpc
{
    public class LoggerReporter : ILoggerReporter
    {
        private readonly ConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly GrpcConfig _grpcConfig;
        private readonly InstrumentConfig _instrumentConfig;

        public LoggerReporter(ConnectionManager connectionManager, IConfigAccessor configAccessor,
            ILoggerFactory loggerFactory)
        {
            _connectionManager = connectionManager;
            _grpcConfig = configAccessor.Get<GrpcConfig>();
            _instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _logger = loggerFactory.CreateLogger(typeof(SegmentReporter));
        }
        
        public async Task ReportAsync(IReadOnlyCollection<LoggerRequest> loggerRequests,
            CancellationToken cancellationToken = default)
        {
            if (!_connectionManager.Ready)
            {
                return;
            }

            var connection = _connectionManager.GetConnection();
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var client = new LogReportService.LogReportServiceClient(connection);
                using (var asyncClientStreamingCall = client.collect(_grpcConfig.GetMeta(),
                           _grpcConfig.GetReportTimeout(), cancellationToken))
                {
                    foreach (var loggerRequest in loggerRequests)
                    {
                        var logBody = new LogData()
                        {
                            Timestamp = loggerRequest.Date,
                            Service = _instrumentConfig.ServiceName,
                            ServiceInstance = _instrumentConfig.ServiceInstanceName,
                            Endpoint = loggerRequest.Endpoint,
                            Body = new LogDataBody()
                            {
                                Type = "text",
                                Text = new TextLog()
                                {
                                    Text = loggerRequest.Message,
                                },
                            },
                            Tags = new LogTags(),
                        };
                        if (loggerRequest.SegmentReference != null)
                        {
                            logBody.TraceContext = new TraceContext()
                            {
                                TraceId = loggerRequest.SegmentReference.TraceId,
                                TraceSegmentId = loggerRequest.SegmentReference.SegmentId,
                                SpanId = loggerRequest.SegmentReference.SpanId,
                            };
                        }
                        foreach (var tag in loggerRequest.Tags)
                        {
                            logBody.Tags.Data.Add(new KeyStringValuePair()
                            {
                                Key = tag.Key,
                                Value = tag.Value.ToString(),
                            });
                        }
                        await asyncClientStreamingCall.RequestStream.WriteAsync(logBody);
                    }

                    await asyncClientStreamingCall.RequestStream.CompleteAsync();
                    await asyncClientStreamingCall.ResponseAsync;

                    stopwatch.Stop();
                    _logger.Information($"Report {loggerRequests.Count} logs. cost: {stopwatch.Elapsed}s");
                }
            }
            catch (IOException ex)
            {
                _logger.Error("Report trace segment fail.", ex);
                _connectionManager.Failure(ex);
            }
            catch (Exception ex)
            {
                _logger.Error("Report trace segment fail.", ex);
            }
        }
    }
}