# MongoDB Storage Integration

This functionality enables seamless integration of MongoDB storage into your existing project. With this integration, you can easily store and retrieve data using MongoDB as your backend storage system.

## Features
- Robust and scalable MongoDB storage solution
- Easy configuration and registration of MongoDB storage using extension methods
- Support for custom serialization of allowed types

## Getting Started

To get started, follow these steps:

1. Install the required package `Bot.Builder.Community.Storage.MongoDB` for your project


2. Add the following using statements to your project:
```csharp
using Bot.Builder.Community.Storage.MongoDB;
```

3. In the ConfigureServices method of your Startup.cs file, add the following code to configure and register MongoDB storage:
```csharp
services.AddMongoDbStorage(settings =>
{
    settings.ConfigureOptions(Configuration.GetSection("MongoDb"));
    settings.RegisterTypes(
        typeof(YourType1),
        typeof(YourType2)
    );
});
```
Replace YourType with the type(s) you want to store in 

4. Update your appsettings.json file to include the MongoDB configuration settings:

```json
{
    "MongoDb": {
        "ConnectionString": "your_connection_string",
        "Database": "your_database_name",
        "Collection": "your_collection_name"
    }
}
```
Replace your_connection_string, your_database_name, and your_collection_name with the appropriate values for your MongoDB instance.