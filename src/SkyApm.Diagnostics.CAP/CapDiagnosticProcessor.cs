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
using System.Collections.Concurrent;
using DotNetCore.CAP.Diagnostics;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Transport;
using Newtonsoft.Json;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using CapEvents = DotNetCore.CAP.Diagnostics.CapDiagnosticListenerNames;

namespace SkyApm.Diagnostics.CAP
{
    /// <summary>
    ///  Diagnostics processor for listen and process events of CAP.
    /// </summary>
    public class CapTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        private readonly ConcurrentDictionary<string, CrossThreadCarrier> _carriers = new ConcurrentDictionary<string, CrossThreadCarrier>();
        public string ListenerName => CapEvents.DiagnosticListenerName;

        private const string OperateNamePrefix = "CAP/";
        private const string ProducerOperateNameSuffix = "/Publisher";
        private const string ConsumerOperateNameSuffix = "/Subscriber";

        private readonly ITracingContext _tracingContext;
        private readonly TracingConfig _tracingConfig;

        public CapTracingDiagnosticProcessor(ITracingContext tracingContext,
            IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        [DiagnosticName(CapEvents.BeforePublishMessageStore)]
        public void BeforePublishStore([Object] CapEventDataPubStore eventData)
        {
            var context = _tracingContext.CreateLocal("Event Persistence: " + eventData.Operation);
            context.Span.SpanLayer = SpanLayer.DB;
            context.Span.Component = Components.CAP;
            context.Span.AddTag(Tags.DB_TYPE, "Sql");
            context.Span.AddLog(LogEvent.Event("Event Persistence Start"));
            context.Span.AddLog(LogEvent.Message("CAP message persistence start..."));

            _carriers[eventData.Message.GetId()] = context.GetCrossThreadCarrier();
        }

        [DiagnosticName(CapEvents.AfterPublishMessageStore)]
        public void AfterPublishStore([Object] CapEventDataPubStore eventData)
        {
            var context = _tracingContext.CurrentLocal;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Event Persistence End"));
            context.Span.AddLog(LogEvent.Message($"CAP message persistence succeeded!{Environment.NewLine}" +
                                                 $"--> Spend Time: { eventData.ElapsedTimeMs }ms.{Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.Message.GetId() } , Name: { eventData.Operation} "));

            _tracingContext.Finish(context);
        }

        [DiagnosticName(CapEvents.ErrorPublishMessageStore)]
        public void ErrorPublishStore([Object] CapEventDataPubStore eventData)
        {
            var context = _tracingContext.CurrentLocal;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Event Persistence Error"));
            context.Span.AddLog(LogEvent.Message($"CAP message persistence failed!{Environment.NewLine}" +
                                                 $"--> Message Info:{Environment.NewLine}" +
                                                 $"{ JsonConvert.SerializeObject(eventData.Message, Formatting.Indented)}"));

            context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
            _tracingContext.Finish(context);
        }

        [DiagnosticName(CapEvents.BeforePublish)]
        public void BeforePublish([Object] CapEventDataPubSend eventData)
        {
            var carrier = _carriers[eventData.TransportMessage.GetId()];

            var host = eventData.BrokerAddress.Endpoint.Replace("-1", "5672");
            var context = _tracingContext.CreateExit(OperateNamePrefix + eventData.Operation + ProducerOperateNameSuffix,
                host, carrier, new CapCarrierHeaderCollection(eventData.TransportMessage));

            context.Span.SpanLayer = SpanLayer.MQ;
            context.Span.Component = GetComponent(eventData.BrokerAddress, true);
            context.Span.Peer = host;
            context.Span.AddTag(Tags.MQ_TOPIC, eventData.Operation);
            context.Span.AddTag(Tags.MQ_BROKER, eventData.BrokerAddress.Endpoint);
            context.Span.AddLog(LogEvent.Event("Event Publishing Start"));
            context.Span.AddLog(LogEvent.Message("CAP message publishing start..."));
        }

        [DiagnosticName(CapEvents.AfterPublish)]
        public void AfterPublish([Object] CapEventDataPubSend eventData)
        {
            var context = _tracingContext.CurrentExit;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Event Publishing End"));
            context.Span.AddLog(LogEvent.Message($"CAP message publishing succeeded!{Environment.NewLine}" +
                                                 $"--> Spend Time: { eventData.ElapsedTimeMs }ms.  {Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.TransportMessage.GetId() }, Name: {eventData.Operation}"));

            _tracingContext.Finish(context);

            _carriers.TryRemove(eventData.TransportMessage.GetId(), out _);
        }

        [DiagnosticName(CapEvents.ErrorPublish)]
        public void ErrorPublish([Object] CapEventDataPubSend eventData)
        {
            var context = _tracingContext.CurrentExit;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Event Publishing Error"));
            context.Span.AddLog(LogEvent.Message($"CAP message publishing failed!{Environment.NewLine}" +
                                                 $"--> Spend Time: { eventData.ElapsedTimeMs }ms.  {Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.TransportMessage.GetId() }, Name: {eventData.Operation}"));
            context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);

            _tracingContext.Finish(context);

            _carriers.TryRemove(eventData.TransportMessage.GetId(), out _);
        }


        [DiagnosticName(CapEvents.BeforeConsume)]
        public void CapBeforeConsume([Object] CapEventDataSubStore eventData)
        {
            var carrierHeader = new CapCarrierHeaderCollection(eventData.TransportMessage);
            var eventName = eventData.TransportMessage.GetGroup() + "/" + eventData.Operation;
            var operationName = OperateNamePrefix + eventName + ConsumerOperateNameSuffix;
            var context = _tracingContext.CreateEntry(operationName, carrierHeader);
            context.Span.SpanLayer = SpanLayer.DB;
            context.Span.Component = GetComponent(eventData.BrokerAddress, false);
            context.Span.Peer = eventData.BrokerAddress.Endpoint.Replace("-1", "5672");
            context.Span.AddTag(Tags.MQ_TOPIC, eventData.Operation);
            context.Span.AddTag(Tags.MQ_BROKER, eventData.BrokerAddress.Endpoint);
            context.Span.AddLog(LogEvent.Event("Event Persistence Start"));
            context.Span.AddLog(LogEvent.Message("CAP message persistence start..."));

            _carriers[eventData.TransportMessage.GetId() + eventData.TransportMessage.GetGroup()] = context.GetCrossThreadCarrier();
        }

        [DiagnosticName(CapEvents.AfterConsume)]
        public void CapAfterConsume([Object] CapEventDataSubStore eventData)
        {
            var context = _tracingContext.CurrentEntry;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Event Persistence End"));
            context.Span.AddLog(LogEvent.Message($"CAP message persistence succeeded!{Environment.NewLine}" +
                                                 $"--> Spend Time: { eventData.ElapsedTimeMs }ms. {Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.TransportMessage.GetId() }, Group: {eventData.TransportMessage.GetGroup()}, Name: {eventData.Operation}"));

            _tracingContext.Finish(context);
        }

        [DiagnosticName(CapEvents.ErrorConsume)]
        public void CapErrorConsume([Object] CapEventDataSubStore eventData)
        {
            var context = _tracingContext.CurrentEntry;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Event Persistence Error"));
            context.Span.AddLog(LogEvent.Message($"CAP message publishing failed! {Environment.NewLine}" +
                                                 $"--> Spend Time: { eventData.ElapsedTimeMs }ms.  {Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.TransportMessage.GetId() }, Group: {eventData.TransportMessage.GetGroup()}, Name: {eventData.Operation}"));
            context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);

            _tracingContext.Finish(context);
        }

        [DiagnosticName(CapEvents.BeforeSubscriberInvoke)]
        public void CapBeforeSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            var carrier = _carriers[eventData.Message.GetId() + eventData.Message.GetGroup()];

            var context = _tracingContext.CreateLocal("Subscriber Invoke: " + eventData.MethodInfo.Name, carrier);
            context.Span.SpanLayer = SpanLayer.MQ;
            context.Span.Component = Components.CAP;
            context.Span.AddLog(LogEvent.Event("Subscriber Invoke Start"));
            context.Span.AddLog(LogEvent.Message($"Begin invoke the subscriber: {eventData.MethodInfo} {Environment.NewLine}" +
                                                 $"--> Message Id: { eventData.Message.GetId()}, Group: {eventData.Message.GetGroup()}, Name: {eventData.Operation}"));
        }

        [DiagnosticName(CapEvents.AfterSubscriberInvoke)]
        public void CapAfterSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            var context = _tracingContext.CurrentLocal;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Subscriber Invoke End"));
            context.Span.AddLog(LogEvent.Message("Subscriber invoke succeeded!"));
            context.Span.AddLog(LogEvent.Message($"Subscriber invoke spend time: { eventData.ElapsedTimeMs}ms. {Environment.NewLine}" +
                                                 $"--> Method Info: {eventData.MethodInfo}"));

            _tracingContext.Finish(context);

            _carriers.TryRemove(eventData.Message.GetId() + eventData.Message.GetGroup(), out _);
        }

        [DiagnosticName(CapEvents.ErrorSubscriberInvoke)]
        public void CapErrorSubscriberInvoke([Object] CapEventDataSubExecute eventData)
        {
            var context = _tracingContext.CurrentLocal;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Subscriber Invoke Error"));
            context.Span.AddLog(LogEvent.Message($"Subscriber invoke failed! {Environment.NewLine}" +
                                                 $"--> Method Info: { eventData.MethodInfo} {Environment.NewLine}" +
                                                 $"--> Message Info: {Environment.NewLine}" +
                                                 $"{ JsonConvert.SerializeObject(eventData.Message, Formatting.Indented)}"));

            context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);

            _tracingContext.Finish(context);

            _carriers.TryRemove(eventData.Message.GetId() + eventData.Message.GetGroup(), out _);
        }

        private StringOrIntValue GetComponent(BrokerAddress address, bool isPub)
        {
            if (isPub)
            {
                switch (address.Name)
                {
                    case "RabbitMQ":
                        return 52;  // "rabbitmq-producer";
                    case "Kafka":
                        return 40;  //"kafka-producer";
                }
            }
            else
            {
                switch (address.Name)
                {
                    case "RabbitMQ":
                        return 53; // "rabbitmq-consumer";
                    case "Kafka":
                        return 41; // "kafka-consumer";
                }
            }
            return Components.CAP;
        }
    }
}