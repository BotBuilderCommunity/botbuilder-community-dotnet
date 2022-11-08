# Bot Builder Instrumentation for OpenTelemetry

## Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains Bot Framework Components and other projects / packages for use with Bot Framework Composer and the Bot Builder .NET SDK v4.

This is an [Instrumentation Library](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/glossary.md#instrumentation-library), which instruments Bot Builder and collects telemetry.
OpenTelemetry is a collection of tools, APIs, and SDKs. Use it to instrument, generate, collect, and export telemetry data (metrics, logs, and traces) to help you analyze your software’s performance and behavior. You can get more info at https://opentelemetry.io

## Usage

- [Installation](#Installation)
- [Enable Bot Builder Instrumentation at application startup](#Enable-Bot-Builder-Instrumentation-at-application-startup)

## Installation

You need to install the `Bot.Builder.Community.OpenTelemetry` to be able to use the OpenTelemetry Bot Instrumentation.

Available via NuGet package [Bot.Builder.Community.OpenTelemetry](https://www.nuget.org/packages/Bot.Builder.Community.OpenTelemetry/) 

```shell
dotnet add Bot.Builder.Community.OpenTelemetry
```

## Enable Bot Builder Instrumentation at application startup

Bot Builder instrumentation must be enabled at application startup.

The following example demonstrates adding Bot Builder instrumentation to a
console application. This example also sets up the OpenTelemetry Console
exporter, which requires adding the package
[`OpenTelemetry.Exporter.Console`](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Console/README.md)
to the application.

```csharp
using OpenTelemetry.Trace;
public class Program
{
    public static void Main(string[] args)
    {
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddBotBuilderInstrumentation()
            .AddConsoleExporter()
            .Build();
    }
}
```

For an ASP.NET Core application, adding instrumentation is typically done in
the `ConfigureServices` of your `Startup` class. Refer to [example](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/AspNetCore/Program.cs).

For an ASP.NET application, adding instrumentation is typically done in the
`Global.asax.cs`. Refer to [example](../../examples/AspNet/Global.asax.cs).
In order to sucessfully configure Zoom to send requests to your bot, you are required to provide it with you bot's Zoom endpoint. To do this deploy your bot to Azure and make a note of the URL to your deployed bot. Your Zoom messaging endpoint is the URL for your bot, which will be the URL of your deployed application (or ngrok endpoint), plus '/api/zoom' (for example, `https://yourbotapp.azurewebsites.net/api/zoom`).
