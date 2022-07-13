# Session Expire / Reminder Conversation - Trigger Component for Bot Framework Composer

## Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [Available via NuGet](https://www.nuget.org/packages/Bot.Builder.Community.Components.Trigger.ConversationExpire/) |

## Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains open source Bot Framework Composer components, along with other extensions, including middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

Session Expire Conversation : If a user does not respond after a certain period of time 'Session Expire Conversation' activity gets trigger to restart a conversation.

Reminder Conversation : If a user does not respond after a certain period of time 'Reminder Conversation' activity gets trigger to reminder a conversation


* [Composer component installation](#composer-component-installation)
* [ Add Reminder trigger](# Add Reminder trigger)
* [Result from Add Reminder trigger](#result-from-expire-conversation)
* [ Add Session trigger](# Add Session trigger)
* [Result from Add Reminder trigger](#result-from-Session-expire-conversation)
* [Option Settings](#Option-Settings)
* [Note](#Note)


### Composer component installation

1. Navigate to the Bot Framework Composer **Package Manager**.
2. Search for 'Agent Session' and install **Bot.Builder.Community.Components.Trigger.AgentSession**

![packageconversation](https://user-images.githubusercontent.com/16264167/147586529-ec1b2dcc-6e08-44ed-ad62-f4f6e12a5f28.png)


### Add Reminder trigger

1. Go to the create a trigger dialog and add the 'Reminder conversation'.


![expiretrigger](https://user-images.githubusercontent.com/16264167/147586578-887f6c80-3e9f-44c2-9a8f-f9acaed4eae3.png)


2. Set the Reminder in seconds in the right side 

![expireconversation](https://user-images.githubusercontent.com/16264167/147586589-867ea8ad-5758-4c7a-88e3-bde67b72ec87.png)

### Result from Reminder trigger
Once you've configured the 'Reminder trigger' and set the 'Expire in seconds' then this component will run on and start monitor the activitiy received or not incase activities is not received Session 'reminder trigger' gets invoked

You can get the results use this syntax '${turn.activity.entities}'
entities contains user details
 



### Add Session trigger

1. Go to the create a trigger dialog and add the 'Session expire conversation'.

2. Set the Expire in seconds in the right side


### Result from Session Expire Conversation
Once you've configured the 'Reminder trigger' and set the 'Expire in seconds' then this component will run on and start monitor the activitiy received or not incase activities is not received 'Session Expire Conversation' gets invoked

You can get the results use this syntax '${turn.activity.entities}'
entities contains user details


### Option Settings
Below settings are options  

This settings option is use to Control Agent Service , default Agent service scan the conversation every 1000 Milliseconds , you can change this scan option in the settings page.
IsEnable : This option is use to "Enable or Disable" Agent Service triggers 

goto navigate to Project Settings and toggle the Advanced Settings View (json) add the following settings at the root of your settings JSON.


### Note

1. Both the trigger's work independently , incase adding two trigger's together,Reminder trigger 'seconds' must be always less then Session expire in seconds.
2. After receive the 'Session Expire Conversation trigger' developer should be handle end of conversation , clear memory , Database etc..