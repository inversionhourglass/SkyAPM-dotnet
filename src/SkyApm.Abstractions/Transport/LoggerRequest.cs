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

using System.Collections.Generic;

namespace SkyApm.Transport
{
    public class LoggerRequest
    {
        public string Message { get; set; }

        public Dictionary<string, object> Tags { get; set; }

        public LoggerSegmentReference SegmentReference { get; set; }

        public long Date { get; set; }

        public string Endpoint { get; set; }
    }
    
    public class LoggerSegmentReference
    {
        public string SegmentId { get; set; }

        public string TraceId { get; set; }

        public int SpanId { get; set; }
    }
}
