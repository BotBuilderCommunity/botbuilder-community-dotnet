# Infobip Messages Adapter Sample

A comprehensive sample bot demonstrating **real Infobip Messages API integration** using the `Bot.Builder.Community.Adapters.Infobip.Messages` library.

This sample shows how to create a multi-channel bot that **actually sends messages to Infobip** across WhatsApp, SMS, Viber, RCS, and other supported channels with **full payload logging** and **channel configuration**.

## Features

- **Real Infobip API Integration** - Actual message sending to Infobip Messages API
- **Multi-Channel Support** - WhatsApp, SMS, Viber, RCS, and more
- **Payload Logging** - See actual API requests/responses in console
- **Channel Configuration** - Multiple ways to set channels per message
- **Interactive Features** - Buttons, lists, media, templates
- **Advanced Tracking** - Callback data, entity IDs, delivery reports
- **Error Handling** - Comprehensive error handling and debugging
- **Webhook Processing** - Handle incoming messages from Infobip

## Prerequisites

- [.NET Core SDK 3.1+](https://dotnet.microsoft.com/download)
- [Infobip Account](https://www.infobip.com/) with Messages API access
- [Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) for testing

## Quick Start

### 1. Configure Your Infobip Settings

Update `appsettings.json` with your **real Infobip credentials**:

```json
{
  "InfobipApiKey": "your-actual-infobip-api-key",
  "InfobipMessagesApiBaseUrl": "https://api.infobip.com",
  "InfobipAppSecret": "your-webhook-secret",
  "DefaultSender": "sender-id",
  "DefaultChannel": "WHATSAPP",
  "EnablePayloadLogging": true
}
```

### 2. Run the Sample Bot

```bash
cd "samples/Infobip Messages Adapter Sample"
dotnet restore
dotnet build
dotnet run
```

The bot will start on `http://localhost:3978` (check console for exact port).

### 3. Test with Bot Framework Emulator

1. Open [Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator/releases)
2. Connect to: `http://localhost:3978/api/messages`
3. Try these commands to see **real API calls**:

```
help              # Show all available commands
test whatsapp     # Send via WhatsApp (real API call)
test sms          # Send via SMS (real API call)
test rcs          # Send via RCS (real API call)
send whatsapp     # Interactive message sending workflow
status            # Show configuration
```

### 4. Check Console Output

You'll see **actual API payloads** like this:

```
=== INFOBIP MESSAGES API REQUEST ===
Endpoint: https://api.infobip.com/messages-api/1/messages
Payload: {
  "messages": [
    {
      "sender": "sender-id",
      "channel": "RCS",
      "destinations": [{"to": "user-conversation-id"}],
      "content": {
        "body": {
          "type": "TEXT",
          "text": "This message is sent via RCS!"
        }
      }
    }
  ]
}
===================================
```

## Testing Features

### Channel Testing Commands

- `test whatsapp` - Send message via WhatsApp API
- `test sms` - Send message via SMS API
- `test rcs` - Send message via RCS (Rich Communication Services) API
- `send whatsapp` - Interactive WhatsApp message with custom API key
- `send sms` - Interactive SMS message with custom API key
- `send viber` - Interactive Viber Business Messages
- `send rcs` - Interactive RCS message with custom API key

### Utility Commands

- `help` - Show all available commands
- `status` - Show current configuration

## Webhook Testing (Real Infobip Integration)

### 1. Expose Your Bot with ngrok

```bash
# Install ngrok: https://ngrok.com/download
ngrok http 3978 --host-header="localhost:3978"
```

Copy the HTTPS URL (e.g., `https://abc123.ngrok-free.app`)

### 2. Configure Infobip Webhook

1. Login to [Infobip Portal](https://portal.infobip.com/)
2. Navigate to your channel configuration
3. Set webhook URL to: `https://abc123.ngrok-free.app/api/infobip`
4. Enable delivery and seen reports (optional)

### 3. Test with Real Channels

- Send **WhatsApp message** to your Infobip number
- Send **SMS** to your Infobip number
- Check **bot console logs** for incoming webhooks
- Verify **bot responses** are sent to your device

## Architecture

```
Incoming Request
       ?
   BotController
       ?
   [Route Decision]
       ?
???????????????????    ???????????????????????????
? Bot Framework   ?    ? Infobip Messages        ?
? Emulator        ?    ? Webhook                 ?
?                 ?    ?                         ?
? User-Agent:     ?    ? Headers:                ?
? Microsoft-BF    ?    ? X-Infobip-Signature     ?
?                 ?    ? Infobip-Signature       ?
? ?               ?    ? ?                       ?
? Standard        ?    ? Infobip                 ?
? Adapter         ?    ? Adapter                 ?
???????????????????    ???????????????????????????
       ?                        ?
   EchoBot ???????????????????? EchoBot
```

## Configuration

### Local Development (Bot Framework Emulator)

- `MicrosoftAppId`: `""` (empty)
- `MicrosoftAppPassword`: `""` (empty)
- No signature verification required
- Routes through standard Bot Framework adapter

### Production (Real Infobip Webhooks)

- Configure webhook URL in Infobip Portal
- Signature verification enabled automatically
- Routes through Infobip Messages adapter
- Requires proper webhook configuration

## Project Structure

```
Infobip Messages Adapter Sample/
??? Bots/
?   ??? EchoBot.cs                    # Main bot logic with rich message support
??? Controllers/
?   ??? BotController.cs              # Standard Bot Framework endpoint
?   ??? InfobipMessagesController.cs  # Infobip webhook endpoint
??? Properties/
?   ??? launchSettings.json           # Launch configuration
??? wwwroot/
?   ??? index.html                    # Testing instructions page
??? AdapterWithErrorHandler.cs       # Standard Bot Framework adapter
??? Program.cs                        # Application entry point
??? Startup.cs                        # Dependency injection configuration
??? appsettings.json                  # Configuration settings
??? README.md                         # This documentation
```

## Troubleshooting

### Authentication Issues

- **"Cannot post activity. Unauthorized"**: Check port configuration (should be 3978)
- **"Request signature verification failed"**: Expected for emulator requests (they don't have Infobip signatures)

### Connection Issues

1. **Check port**: Ensure bot runs on `http://localhost:3978`
2. **Clear emulator cache**: Clear Bot Framework Emulator history
3. **Restart everything**: Stop bot, restart, reconnect emulator
4. **Check firewall**: Ensure port 3978 isn't blocked

### Message Sending Issues

1. **Verify API key**: Check your Infobip API key is correct and active
2. **Check sender configuration**: Ensure sender number is configured in Infobip account
3. **Validate phone number format**: Use international format (+country code)
4. **Verify channel setup**: Ensure channel is configured in your Infobip account

## Security Features

- **Dynamic API key collection** - No hardcoded keys in source code
- **Custom sender configuration** - Configure sender per message
- **Temporary credential storage** - Credentials only stored during active session
- **Complete input validation** - Phone number and API key validation

## Getting Help

1. Check console logs for detailed error information
2. Use `status` command to verify bot configuration
3. Test with simple commands first (`hello`, `help`)
4. For webhook testing, ensure ngrok tunnel is properly configured

## License

This project is part of the Bot Builder Community project and follows the same license terms.

## Contributing

Issues and pull requests are welcome! Please see the main Bot Builder Community repository for contribution guidelines.Issues and pull requests are welcome! Please see the main Bot Builder Community repository for contribution guidelines.
