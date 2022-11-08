using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Bot.Builder;
using OpenTelemetry.Trace;

namespace Bot.Builder.Community.OpenTelemetry
{
    public class BotOpenTelemetryClient : IBotTelemetryClient, IBotPageViewTelemetryClient
    {
        private readonly Dictionary<string, Histogram<double>> meters = new Dictionary<string, Histogram<double>>();

        public BotOpenTelemetryClient()
        {
        }

        /// <summary>
        /// Send information about availability of an application.
        /// </summary>
        /// <param name="name">Availability test name.</param>
        /// <param name="timeStamp">The time when the availability was captured.</param>
        /// <param name="duration">The time taken for the availability test to run.</param>
        /// <param name="runLocation">Name of the location the availability test was run from.</param>
        /// <param name="success">True if the availability test ran successfully.</param>
        /// <param name="message">Error message on availability test run failure.</param>
        /// <param name="properties">Named string values you can use to classify and search for this availability telemetry.</param>
        /// <param name="metrics">Additional values associated with this availability telemetry.</param>
        public void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string message = null, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            using (var activity = BotOpenTelemetryHelper.ActivitySource.StartActivity("Availability", ActivityKind.Internal, null, null, null, timeStamp))
            {
                activity.DisplayName = name;

                activity.SetEndTime(timeStamp.Add(duration).DateTime);

                activity.AddTag("availability.runLocation", runLocation);
                activity.AddTag("availability.success", success);
                activity.AddTag("availability.message", message);

                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        activity.SetTag(property.Key, property.Value);
                    }
                }

                if (metrics != null)
                {
                    foreach (var metric in metrics)
                    {
                        var meter = BotOpenTelemetryHelper.Meter.CreateHistogram<double>(metric.Key);
                        meter.Record(metric.Value);
                    }
                }
            }
        }

        /// <summary>   
        /// Send information about an external dependency (outgoing call) in the application.
        /// </summary>
        /// <param name="dependencyTypeName">Name of the command initiated with this dependency call. Low cardinality value.
        /// Examples are SQL, Azure table, and HTTP.</param>
        /// <param name="target">External dependency target.</param>
        /// <param name="dependencyName">Name of the command initiated with this dependency call. Low cardinality value.
        /// Examples are stored procedure name and URL path template.</param>
        /// <param name="data">Command initiated by this dependency call. Examples are SQL statement and HTTP
        /// URL's with all query parameters.</param>
        /// <param name="startTime">The time when the dependency was called.</param>
        /// <param name="duration">The time taken by the external dependency to handle the call.</param>
        /// <param name="resultCode">Result code of dependency call execution.</param>
        /// <param name="success">True if the dependency call was handled successfully.</param>
        public void TrackDependency(string dependencyTypeName, string target, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success)
        {
            using (var activity = BotOpenTelemetryHelper.ActivitySource.StartActivity("Dependency", ActivityKind.Internal, null, null, null, startTime))
            {
                activity.DisplayName = dependencyTypeName;

                activity.SetEndTime(startTime.Add(duration).DateTime);

                activity.AddTag("dependency.target", target);
                activity.AddTag("dependency.dependencyName", dependencyName);
                activity.AddTag("dependency.resultCode", resultCode);
                activity.AddTag("dependency.success", success);
            }
        }

        /// <summary>
        /// Logs custom events with extensible named fields.
        /// </summary>
        /// <param name="eventName">A name for the event.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        /// <param name="metrics">Measurements associated with this event.</param>
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            using (var activity = BotOpenTelemetryHelper.ActivitySource.StartActivity("Event"))
            {
                activity.DisplayName = eventName;

                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        activity.SetTag(property.Key, property.Value);
                    }
                }

                if (metrics != null)
                {
                    foreach (var metric in metrics)
                    {
                        if (!meters.TryGetValue(metric.Key, out Histogram<double> meter))
                        {
                            meter = BotOpenTelemetryHelper.Meter.CreateHistogram<double>(metric.Key);
                            meters.Add(metric.Key, meter);
                        }

                        meter.Record(metric.Value);
                        activity.SetTag($"metrics.{metric.Key}", metric.Value);
                    }
                }

                activity.AddEvent(new ActivityEvent(eventName, DateTimeOffset.Now));
            }
        }

        /// <summary>
        /// Logs a system exception.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="properties">Named string values you can use to classify and search for this exception.</param>
        /// <param name="metrics">Additional values associated with this exception.</param>
        public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            using (var activity = BotOpenTelemetryHelper.ActivitySource.StartActivity("Exception"))
            {
                activity.DisplayName = exception.Message;

                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        activity.SetTag(property.Key, property.Value);
                    }
                }

                if (metrics != null)
                {
                    foreach (var metric in metrics)
                    {
                        if (!meters.TryGetValue(metric.Key, out Histogram<double> meter))
                        {
                            meter = BotOpenTelemetryHelper.Meter.CreateHistogram<double>(metric.Key);
                            meters.Add(metric.Key, meter);
                        }

                        meter.Record(metric.Value);
                        activity.SetTag($"metrics.{metric.Key}", metric.Value);
                    }
                }

                activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
                activity?.RecordException(exception);
            }
        }

        /// <summary>
        /// Logs a dialog entry / as an Application Insights page view.
        /// </summary>
        /// <param name="dialogName">The name of the dialog to log the entry / start for.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        /// <param name="metrics">Measurements associated with this event.</param>
        public void TrackPageView(string dialogName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            using (var activity = BotOpenTelemetryHelper.ActivitySource.StartActivity("PageView"))
            {
                activity.DisplayName = dialogName;

                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        activity.SetTag(property.Key, property.Value);
                    }
                }

                if (metrics != null)
                {
                    foreach (var metric in metrics)
                    {
                        if (!meters.TryGetValue(metric.Key, out Histogram<double> meter))
                        {
                            meter = BotOpenTelemetryHelper.Meter.CreateHistogram<double>(metric.Key);
                            meters.Add(metric.Key, meter);
                        }

                        meter.Record(metric.Value);
                        activity.SetTag($"metrics.{metric.Key}", metric.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Send a trace message.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="severityLevel">Trace severity level <see cref="Severity"/>.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        public void TrackTrace(string message, Severity severityLevel, IDictionary<string, string> properties)
        {
            using (var activity = BotOpenTelemetryHelper.ActivitySource.StartActivity("Trace"))
            {
                activity.DisplayName = message;
                activity.AddTag("trace.message", message);
                activity.AddTag("trace.severityLevel", severityLevel.ToString());

                var eventTags = new ActivityTagsCollection();

                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        eventTags.Add(property.Key, property.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Flushes the in-memory buffer and any metrics being pre-aggregated.
        /// </summary>
        public void Flush()
        {
        }
    }

}