# TextRecognizer Middleware Component for Bot Framework Composer

## Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [Available via NuGet](https://www.nuget.org/packages/Bot.Builder.Community.Components.Middleware.TextRecognizer/) |

## Description

This is part of the [Bot Builder Community](https://github.com/botbuildercommunity) project which contains open source Bot Framework Composer components, along with other extensions, including middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Text Recognizer Middleware library is a compliment to the Text Recognizer custom input.These middleware components can be used to identify certain text sequences that you might want to alter prior to appearing on the chat window. For example, turning a URL into an actual link, or turning a hashtag into a link that points to a Twitter search.

Supported middleware classes include:

| Class | Description | Property  |
| ---- | ----------- | ----------- |
| `EmailRecognizer` | Extract an email address from a message from the user | `${turn.EmailEntities}` |
| `PhoneNumberRecognizer` | Extract a phone number from a message from the user | `${turn.PhoneNumberEntities}` |
| `InternetProtocol` | Extract one of the (IpAddress or Url) types based on which InternetProtocolType enum value is passed in | `${turn.InternetTypeEntities}` |
| `SocialMediaRecognizer` | Extract one of the (Mention or Hashtag) types based on which SocialMediaType enum value is passed in | `${turn.MediaTypeEntities}` |


| Enum Type  | Value |
| ---- | ----------- |
| InternetProtocolType |  IpAddress ,  Url |
| SocialMediaType |  Mention ,  Hashtag |



### Composer component installation

1. Navigate to the Bot Framework Composer **Package Manager**.
2. Change the filter to **Community packages**.
3. Search for 'TextRecognizer' and install **Bot.Builder.Community.Components.Middleware.TextRecognizer**

![image](https://user-images.githubusercontent.com/16264167/118684398-76f2c880-b802-11eb-8c92-898cd6d8d9f8.png)


### Composer component configuration

1. Within Composer, navigate to **Project Settings** and toggle the **Advanced Settings View (json)**.
2. Add the following settings at the root of your settings JSON, replacing the placeholders as described below.

```json
"components": [
{
    "name": "Bot.Builder.Community.Components.Middleware.TextRecognizer",
    "settingsPrefix": "Bot.Builder.Community.Components.Middleware.TextRecognizer"
}
```

```json
"Bot.Builder.Community.Components.Middleware.TextRecognizer": {
    "IsEmailEnable": true,
    "IsPhoneNumberEnable": true,
    "IsSocialMediaEnable": true,
    "MediaType": "Mention",
    "IsInternetProtocolEnable": true,
    "InternetProtocolType": "url",
    "Locale": "de-De"
  },
```

### NOTE
If you do not want to use particular middleware, you can simply set the value of "Enabled property" to false.

Example to disable SocialMediaMiddleware set the flag to false

```json
IsSocialMediaEnable : false
```

### Getting the TextRecognizer result from Middleware

Once you've configured the middleware component and enabled it from the Connections (`"..Enabled": true`) then this component will run on every message received (`ActivityType is Message`). 

You can now get the results using below settings;

EmailRecognizer Middleware<BR>
`${turn.EmailEntities}`


PhoneNumberRecognizer Middleware<BR>
`${turn.PhoneNumberEntities}`


SocialMediaRecognizer Middleware<BR>
`${turn.MediaTypeEntities}`

InternetProtocol Middleware<BR>
`${turn.InternetTypeEntities}`
    
Composer Settings to display the results
    
![image](https://user-images.githubusercontent.com/16264167/118853093-ac1b1b80-b8d3-11eb-8b48-28185e3cc8a0.png)
    

This will give result example email address
    
![image](https://user-images.githubusercontent.com/16264167/118853293-e2589b00-b8d3-11eb-8389-01ddec129829.png)


