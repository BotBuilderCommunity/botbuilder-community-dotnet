## Elasticsearch storage for Bot Builder v4 .NET SDK

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.184-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Storage.Elasticsearch/) |

### Description

This is part of the [Bot Builder Community Extensions](https://github.com/garypretty/botbuilder-community) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

Elasticsearch based storage extension for bots created using Microsoft Bot Framework.

### Installation

Available via NuGet package [Bot.Builder.Community.Storage.Elasticsearch](https://www.nuget.org/packages/Bot.Builder.Community.Storage.Elasticsearch/)

Install into your project using the following command in the package manager;
```
    PM> Install-Package Bot.Builder.Community.Storage.Elasticsearch
```

### Usage

The extension uses NEST as the native client for connecting and working with Elasticsearch. Therefore the configuration options have been created following NEST standards and guidelines. To use this component, you can replace the existing `MemoryStorage` or any existing storage component with the `ElasticsearchStorage` component. 
Although you can use one storage component for storing user and conversation state, it is recommended to create different storage components for storing user, conversation or any other type of data.

Following are examples where two storage components are used for storing user and conversation data respectively.

```csharp
// Conversation State Storage
var conversationDataStorageOptions = new ElasticsearchStorageOptions();
conversationDataStorageOptions.ElasticsearchEndpoint = new Uri("http://localhost:9200");
conversationDataStorageOptions.UserName = "xxxxx";
conversationDataStorageOptions.Password = "yyyyy";
conversationDataStorageOptions.IndexName = "conversation-data";
conversationDataStorageOptions.IndexMappingDepthLimit = 100000;

IStorage conversationDataStore = new ElasticsearchStorage(conversationDataStorageOptions);
var conversationState = new ConversationState(conversationDataStore);

// User State Storage
var userDataStorageOptions = new ElasticsearchStorageOptions();
userDataStorageOptions.ElasticsearchEndpoint = new Uri(elasticsearchStorageSettings.ElasticsearchEndpoint);
userDataStorageOptions.UserName = "xxxxx";
userDataStorageOptions.Password = "yyyyy";
userDataStorageOptions.IndexName = "user-data";
userDataStorageOptions.IndexMappingDepthLimit = 100000;

IStorage userDataStore = new ElasticsearchStorage(userDataStorageOptions);
var userState = new UserState(userDataStore);
```

#### Behaviour

The component automatically creates and maintains rolling indexes for storing data. This means if you are specifying the `IndexName` as conversation-data, on any given day the index would be named as conversation-data-current-date for e.g. conversation-data-09-27-2018. This would help to expire/delete old indexes by running a periodic job.

As soon as the component does not find the index specific to the current day, it creates a new rolling index and aliases it with `IndexName` for e.g. conversation-data. The component writes and maintains the user or conversation data per se on a single document within the index. While writing the data it uses the index name. However, while reading the data it uses the alias name.

For concurrency, it follows the semantics of **Last Write Wins** as dictated by the BotState implementation.
