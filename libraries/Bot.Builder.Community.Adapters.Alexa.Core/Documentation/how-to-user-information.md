# Alexa Core How To: Retrieve User Information

Available: 4.9.0

## Background
A message from Alexa can give you the user ID (Alexa Ids) of the speaker and device owner. If present in the request from Alexa these IDs, along with the entire request message, will be added to the ChannelData property of the Activity.

Alexa request messages follow the format described in the [Alexa Custom Skill Documentation](https://developer.amazon.com/en-US/docs/alexa/custom-skills/request-and-response-json-reference.html#request-body-syntax). The 

* session.user.userId
* context.System.user.userId

fields contain the Alexa user IDs.

If you need more then the Alexa user IDs you need to configure your skill to request those properties. See [Alexa: configuring permissions](https://developer.amazon.com/en-US/docs/alexa/custom-skills/configure-permissions-for-customer-information-in-your-skill.html).

![User Information](/libraries/Bot.Builder.Community.Adapters.Alexa.Core/Documentation/media/user-information.png?raw=true)

Settings these allows your bot to request the specified additional information about a user. Each user must consent, specifically for your skill, to give you this information. A user consents via the Alexa App (i.e. on their phone). 
A user is prompted for content when they add the skill or, on demand, when your bot sends an "Ask for Permissions Consent Card". If possible, you should design your bot to work without this additional information.
Users may not consent or may not see the consent notification on their Alexa App immediately.

When sending an "Ask for Permissions Consent Card" you can customize the message to the user. If they do not consent your bot can still communicate with the user but will not be able to get additional user information. Requests to get additonal user information will result in a 401 status code.

After getting additional user information you may want to cache that data so subsequent calls do not need to retrieve it. Be sure to comply with all Alexa licensing as well as regional laws including such things as GDPR.


_How to tell if you have permissions to get additional user information_

The user element in the request from Alexa (in the ChannelData) will include an authorization token. Documentation mentions it may also include a list of granted permissions but that can vary.
Ultimately, if the request for additional user information fails with a 401 the user has not granted your skill permissions.

_How to get the user information_

The easiest way is to use the `Alexa.NET.CustomerProfile.CustomerProfileClient` package that is a dependency of this package.

```
// Extract full SkillRequest. You may not always need this or want to do it a faster way
var channelData = turnContext.Activity.ChannelData;
var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(JsonConvert.SerializeObject(channelData));

// These are the permissions from the image above
var nameAndEmailPermissions = new List<string>
{
    CustomerProfilePermissions.FullName,
    CustomerProfilePermissions.Email
};

// Make the call to Alexa to get the additional user info. This uses the auth token in the request to complete the 
// call - if you haven't configured your skill to ask for those permissions or the user hasn't given you those permissions
// the call will fail.
var personProfileClient = new CustomerProfileClient(skillRequest);
var personName = await personProfileClient.FullName().ConfigureAwait(false);
```

_How to send an "Ask for Permissions Consent Card"_

The easiest way is to use the `Alexa.NET.CustomerProfile.CustomerProfileClient` package that is a dependency of this package.

```
const string message = "Please give me permission to get your user info from the Alexa App";
var activity = MessageFactory.Attachment(new AskForPermissionsConsentCard() { Permissions = nameAndEmailPermissions }.ToAttachment(), message);
await turnContext.SendActivityAsync(activity).ConfigureAwait(false);
```

