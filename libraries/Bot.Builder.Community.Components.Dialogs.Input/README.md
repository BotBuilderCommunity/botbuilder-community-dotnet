## Custom Input for Bot Framework Composer 

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.39-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Components.Dialogs.Input/) |

### Description
This is part of the [Bot Builder Community](https://github.com/garypretty/botbuilder-community) project which contains Bot Framework Components and other projects / packages for use with Bot Framework Composer and the Bot Builder .NET SDK v4.

This package contains additional Inputs , beyond those offered out of the box by the Bot Framework composer

Currently the following Prompts are available;

| Prompt | Description |
| ------ | ------ |
| [Phone Number](#Phone-Number-Input) | Input a user for PhoneNumber. |
| [SocialMedia](#Social-Medial-Input) | Input a user for find mention,Hashtag. |
| [Email](#Email-Input) | Input a user for Email. |
| [Multi-Select-choice](#Multi-Select-choice) | Options to user select one or more values |
| Number with Unit | coming soon |
| Number with Type | coming soon |
| Guid | coming soon | 
| Internet Protocol | coming soon | 

### Composer component installation

1. Navigate to the Bot Framework Composer **Package Manager**.
2. Change the filter to **Community packages**.
3. Search for 'Dialogs.Input' and install **Bot.Builder.Community.Components.Dialogs.Input**

![image](https://user-images.githubusercontent.com/16264167/122669120-233a1d00-d1bc-11eb-8c48-91fe4bbb0e27.png)


### Add custom input in composer 

Custom Input has added in sub-menu of "Ask a question" menu.

![image](https://user-images.githubusercontent.com/16264167/122669223-93e13980-d1bc-11eb-96bc-003c34802854.png)


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

The SocialMediaInput will extract one of the following types based on which SocialMediaPromptType enum value is passed in:

* Mention
* Hashtag

##### Design in Composer

![image](https://user-images.githubusercontent.com/16264167/122669683-c724c800-d1be-11eb-9696-dc54829df674.png)

###### Sample Output

![image](https://user-images.githubusercontent.com/16264167/122669692-d146c680-d1be-11eb-986e-979b630f28a8.png)



#### Multi-Select-choice
The MultiSelectChoice will give option to the user to select one or more value

List of choices : Add the choices options 

Display Type : This option is use to how choices should be display Horizontal or Vertical

Action Name :  User submit the selected choices ( ex : Submit)


##### Design in Composer

![image](https://user-images.githubusercontent.com/16264167/122669978-ecfe9c80-d1bf-11eb-95dc-66aaa69fe8c4.png)

###### Sample output

![image](https://user-images.githubusercontent.com/16264167/122669987-f7209b00-d1bf-11eb-9c6f-4e72fe8850e5.png)


