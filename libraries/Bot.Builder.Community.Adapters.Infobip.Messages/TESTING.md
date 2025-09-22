# Infobip Messages API Adapter Testing Guide

This guide provides comprehensive testing instructions for the Infobip Messages API Adapter, including Bot Framework Emulator integration and using the existing sample bot.

## ?? **Quick Start - Using the Sample Bot**

The fastest way to test the Infobip Messages API adapter is using the included sample bot.

### Using the Existing Sample Bot

#### 1. Navigate to Sample Directory

```bash
cd ../../samples/Infobip\ Messages\ Adapter\ Sample
```

#### 2. Configure Credentials

Update `appsettings.json` with your Infobip credentials:

```json
{
  "InfobipApiKey": "your-api-key-here",
  "InfobipMessagesApiBaseUrl": "https://2kdgrw.api.infobip.com",
  "InfobipAppSecret": "your-app-secret-here",
  "DefaultSender": "sender-id",
  "DefaultChannel": "WHATSAPP",
  "EnablePayloadLogging": true,
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Bot.Builder.Community.Adapters.Infobip": "Debug"
    }
  }
}
```

#### 3. Run the Sample Bot

```bash
dotnet restore
dotnet build
dotnet run
```

#### 4. Test with Bot Framework Emulator

1. **Open Bot Framework Emulator**
2. **Connect to**: `http://localhost:3978/api/messages`
3. **Try test commands**:
   - `"help"` ? Shows all available commands
   - `"test whatsapp"` ? Sends via WhatsApp with channel logging
   - `"test sms"` ? Sends via SMS with channel logging
   - `"test buttons"` ? Interactive buttons
   - `"debug payload"` ? Shows expected payload format
   - `"status"` ? Shows current configuration

**Expected Console Output:**
```
=== INFOBIP MESSAGES API REQUEST ===
Endpoint: https://2kdgrw.api.infobip.com/messages-api/1/messages
Payload: {
  "messages": [
    {
      "sender": "sender-id",
      "channel": "WHATSAPP",
      "destinations": [{"to": "user-conversation-id"}],
      "content": {
        "body": {
          "type": "TEXT",
          "text": "?? This message is sent via WhatsApp!"
        }
      }
    }
  ]
}
===================================
```

## ?? **Sample Bot Test Commands**

The sample bot includes comprehensive test commands to validate all features:

### Channel Testing
- `"test whatsapp"` - Send via WhatsApp with explicit channel
- `"test sms"` - Send via SMS with explicit channel  
- `"test viber"` - Send via Viber Business Messages
- `"test channels"` - Show all supported channels

### Message Types
- `"test buttons"` - Interactive buttons (WhatsApp optimized)
- `"test list"` - Interactive list (WhatsApp specific)
- `"test media"` - Media message with image
- `"test location"` - Location sharing
- `"test template"` - WhatsApp template message

### Advanced Features
- `"test callback"` - Message with callback data tracking
- `"test tracking"` - Message with entity/app ID tracking
- `"test fallback"` - Multi-channel fallback configuration
- `"debug payload"` - Show expected payload format

### Utility Commands
- `"help"` - Show all available commands
- `"status"` - Show current configuration and endpoints

## ?? **Payload Format Verification**

The sample bot automatically ensures correct payload formatting:

### Expected Format (ALL CAPS):
```json
{
    "messages": [
        {
            "sender": "sender-id",
            "channel": "WHATSAPP",
            "destinations": [
                {
                    "to": "user-conversation-id"
                }
            ],
            "content": {
                "body": {
                    "type": "TEXT",
                    "text": "Hello World"
                }
            }
        }
    ]
}
```

### Key Requirements:
- ? Channel names: `"WHATSAPP"`, `"SMS"`, `"VIBER_BM"`, etc.
- ? Content types: `"TEXT"`, `"DOCUMENT"`, `"LOCATION"`, etc.
- ? Structure matches Infobip Messages API exactly

## ?? **Testing Scenarios**

### Test 1: Basic Functionality with Sample Bot

1. **Run sample bot** from `../../samples/Infobip Messages Adapter Sample`
2. **Connect Bot Framework Emulator** to `http://localhost:3978/api/messages`
3. **Test basic commands:**
   - Send `"help"` ? Should show available commands
   - Send `"test whatsapp"` ? Should log WhatsApp channel selection
   - Send `"test sms"` ? Should log SMS channel selection
   - Send `"debug payload"` ? Should show correct payload format

**Expected Results:**
- ? Bot responds to all commands
- ? Console shows detailed payload logging
- ? Channel field is always present and in ALL CAPS
- ? Content types are in ALL CAPS

### Test 2: Interactive Messages

1. **Test interactive buttons:**
   ```
   Send: "test buttons"
   Expected: Suggested actions with WhatsApp channel
   Console: Should show WHATSAPP channel and button structure
   ```

2. **Test interactive lists:**
   ```
   Send: "test list" 
   Expected: Interactive list with product options
   Console: Should show proper list structure for WhatsApp
   ```

### Test 3: Channel Configuration

1. **Test explicit channel setting:**
   ```
   Send: "test whatsapp"
   Console Output: "?? Channel detected from channel data: WHATSAPP"
   ```

2. **Test fallback configuration:**
   ```
   Send: "test fallback"
   Console Output: Shows primary and fallback channels
   ```

### Test 4: Advanced Features

1. **Test callback data:**
   ```
   Send: "test callback"
   Console: Should show callback data in payload
   ```

2. **Test tracking features:**
   ```
   Send: "test tracking"
   Console: Should show entity ID and application ID
   ```

### Test 5: Full Integration with ngrok

#### Step 1: Start ngrok tunnel

```bash
# Use actual port from sample bot console output
ngrok http 3978 --host-header="localhost:3978"

# Copy the HTTPS URL (e.g., https://abc123.ngrok-free.app)
```

#### Step 2: Configure Infobip Webhook

1. **Login to Infobip Portal**
2. **Navigate to your channel configuration**
3. **Set webhook URL to**: `https://abc123.ngrok-free.app/api/infobip/messages`
4. **Enable delivery and seen reports** (optional)

#### Step 3: Test with Real Channels

- **Send WhatsApp message** to your Infobip number
- **Send SMS** to your Infobip number  
- **Check sample bot logs** for incoming webhook data
- **Verify bot responses** are sent to your device

**Expected Results:**
- ? Messages received from real channels
- ? Bot responses sent to real devices
- ? Payload logging shows correct format
- ? Delivery reports received (if configured)

## ?? **Alternative: Create Your Own Test Bot**

If you prefer to create a minimal test bot from scratch, follow the guide in the previous version. However, using the existing sample bot is recommended as it includes all features and proper configuration.

## ?? **Debugging and Troubleshooting**

### Console Output Validation

The sample bot provides detailed console output for debugging:

```
?? Converting Bot Framework Activity to Infobip Messages API request
?? Channel detected from channel data: WHATSAPP
?? Sender: sender-id
?? Destination: user-conversation-id
??? Entity ID: entity-20231201
?? Application ID: sample-bot-app
? Successfully converted Activity to Infobip Messages API request

=== INFOBIP MESSAGES API REQUEST ===
Endpoint: https://2kdgrw.api.infobip.com/messages-api/1/messages
Payload: {"messages":[...]"}
===================================

? Infobip Messages API response successful
?? Response Payload: {"messageId":"msg-12345",...}
```

### Common Issues

#### 1. **Sample bot not starting**
```bash
cd ../../samples/Infobip\ Messages\ Adapter\ Sample
dotnet restore
dotnet build
```

#### 2. **Missing configuration**
- Check `appsettings.json` exists in sample directory
- Verify all required fields are populated

#### 3. **Channel not set correctly**
- Sample bot automatically sets channels
- Check console logs for channel detection messages

#### 4. **Payload format incorrect**
- Sample bot ensures ALL CAPS automatically
- Use `"debug payload"` command to verify format

### Enable Detailed Logging

The sample includes comprehensive logging by default. For even more detail:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Bot.Builder.Community.Adapters.Infobip": "Debug",
      "Microsoft": "Warning"
    }
  }
}
```

## ? **Test Results Checklist**

### Sample Bot Functionality
- [ ] ? Sample bot starts without errors
- [ ] ? Console shows correct HTTP/HTTPS URLs
- [ ] ? Bot responds to all test commands
- [ ] ? Help command shows available options
- [ ] ? Status command shows configuration

### Payload Format Validation
- [ ] ? Channel names are in ALL CAPS
- [ ] ? Content types are in ALL CAPS  
- [ ] ? Structure matches expected format
- [ ] ? Console logging shows complete payloads
- [ ] ? Debug payload command works

### Channel Configuration
- [ ] ? WhatsApp channel selection works
- [ ] ? SMS channel selection works
- [ ] ? Channel detection logging appears
- [ ] ? Fallback configuration works
- [ ] ? Default channel is used when not specified

### Advanced Features
- [ ] ? Interactive buttons work
- [ ] ? Interactive lists work (WhatsApp)
- [ ] ? Media messages work
- [ ] ? Callback data tracking works
- [ ] ? Entity/Application ID tracking works

### Integration Testing
- [ ] ? ngrok tunnel established
- [ ] ? Infobip webhook endpoint accessible
- [ ] ? Messages received from real channels
- [ ] ? Bot responses sent to real devices
- [ ] ? Delivery reports received

## ?? **Additional Resources**

- **Sample Bot Guide**: [SAMPLE_BOT.md](SAMPLE_BOT.md)
- **Channel Configuration**: [CHANNEL_CONFIGURATION.md](CHANNEL_CONFIGURATION.md)
- **Infobip Portal**: [https://portal.infobip.com](https://portal.infobip.com)
- **Infobip API Docs**: [Messages API Documentation](https://www.infobip.com/docs/api/platform/messages-api)

---

Using the existing sample bot provides the fastest and most comprehensive way to test all Infobip Messages API adapter features with proper payload formatting and debugging capabilities.