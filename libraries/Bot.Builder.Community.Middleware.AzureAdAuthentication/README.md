## Azure Active Directory Authentication Middleware

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.39-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.AzureAdAuthentication/) |

### Description
This is part of the [Bot Builder Community](https://github.com/garypretty/botbuilder-community) project which contains Bot Framework Components and other projects / packages for use with Bot Framework Composer and the Bot Builder .NET SDK v4.

This middleware will allow your bot to authenticate with Azure AD.  It was created to support integration with Microsoft Graph but it will work with any application that uses the OAuth 2.0 authorization code flow. https://docs.microsoft.com/en-gb/azure/active-directory/develop/v2-oauth2-auth-code-flow

It supports:
- Request an authorization code (Client side user consent)
- Request an access token using authorization code (Server side)
- Request an access token using refresh token (Server side)

**Note: This middleware requires you provide a class to store OAuth access/refresh tokens somewhere. I have purposefully not prescribed how to store these access tokens.  If you make use of this middleware you need to provide an implementation of `IAuthTokenStorage`. This should use secure storage like Azure Key Vault. Read up on that here. https://docs.microsoft.com/en-us/azure/key-vault/quick-create-net.**

### Installation 

Available via NuGet package [Bot.Builder.Community.Middleware.AzureAdAuthentication](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.AzureAdAuthentication/)

Install into your project using the following command in the package manager;
```
    PM> Install-Package Bot.Builder.Community.Middleware.AzureAdAuthentication
```

### Usage

##### Step 1 - Define an implementation of `IAuthTokenStorage` to store and retrieve tokens
This is an example of an in-memory `IAuthTokenStorage`. This is to demonstrate the principle only.

**💣 AGAIN, DO NOT USE THE InMemoryAuthTokenStorage OR DiskAuthTokenStorage COMPONENTS FOR PRODUCTION APPLICATIONS 💣** 

```
public class InMemoryAuthTokenStorage : IAuthTokenStorage
{
    private static readonly Dictionary<string, ConversationAuthToken> InMemoryDictionary = new Dictionary<string, ConversationAuthToken>();

    public ConversationAuthToken LoadConfiguration(string id)
    {
        if (InMemoryDictionary.ContainsKey(id))
        {
            return InMemoryDictionary[id];
        }

        return null;
    }

    public void SaveConfiguration(ConversationAuthToken token)
    {
        InMemoryDictionary[state.Id] = token;
    }
}
```
##### Step 2 - Register the middleware

To ensure that users are always authenticated, add this middleware to the start of the pipeline.

In your `Startup.cs` file, register an your `IAuthTokenStorage` implementation as a singleton into the asp dotnet core ioc container. Then configure your bot type to use an instance of `AzureAdAuthMiddleware`.


```
var tokenStorage = new InMemoryAuthTokenStorage();
services.AddSingleton<IAuthTokenStorage>(tokenStorage);
          
services.AddBot<Bot>((options) => {
    options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                
    options.Middleware.Add(new AzureAdAuthMiddleware(tokenStorage, Configuration));
    // more middleware
});
```

Note this requires an instance of `IConfiguration` passing to it.  Use the instance injected into the `Startup.cs` class.  

The configuration can be read from your `appsettings.json` file which needs the following keys (I've included some sample permissions- you can change these to meet your needs).
```
{
  "AzureAdTenant": "<AZURE AD TENANT>",
  "AppClientId": "<AZURE AD APPLICATION ID>",
  "AppRedirectUri": "https://<HOSTNAME>:<PORT>/redirect",
  "PermissionsRequested": "Calendars.ReadWrite.Shared User.ReadBasic.All People.Read",
  "AppClientSecret": "<AZURE AD APPLICATION KEY>"
}
```
