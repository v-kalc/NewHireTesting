


## Troubleshooting

### Generic possible issues

**1. Bot returns 'You are not authorized to access this Bot. Please contact administrator.'**

- Please verify account you are using in Microsoft Teams is of same tenant.
- Check whether tenant ID mentioned in application settings section of configuration menu for bot app service matches Microsoft Teams account tenant. Ref: [https://docs.microsoft.com/en-us/azure/app-service/configure-common](https://docs.microsoft.com/en-us/azure/app-service/configure-common)

![Checking application settings in app service](https://docs.microsoft.com/en-us/azure/app-service/media/configure-common/open-ui.png)

**2. How to change images in a bot**

- To change the image in Bot go to the URL as [/Source/Microsoft.Teams.Apps.NewHireOnboarding/wwwroot/Artifacts/](/Source/Microsoft.Teams.Apps.NewHireOnboarding/wwwroot/Artifacts/)
- Here in Artifacts folder, find the respective image that needs to be updated and replace new image with the same name as previous image.

**3. How to change/add text or text on button in a bot**

- To change or add text in bot the redirect to the below URL.
[/Source/Microsoft.Teams.Apps.NewHireOnboarding/Resources/](/Source/Microsoft.Teams.Apps.NewHireOnboarding/Resources/)
- Here in String.resx file, update the text only in "Value" column.
- Then open the String.designer.cs file, remove .Resources from the namespace path.

### Problems while deploying to Azure

**1. Error when attempting to reuse a Microsoft Azure Active Directory application ID for the bot registration**

Bot is not valid. Errors: MsaAppId is already in use.

- Creating the resource of type Microsoft.BotService/botServices failed with status "BadRequest"

This happens when the Microsoft Azure application ID entered during the setup of the deployment has already been used and registered for a bot.

**Fix:**
Either register a new Microsoft Azure Active Directory application or delete the bot registration that is currently using the attempted Microsoft Azure application ID.

### Problems while customizing bot

**1. Client app packages restoration failure for first build**
Once repository is cloned and you try to build the solution in Visual Studio, build fails due to multiple NPM packages fails to install.

**Fix:**
If you have cloned using Visual Studio, please retry building solution after closing and re-opening the Visual Studio.
	If you still get errors, navigate to 'ClientApp' folder in project directory using file explorer. Open command prompt at this location and type 'npm install'. This will install app the packages for client app. Once it is done, try rebuild solution from Visual Studio.

## Problems in bot
If facing any issues related to bot.

**Fix**
Please go to app-insights and check for errors.
- Go to [azure portal](http://portal.azure.com/)
- Go to App-insights related to your app.
- Open Logs (Analytics)
- Select Time Range & fire the query from different tables like exceptions, customEvent etc. like below:

![AppInsights](/wiki/images/AppInsights.png)

**Didn't find your problem here?**
Please, report the issue [here](https://github.com/OfficeDev/microsoft-teams-apps-newhireonboarding/issues/new)