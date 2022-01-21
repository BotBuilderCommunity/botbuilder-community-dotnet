## Custom Input for Bot Framework Composer 

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.39-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Components.Dialogs.Input/) |

### Description
This is part of the [Bot Builder Community](https://github.com/garypretty/botbuilder-community) project which contains Bot Framework Components and other projects / packages for use with Bot Framework Composer and the Bot Builder .NET SDK v4.

This package contains additional Inputs , beyond those offered out of the box by the Bot Framework composer. Internally the Inputs uses the [Microsoft Text Recognizers](https://github.com/Microsoft/Recognizers-Text/tree/master/.NET) and [Multi-Select-choice](#Multi-Select-choice) use the Adaptive card.

[Installation](#Composer-component-installation)

Currently the following Inputs are available

| Input | Description |
| ------ | ------ |
| [Phone Number](#Phone-Number-Input) | Input a user for PhoneNumber. |
| [SocialMedia](#Social-Medial-Input) | Input a user for find mention,Hashtag. |
| [Email](#Email-Input) | Input a user for Email. |
| [Multi-Select-choice](#Multi-Select-choice) | Options to user to select one or more values |
| [Number-with-Unit](#Number-with-Unit) | Input to find unit type |
| [Number-with-Type](#Number-with-Type)| Input to find number type |
| [Guid](#Guid) | Extract a GUID from a message from the user. | 
| [Internet-Protocol](#internet-protocol) | Input to find IpAddress,Url.| 
| [True-False](#true-false) | Input to find true or false | 

### Composer component installation

1. Navigate to the Bot Framework Composer **Package Manager**.
2. Change the filter to **Community packages**.
3. Search for '***Input***' and install **Bot.Builder.Community.Components.Dialogs.Input**

![image](https://user-images.githubusercontent.com/16264167/122669120-233a1d00-d1bc-11eb-8c48-91fe4bbb0e27.png)


After install the package custom Input's is avaliable in sub-menu of "Ask a question -> Community " menu.<BR>
  
###### Note : Composer is not supported tab view for the 3rd party input types so custom inputs display in the flat view. ######


![custominput](https://user-images.githubusercontent.com/16264167/147657818-0ee8dd6d-5ca9-435e-b708-88705060c2c8.png)



#### Email Input

The EmailInput will extract an email address from a message from the user.

##### Design in composer

![image](https://user-images.githubusercontent.com/16264167/122669432-96905e80-d1bd-11eb-84c3-1ed7c17172f6.png)

###### Sample output

![image](https://user-images.githubusercontent.com/16264167/122669747-14a13500-d1bf-11eb-8e7d-20902a7cee14.png)




#### Phone Number Input

The PhoneNumberInput will extract a phone number from a message from the user;

##### Design in composer

![image](https://user-images.githubusercontent.com/16264167/122669543-32ba6580-d1be-11eb-86a0-13262f937027.png)

###### Sample output

![image](https://user-images.githubusercontent.com/16264167/122669556-406feb00-d1be-11eb-9094-2f8fc434fe3d.png)


#### Social Medial Input

The SocialMediaInput will extract one of the following types based on which MediaType enum value is passed in:

| Properties | Description | Example |
| ------ | ------ | ------ |
| Mention | extract mention tag from string | If a user enters my twitter handle is @VinothRajendran <BR> the resulting answers is @vinothrajendran |
| Hashtag | extract hash tag from string | if a user enters "Trends? Does #WM35 count?" <BR> the resulting answers is "#WM35" |


##### Design in Composer

![image](https://user-images.githubusercontent.com/16264167/122669683-c724c800-d1be-11eb-9696-dc54829df674.png)

###### Sample Output

![image](https://user-images.githubusercontent.com/16264167/122716009-02c89c00-d26a-11eb-81ef-d9244f170651.png)




#### Multi-Select-choice
  
The MultiSelectChoice will give options to the user to select one or more values
  
| Properties | Description |
| ------ | ------ |
| List of choices | Add the choices |
| Display Type | choices display option Horizontal or Vertical |
| Action Name | User 'submit' , name like "OK" , "Submit" , "Process" |

##### Design in Composer

![image](https://user-images.githubusercontent.com/16264167/122669978-ecfe9c80-d1bf-11eb-95dc-66aaa69fe8c4.png)

###### Sample output

![image](https://user-images.githubusercontent.com/16264167/122669987-f7209b00-d1bf-11eb-9c6f-4e72fe8850e5.png)

  
#### Number-with-Unit

The Number with Unit Prompt allows you to prompt for the following types of number

| Properties | Description | Example : Input | output |
| ------ | ------ | ------ | ------ |
| Currency | find any currency presented | 42 dollars | Value: 42, Unit: Dollar |
| Temperature| find any temperature presented | 25 degrees celsius | Value: 25, Unit: C |
| Age | find any age number presented |  25 years old | Value: 25, Unit: Year | 
| Dimension | find any currency presented | 6 miles |Value: 6, Unit: Mile |
  
#### Number-with-Type
The Number with Type Prompt allows you to prompt for the following types of number
| Properties | Description | Example : Input | output |
| ------ | ------ | ------ | ------ |
| Ordinal | find any ordinal number |Ok, eleventh | Value: 11 |
| Percentage | find any ordinal number | Ok, I sent one hundred percents find that | Value:100% |
| NumberRange | find any cardinal or ordinal number range |  the range between 1982 and 1987 | Value: [1982,1987) | 
| Number | find any number from the input | Total four projects in bot builder community |Value: 4 |
  
 
#### Internet-Protocol
The InternetTypePrompt will extract one of the following types based on which InternetTypePromptType enum value is passed in:
| Properties | Description | Example : Input | output |
| ------ | ------ | ------ | ------ |
| IpAddress | find ip address |my ip address is 127.0.0.0 | 127.0.0.0 |
| Url | find url | community address is https://github.com/BotBuilderCommunity | https://github.com/BotBuilderCommunity |  
  
#### Guid
The GuidPrompt will extract a GUID from a message from the user.<br>
For example, if a user enters "my azure id is 7d7b0205-9411-4a29-89ac-b9cd905886fa" when you are using the Guid prompt type, the resulting Guid is "7d7b0205-9411-4a29-89ac-b9cd905886fa"

#### True-False
 This recognizer will find any boolean value, even if its write with emoji. E.g. "👌 It's ok" , The result of this input will return True.
