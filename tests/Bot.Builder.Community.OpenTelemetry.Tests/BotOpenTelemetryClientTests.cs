using System.Diagnostics;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics.Metrics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot.Builder.Community.OpenTelemetry.Tests
{
    [TestClass]
    public class BotOpenTelemetryClientTests
    {
        [TestMethod]
        public void ShouldTrackEvent()
        {
            var activities = new List<System.Diagnostics.Activity>();

            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                   .AddInMemoryExporter(activities)
                   .AddBotBuilderInstrumentation()
                   .Build();

            var telemetryClient = new BotOpenTelemetryClient();

            telemetryClient.TrackEvent("Event1");

            tracerProvider?.ForceFlush();

            Assert.AreEqual(1, activities.Count);
        }

        [TestMethod]
        public void ShouldTrackEventWithMetrics()
        {
            var activities = new List<System.Diagnostics.Activity>();
            var metrics = new List<Metric>();

            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                   .AddInMemoryExporter(activities)
                   .AddBotBuilderInstrumentation()
                   .Build();

            var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddInMemoryExporter(metrics)
                .AddBotBuilderInstrumentation()
                .Build();

            var telemetryClient = new BotOpenTelemetryClient();

            var eventMetrics = new Dictionary<string, double>() {
                {"event.duration", 100}
            };

            telemetryClient.TrackEvent("Event1", metrics: eventMetrics);

            tracerProvider?.ForceFlush();
            meterProvider.ForceFlush();

            Assert.AreEqual(1, activities.Count);
            Assert.AreEqual(1, metrics.Count);
        }

        [TestMethod]
        public async Task ShouldTrackDialogEventAsync()
        {
            var activities = new List<System.Diagnostics.Activity>();

            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                   .AddInMemoryExporter(activities)
                   .AddBotBuilderInstrumentation()
                   .Build();

            var conversationState = new ConversationState(new MemoryStorage());
            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                 .Use(new AutoSaveStateMiddleware(conversationState));

             // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);
            
            dialogs.Add(new WaterfallDialog("test", NewWaterfall()));
            dialogs.TelemetryClient = new BotOpenTelemetryClient();

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync("test", null, cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("step1")
            .Send("hello")
            .AssertReply("step2")
            .Send("hello")
            .AssertReply("step3")
            .StartTestAsync();
            
            tracerProvider?.ForceFlush();
            
            Assert.AreEqual(5, activities.Count);
        }

         private static WaterfallStep[] NewWaterfall()
        {
            return new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    await step.Context.SendActivityAsync("step1");
                    return Dialog.EndOfTurn;
                },
                async (step, cancellationToken) =>
                {
                    await step.Context.SendActivityAsync("step2");
                    return Dialog.EndOfTurn;
                },
                async (step, cancellationToken) =>
                {
                    await step.Context.SendActivityAsync("step3");
                    return Dialog.EndOfTurn;
                },
            };
        }
    }


}