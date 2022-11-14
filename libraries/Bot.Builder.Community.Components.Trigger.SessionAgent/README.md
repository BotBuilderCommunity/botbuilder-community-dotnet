# Session Expire / Reminder Conversation - Trigger Component for Bot Framework Composer

## Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [Available via NuGet](https://www.nuget.org/packages/Bot.Builder.Community.Components.Trigger.SessionAgent/) |

## Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains open source Bot Framework Composer components, along with other extensions, including middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

**Session Expire Conversation** : If a user does not respond after a certain period 'Session Expire Conversation' activity gets triggered to restart a conversation.

**Reminder Conversation** : If a user does not respond after a certain period 'Reminder Conversation' activity gets triggered to remind a conversation


* [Composer component installation](#composer-component-installation)
* [Add Reminder trigger](#Add-Reminder-trigger)
* [Result from Add Reminder trigger](#result-from-expire-conversation)
* [Add Session expire trigger](#Add-Session-Expire-trigger)
* [Result from Add Session expire trigger](#result-from-Session-expire-conversation)
* [Settings](#Settings)
* [Note](#Note)


### Composer component installation

1. Navigate to the Bot Framework Composer **Package Manager**.
2. Search for 'Session' and install **Bot.Builder.Community.Components.Trigger.SessionAgent**

![image](https://user-images.githubusercontent.com/16264167/178138577-0d81798a-2b55-4efa-a394-60221858a42a.png)


### Add Reminder trigger
1. Go to the create a trigger dialog and add the 'Reminder conversation'.
![image](https://user-images.githubusercontent.com/16264167/178139209-3a62181d-935c-4d16-9688-882d9c8e9e78.png)



2. Set the Reminder in seconds in the right side 
![image](https://user-images.githubusercontent.com/16264167/178138798-25aedf1f-434a-4b0a-ae91-84ec815b723e.png)


### Result from Reminder trigger
Once you've configured the 'Reminder trigger' and set the 'Reminder in seconds' then this component will run on and start to monitor the activity, In case activities are not received within the time limit 'reminder trigger' gets invoked.

You can get the results use this syntax '${turn.activity.entities}'  (entities contains user details)

```json
{

“lgType”: “Activity”,

“text”: [

{

  "type": "ReminderConversation",

  "mentioned": {

    "id": "e765a676-bb70-412c-a351-3587745e52cb",

    "name": "User",

    "aadObjectId": null,

    "role": "user"

  },

  "text": null

}
]
}
```

### Add Session Expire trigger

1. Go to the create a trigger dialog and add the 'Session expire conversation'.
![image](https://user-images.githubusercontent.com/16264167/178139106-bb1b104e-65af-47b3-9fe2-73f7689183cb.png)


2. Set the Expire in seconds in the right side
![image](https://user-images.githubusercontent.com/16264167/178139139-62ef98e3-f45c-4513-9909-f0b5ff32fef0.png)


### Result from Session Expire trigger 
Once you've configured the 'Session Expire trigger' and set the 'Expire in seconds' then this component will run on and start to monitor the activity, In case activities are not received within the time limit Session Expire trigger' gets invoked.

You can get the results use this syntax '${turn.activity.entities}'
entities contains user details

```json

{

“lgType”: “Activity”,

“text”: [

{

  "type": "SessionExpireConversation",

  "mentioned": {

    "id": "78ee4558-a015-4817-aa04-ed6e353e0943",

    "name": "User",

    "aadObjectId": null,

    "role": "user"

  },

  "text": null

}
]
}

```

### Settings

Both settings are options  

SleepTime : This option is used to change the scan service, default agent service scans the conversation every 1000 Milliseconds, you can change this scan option on the settings page.

IsEnable : This option is use to "Enable or Disable" Agent Service triggers 

goto navigate to Project Settings and toggle the Advanced Settings View (json) add the following settings at the root of your settings JSON.

```json
"Bot.Builder.Community.Components.Trigger.SessionAgent" : {
    "SleepTime" : "2000",
    "IsEnabled" : true
  }
```
### Note
1. Both the triggers work independently, in case adding two trigger's together, the Reminder trigger 'seconds' must be always less than the session expires trigger seconds
2. After receiving the 'Session Expire Conversation trigger' the developer should handle the end of the conversation, clear memory, etc.
