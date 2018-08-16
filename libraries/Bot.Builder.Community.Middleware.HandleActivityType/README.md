## Handle Activity Type Middleware

This is part of the [Bot Builder v4 Extensions](https://github.com/garypretty/botbuilder-dotnet-extensions/) project which contains various pieces pf middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This piece of middleware will allow you you to handle incoming activities of specific types, such as 'conversationUpdate' or 'contactRelationUpdate'.

Available via [NuGet](https://www.nuget.org/packages/GaryPretty.Bot.Builder.Middleware.HandleActivityType/).

Install into your project using the following command in the package manager;
```
    PM> Install-Package GaryPretty.Bot.Builder.Middleware.HandleActivityType
```

To use the middleware, add it to the pipeline:

```cs
middleware.Add(new HandleActivityTypeMiddleware(ActivityTypes.ConversationUpdate, async (context, next) =>
                    {
                        // here you can do whatever you want to respond to the activity
                        await context.SendActivity("Hi! Welcome. I am the bot :)");

                        // If you want to continue routing through the pipeline to additional
                        // middleware and to the bot itself then call the following line.
                        await next();
                    }));
```

You can also use the middleware to simply filter out activity types you do not wish your bot to handle at all

```cs
middleware.Add(new HandleActivityTypeMiddleware(ActivityTypes.ConversationUpdate, async (context, next) => { }));
```