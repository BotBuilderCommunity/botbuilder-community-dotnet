using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Diagnostics.Metrics;

namespace Bot.Builder.Community.OpenTelemetry
{

    internal class BotOpenTelemetryHelper
    {
        public static readonly AssemblyName AssemblyName = typeof(BotOpenTelemetryHelper).Assembly.GetName();
        public static readonly string InstrumentationName = AssemblyName.Name;
        public static readonly string ActivityName = InstrumentationName + ".Execute";
        private static readonly Version Version = typeof(BotOpenTelemetryHelper).Assembly.GetName().Version;
        internal static readonly ActivitySource ActivitySource = new ActivitySource(InstrumentationName, Version.ToString());
        internal static readonly Meter Meter = new Meter(InstrumentationName, Version.ToString());
    }
}