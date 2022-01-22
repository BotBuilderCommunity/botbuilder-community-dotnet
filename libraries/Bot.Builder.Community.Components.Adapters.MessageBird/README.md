# Unofficial MessageBird WhatsApp Adapter for Bot Framework Composer 


# Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains Bot Framework Components and other projects / packages for use with Bot Framework Composer and the Bot Builder .NET SDK v4.

The MessageBird WhatsApp adapter enables receiving and sending WhatsApp messages. The MessageBird adapter allows you to add an additional endpoint to your bot for receiving WhatsApp messages. The MessageBird endpoint can be used in conjunction with other channels meaning, for example, you can have a bot exposed on out of the box channels such as Facebook and Teams, but also via an MessageBird (as well as side by side with the Google/Twitter adapters also available from the Bot Builder Community Project).


The adapter currently supports the following scenarios:

* Send/receive messages with WhatsApp Sandbox
* Send/receive text messages
* Send/receive media messages (document, image, video, audio) - Supported formats for media message types available [here](https://developers.facebook.com/docs/whatsapp/api/media/#supported-files)
* Send/receive location messages
* Verification of incoming MessageBird requests (There is one issue for delivery report webhook verification, MessageBird team is investigating.)
* Receive delivery reports
* Full incoming request from MessageBird is added to the incoming activity as ChannelData

## Usage

- [Prerequisites](#Prerequisites)
- [Component Installation](#Component-installation-via-Composer-Package-Manager)
- [Configure the MessageBird connection in Composer](#Configuring-the-MessageBird-connection-in-Bot-Framework-Composer)

### Prerequisites

- [Bot Framework Composer](https://dev.botframework.com/)

## Component installation via Composer Package Manager

1. Go to Package Manager (in the left hand navigation within Composer).

2. Within in Package Manager, search for an install the latest version of Bot.Builder.Community.Components.Adapters.MessageBird.

## Configuring the MessageBird connection in Bot Framework Composer

1. In Composer, go to your project settings. Within the 'Connections' tab, there should be a new entry called `MessageBird Connection`. Select `Configure` to configure the new connection.


2. A modal will pop up. Fill the fields with the values



3. Once you close the modal, your connection should appear as configured in the bot settings.


- Signing & Access key information ref & MessageBird [here](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/develop/libraries/Bot.Builder.Community.Adapters.MessageBird)

