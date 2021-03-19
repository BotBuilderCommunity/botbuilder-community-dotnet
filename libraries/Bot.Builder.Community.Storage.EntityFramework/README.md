## EntityFrameworkCore storage for Bot Builder v4 .NET SDK

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.184-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Storage.EntityFramework/) |

### Description

This is part of the [Bot Builder Community Extensions](https://github.com/garypretty/botbuilder-community) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

Entity Framework based storage extension for bots created using Microsoft Bot Framework.

### Installation

Available via NuGet package [Bot.Builder.Community.Storage.EntityFramework](https://www.nuget.org/packages/Bot.Builder.Community.Storage.EntityFramework/)

Install into your project using the following command in the package manager;
```
    PM> Install-Package Bot.Builder.Community.Storage.EntityFramework
```

### Sample

A basic sample for using this component can be found [here](../../samples/EntityFramework%20Storage%20Sample).

### Usage

The extension uses Microsoft.EntityFrameworkCore as the library for connecting to Sql server. Therefore a connection string to an existing database must be provided during startup. 

To use EntityFrameworkStorage, you can replace the existing `MemoryStorage` or any existing storage component with `EntityFrameworkStorage` component. 

Following are examples where two storage components are used for storing user and conversation data respectively.

```csharp
// Conversation State Storage
IStorage conversationDataStore = new EntityFrameworkStorage(connectionString);
var conversationState = new ConversationState(conversationDataStore);

// User State Storage
IStorage userDataStore = new EntityFrameworkStorage(connectionString);
var userState = new UserState(userDataStore);
```

To use EntityFrameworkTranscriptStore with TranscriptLoggerMiddleware: 

```csharp
store = new EntityFrameworkTranscriptStore(Configuration.GetSection("BotDataConnectionString").Value);

var transcriptMiddleware = new TranscriptLoggerMiddleware(store);
options.Middleware.Add(transcriptMiddleware);
```

#### Behaviour

For concurrency, EntityFrameworkStorage follows the semantics of **Last Write Wins** and does not implement ETag or isolation level based consistency by default.  However, you can provide an Isolation level in EntityFrameworkStorageOptions. The default is IsolationLevel.ReadCommitted.  

#### Table Creation Script

```sql
CREATE TABLE [dbo].[BotDataEntity](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RealId] [varchar](1024) NOT NULL UNIQUE,
	[Document] [nvarchar](max) NOT NULL,
	[CreatedTime] [datetimeoffset](7) Not NULL,
	[TimeStamp] [datetimeoffset](7) Not NULL,
 CONSTRAINT [PK_BotDataEntity] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[BotDataEntity] ADD  DEFAULT (getutcdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[BotDataEntity] ADD  DEFAULT (getutcdate()) FOR [TimeStamp]
GO

CREATE TABLE [dbo].[TranscriptEntity](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Channel] [varchar](256) NOT NULL,
	[Conversation] [varchar](1024) NOT NULL,
    [Activity] [nvarchar](max) NOT NULL,
	[TimeStamp] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_TranscriptEntity] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY])
GO
CREATE NONCLUSTERED INDEX [IX_TranscriptChannel] ON [dbo].[TranscriptEntity]
(
	[Channel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_TranscriptTimeStamp] ON [dbo].[TranscriptEntity]
(
	[TimeStamp] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_TranscriptConversation] ON [dbo].[TranscriptEntity]
(
	[Conversation] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[TranscriptEntity] ADD  DEFAULT (getutcdate()) FOR [TimeStamp]
GO
```
