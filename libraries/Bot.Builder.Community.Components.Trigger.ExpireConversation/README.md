# Expire Conversation Trigger Component for Bot Framework Composer

## Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [Available via NuGet](https://www.nuget.org/packages/Bot.Builder.Community.Components.Trigger.ConversationExpire/) |

## Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains open source Bot Framework Composer components, along with other extensions, including middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

Expire Conversation : If a user does not respond after a certain period of time 'Expire Conversation' activity gets trigger to restart a conversation from the beginning.

> [NOTE] : Expire conversation it won't trigger automatically , user has to connect the bot.



* [Composer component installation](#composer-component-installation)
* [Add trigger](#Add-trigger)
* [Result from Expire Conversation](#result-from-expire-conversation)


### Composer component installation

1. Navigate to the Bot Framework Composer **Package Manager**.
2. Search for 'Conversation Expired' and install **Bot.Builder.Community.Components.Trigger.ExpireConversation**

![packageconversation](https://user-images.githubusercontent.com/16264167/147586529-ec1b2dcc-6e08-44ed-ad62-f4f6e12a5f28.png)



### Add trigger

1. Go to the create a trigger dialog and add the 'Expire conversation'.


![expiretrigger](https://user-images.githubusercontent.com/16264167/147586578-887f6c80-3e9f-44c2-9a8f-f9acaed4eae3.png)


2. Set the Expire in seconds in the right side

![expireconversation](https://user-images.githubusercontent.com/16264167/147586589-867ea8ad-5758-4c7a-88e3-bde67b72ec87.png)



### Result from Expire Conversation
Once you've configured the 'Expire conversation' and set the 'Expire in seconds' then this component will run on every activity received and track the timing.

You can get the results use this syntax
`${turn.expire}` 


![conversationresult](https://user-images.githubusercontent.com/16264167/147586605-0b169773-2613-4cc0-9ddb-78abdfd64b99.png)
