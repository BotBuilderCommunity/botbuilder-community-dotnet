# Bot Framework Teams Messaging Extensions

This bot has been created to demonstrate how to use a [Microsoft Teams messaging extension](https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/what-are-messaging-extensions) to fulfill task management (To Do & Planner) right within conversations.

For detailed setup instructions please see [here](https://bisser.io/bot-framework-teams-messaging-extensions-walkthrough/) to setup everything accordingly.

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```

- [Azure AD App Registration](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)
  - The AAD app registration should have `https://token.botframework.com/.auth/web/redirect` as the redirect URI as well as the following permissions granted:
    - Group.ReadAll
    - openid
    - profile
    - Tasks.Read
    - Tasks.Read.Shared
    - Tasks.ReadWrite
    - Tasks.ReadWrite.Shared
    - User.Read
    - User.Read.All
  
- [Bot Channels Registration](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0) where you need to add your AAD app registation as an OAuth Connection Setting

## To try this sample

### Create Azure AD App Registration

The first thing we need is to setup a new Azure AD app registration, as we want to use the [Microsoft Graph](https://docs.microsoft.com/en-us/graph/overview) to handle the task management processing. Therefore we need to go over to our Azure portal and create a new Azure AD App registration (like shown [here](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#create-an-azure-active-directory-application)):

![Create AAD app registation](https://bisser.io/images/061-01.png)

While creating your app registration you need to provied the url `https://token.botframework.com/.auth/web/redirect` as a redirect URI to establish a conncetion to the Bot Framework for grabbing your authentication token.

Next up, we need to add some API permissions to our app to make sure we can use the Graph to perform certain tasks (don't forget to grant admin consent for those permissions after adding them):

![Add app permissions](https://bisser.io/images/061-02.png)

### Create Azure Bot Channels Registration

After the Azure AD App Registration has been created, we can create a new Bot Channels Registration in the Azure portal:

![Create BCR](https://bisser.io/images/061-03.png)

After it has been created, we need to add our previously created app registration to the OAuth Connection Settings of our bot:

![Add OAuth settings](https://bisser.io/images/061-04.png)

From here you need to provide the following details:

![Add OAuth settings](https://bisser.io/images/061-05.png)

After saving you can validate your service provider connection setting to see if you can connect to your AAD app registration and get a token from there:

![Test OAuth settings](https://bisser.io/images/061-06.png)

The last thing we need to grab is the Microsoft App ID and App secret for the Bot Channels Registration which can be found from the Azure AD App Registration pane as well.

### Update App Settings

Update the appsettings.json with the correct values:

```json
{
  "MicrosoftAppId": "",
  "MicrosoftAppPassword": "",
  "ConnectionNameGraph": "yourABSConnectionSettingName",
  "tenantId": "yourAADTenantID",
  "serviceUrl": "https://smba.trafficmanager.net/emea/"
}
```

### Create Teams App

Now that the bot is ready, we can create our new Microsoft Teams application using the Teams App Studio. Within there we well connect our Bot Channels Registration service we created earlier and also add a messaging extension command with the name **createTaskModule** exactly as we have used in our EchoBot.cs:

![Create Teams app](https://bisser.io/images/061-09.png)

From the Bots section, make sure to add personal, team and group as a scope so users can use your messaging extension in 1:1 & group chats and teams conversations.

In order to create the messaging extension correctly, you'll need to select the same bot as before and then add a new command like this:

![Create Teams app](https://bisser.io/images/061-10.png)

![Create Teams app](https://bisser.io/images/061-11.png)

![Create Teams app](https://bisser.io/images/061-12.png)

From the Domains and permissions section also add token.botframework.com to the list of valid domains to make authentication work:

![Create Teams app](https://bisser.io/images/061-13.png)

Now you can run the bot and then install the Teams app.

### Run Bot

- In a terminal, navigate to `Bot.Builder.Community.Samples.Teams`

    ```bash
    # change into project folder
    cd # Bot.Builder.Community.Samples.Teams
    ```

- Run the bot from a terminal or from Visual Studio, choose option A or B.

  A) From a terminal

  ```bash
  # run the bot
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `Bot.Builder.Community.Samples.Teams` folder
  - Select `Bot.Builder.Community.Samples.Teams.csproj` file
  - Press `F5` to run the project

## Testing the bot using Bot Framework Emulator

[Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator version 4.5.0 or greater from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to the bot using Bot Framework Emulator

- Launch Bot Framework Emulator
- File -> Open Bot
- Enter a Bot URL of `http://localhost:3978/api/messages`

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [.NET Core CLI tools](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x)
- [Azure CLI](https://docs.microsoft.com/cli/azure/?view=azure-cli-latest)
- [Azure Portal](https://portal.azure.com)
- [Language Understanding using LUIS](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/)
- [Channels and Bot Connector Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
