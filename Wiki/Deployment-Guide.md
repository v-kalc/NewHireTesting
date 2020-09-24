

# Deployment Guide

**Prerequisites**

To begin, you will need:

-   App Service
    
-   App Service plan
    
-   Bot Channels Registration
    
-   Azure Storage account
    
-   Application Insights
    

A copy of the New Hire Onboarding app GitHub repo ([https://github.com/OfficeDev/microsoft-teams-<<To Do>>](https://github.com/OfficeDev/microsoft-teams-<<To Do>>))

### Step 1: Register Azure AD application:

Register one Azure AD applications in your tenant's directory 
1. Log in to the Azure Portal for your subscription, and go to the "App registrations" blade  [here](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps). 
2. Click on "New registration", and create an Azure AD application.

	- **Name**: The name of your Teams app - if you are following the template for a default deployment, we recommend "New hire onboarding Bot". 
	-  **Supported account types**: Select "Accounts in any organizational directory (Any Azure AD directory - Multitenant)"

![multitenant_creation](https://www.odwebp.svc.ms/wiki/images/multitenant_app_creation.png)

3. Click on the "Register" button.
4. When the app is registered, you'll be taken to the app's "Overview" page. Copy the  **Application (client) ID**; we will need it later. Verify that the "Supported account types" is set to**Multiple organizations**.

![Overview](https://www.odwebp.svc.ms/wiki/images/multitenant_app_overview.png)

1.  On the side rail in the Manage section, navigate to the "Certificates & secrets" section. In the Client secrets section, click on "+ New client secret". Add a description (Name of the secret) for the secret and select an expiry time (As per the requirement). Click "Add".

![Secret overview](https://www.odwebp.svc.ms/wiki/images/multitenant_app_secret.png)

1.  Once the client secret is created, copy it's  **Value**; we will need it later. At this point you have 3 values:
2.  Application (client) ID for the bot
3.  Client secret for the bot
4.  Directory (tenant) ID

We recommend that you copy these values into a text file, using an application like Notepad. We will need these values later.

### Step 2: Create a Security Group
1. Log in to the Azure Portal for your subscription, and go to the “Groups” blade [here](https://portal.azure.com/#blade/Microsoft_AAD_IAM/GroupsManagementMenuBlade/AllGroups).

2. Click on "New Group".

![New Security Group](https://www.odwebp.svc.ms/wiki/images/NewSecurityGroup.png)

3. Go to security group overview and copy object ID.

![Security Group Overview](https://www.odwebp.svc.ms/wiki/images/SecurityGroupOverview.png)

4. Add hiring managers to security group.

![AddMembersToSecurityGroup](https://www.odwebp.svc.ms/wiki/images/AddMembersToSecurityGroup.png)

5. We recommend that you copy security group object id into notepad.

### Step 3: Deploy to your Azure subscription

1.  Click on the "Deploy to Azure" button below.

![Deploy to Azure](https://azuredeploy.net/deploybutton.png)(<<**TO DO : Add arm template url**>>)

1.  When prompted, log in to your Azure subscription.
2.  Azure will create a "Custom deployment" based on the ARM template and ask you to fill in the template parameters.
3.  Select a subscription and resource group.
    
4.  We recommend creating a new resource group.
    
5.  The resource group location MUST be in a datacenter that supports: Application Insights; Storage Account. For an up-to-date list, click  [here](https://azure.microsoft.com/en-us/global-infrastructure/services/?products=logic-apps,cognitive-services,search,monitor), and select a region where the following services are available:
    
    -   Application Insights
    -   Storage Account
6.  Enter a "Base Resource Name", which the template uses to generate names for the other resources.
    
    -   The app service names [Base Resource Name] must be available(not taken); otherwise, the deployment will fail with a Conflict error.
    -   Remember the base resource name that you selected. We will need it later.
7.  Fill in the various IDs in the template:
    
8.  **Bot Client ID**: The application (client) ID of the Microsoft Teams Bot app.
    
9.  **Bot Client Secret**: The client secret of the Microsoft Teams Bot app.
    
10. **Tenant Id**: The tenant ID of Bot.
11. **Manifest Id** : This needs to be same as manifest Id provided in manifest.json file inside Manifest folder.
12. **Human Resource Team Link**: Human resource team URL in Microsoft Teams, to which the app will send feedback notifications. This URL starts with https://teams.microsoft.com/l/team/ .
13.  **Site Name**: SharePoint site name.
15.  **New Hire Check List Name**: SharePoint site new hire check list name.
16. **Site Tenant Name**: SharePoint site tenant name.
17. **Share Feedback Form Url**: Share feedback url from SharePoint.
18. **Complete Learning Plan Url**: Complete learning plan url from SharePoint.
19. **New Hire Question List Name**: New hire question list name from SharePoint.
20. **Security Group**: Security group Id (Required for user role).    

Make sure that the values are copied as-is, with no extra spaces. The template checks that GUIDs are exactly 36 characters.

NOTE: If you plan to use a custom domain name instead of relying on Azure Front Door, read the instructions [here](To Do) first.
1.  If you wish to change the app name, description, and icon from the defaults, modify the corresponding template parameters.
2.  Agree to the Azure terms and conditions by clicking on the check box "I agree to the terms and conditions stated above" located at the bottom of the page.
3.  Click on "Purchase" to start the deployment.
4.  Wait for the deployment to finish. You can check the progress of the deployment from the "Notifications" pane of the Azure Portal. It can take more than 10 minutes for the deployment to finish.
5.  Once the deployment has finished, you would be directed to a page that has the following fields:
6.  BotId - This is the Microsoft Application ID for the new hire onboarding Bot.
7.  AppDomain - This is the base domain for the new hire onboarding Bot.

### Step 4: Set up authentication for the app
  
  1. Go back to the "App Registrations" page [here](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps)
2. Enter the botId created in Step 1 under Owned applications search box.
3. Click on the application (this should be the same application registered in step 1)
4. Under left menu, select **Authentication** under **Manage** section.
5. Select 'Accounts in any organizational directory (Any Azure AD directory - Multitenant)' under Supported account types and click "+Add a platform".
6. On the flyout menu, Select "Web"

![Set up authentication for the app](/Wiki/Images/RedirectUrlMenu.png)

7. Add `https://[baseresourcename].azurefd.net/signin-simple-end` under Redirect URLs and select the check boxes "Access tokens" and "ID tokens" and then click "Configure" button and the bottom.

For e.g.

![Set up authentication for the app](/Wiki/Images/RedirectUrlConfiguration.png)

8. Under left menu, select **Expose an API** under **Manage**.

![Set up authentication for the app](/Wiki/Images/ExposeAnApiMenu.png)

9. Select the **Set** link to generate the Application ID URI in the form of `api://{BotID}`. Insert your app Domain (with a forward slash "/" appended to the end) between the double forward slashes and the GUID. The entire ID should have the form of: `api://app Domain/{BotID}`

- for e.g.: `api://newhireonboarding.azuredfd.net/c6c1f32b-5e55-4997-881a-753cc1d563b7`.

10. Select the **Add a scope** button. In the panel that opens, enter `access_as_user` as the **Scope name**.
11. Set Who can consent? to "Admins and users"
12. Fill in the fields for configuring the admin and user consent prompts with values that are appropriate for the `access_as_user` scope. Suggestions:
- **Admin consent display name**: New Hire Onboarding 
- **Admin consent description**: Allows Teams to call the app’s web APIs as the current user.
- **User consent display name**: Teams can access your user profile and make requests on your behalf
- **User consent description:** Enable Teams to call this app’s APIs with the same rights that you have
13. Ensure that **State** is set to **Enabled**
14. Select **Add scope**

- Note: The domain part of the **Scope name** displayed just below the text field should automatically match the **Application ID** URI set in

the previous step, with `/access_as_user` appended to the end; for example:
- `api://newhireonboarding.azurefd.net/c6c1f32b-5e55-4997-881a-753cc1d563b7/access_as_user`

15. In the same page in below section **Authorized client applications**, you identify the applications that you want to authorize to your app’s web application. Each of the following IDs needs to be entered. Click "+Add a client application" and copy-paste the below id and select checkbox "Authorized scopes". Repeat the step for second GUID.
- `1fec8e78-bce4-4aaf-ab1b-5451cc387264` (Teams mobile/desktop application)
- `5e3ce6c0-2b1f-4285-8d4b-75ee78787346` (Teams web application)

16. Under left menu, navigate to **API Permissions**, and make sure to add the follow permissions of Microsoft Graph API :

**Delegated Permissions**:
- User.ReadBasic.All,
- User.Read.All,
- Directory.Read.All, 
- Directory.AccessAsUser.All,
- Team.ReadBasic.All, 
- TeamSettings.Read.All,
- Group.Read.All, 
- GroupMember.Read.All,
- Group.Read.All,
- openid,
- profile,
- User.ManageIdentities.All

**Application Permission**: 
- User.Read.All,
- Sites.Read.All

**Note:** The detailed guidelines for registering an application for SSO Microsoft Teams tab can be found [here](https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/authentication/auth-aad-sso)

### Step 5: Set up authentication for bot

1.  Note the name of the bot that you deployed, which is [BaseResourceName].
    
2.  Go to azure portal  [here](https://portal.azure.com/)  and search for your bot.
    
3.  Click on the bot in the application list. Under "Settings", click on "Add Setting".
    
4.  Fill in the form as follows:
    
    a. For Name, enter "**NewHireOnboardingAuth**". You'll use it in your bot code.
    
    b. For Service Provider, select Azure Active Directory. Once you select this, the Azure AD-specific fields will be displayed.
    
    c. For Client id, enter the application (client) ID that you recorded earlier.
    
    d. For Client secret, enter the secret that you created to grant the bot access to the Azure AD app.
    
    e. For Tenant ID, enter the directory (tenant) ID that your recorded earlier for your AAD app. This will be the tenant associated with the users who can be authenticated.
    
    f. For Grant type , enter "authorization_code".
    
    g. For Login URL , enter "https://login.microsoftonline.com".
    
    h. For Resource URL , enter "https://graph.microsoft.com/".
    
    i. For Scopes, enter the names of the permissions you choose from application registration. Enter space separated values: User.Read AllSites.Read
    
5.  Click Save.
    

### Step 6: Create the Teams app packages

This step covers the Teams application package creation for teams scope and make it ready to install in Teams.
1. Open the `Manifest\manifest.json` file in a text editor.
2. Change the placeholder fields in the manifest to values appropriate for your organization.
* `developer.name` ([What's this?](https://docs.microsoft.com/en-us/microsoftteams/platform/resources/schema/manifest-schema#developer))
* `developer.websiteUrl`
* `developer.privacyUrl`
* `developer.termsOfUseUrl`
3. Change the `<<botId>>` placeholder to your Azure AD application's ID from above. This is the same GUID that you entered in the template under "Bot Client ID".
4. In the "validDomains" section, replace the `<<appDomain>>` with your Bot App Service's domain. This will be `[BaseResourceName].azurefd.net`. For example if you chose "contosonewhireonboarding" as the base name, change the placeholder to `contosonewhireonboarding.azurefd.net`.
* note : please make sure to not add https:// in valid domains. Also make sure to add SharePoint domain in "validDomains" section.
5. In the "webApplicationInfo" section, replace the `<<botId>>` with Bot Client ID of the app created in Step 1. Also replace `api://<<applicationurl>>/<<botId>>` with following Application URI appended with bot client id. This will be as follows for example `api://contoso-newhireonboarding.azurefd.net/19c1102a-fffe-46c4-9a85-016bec13e0ab` where contoso-newhireonboarding is the base resource URL used under valid domains and configurable tabs and 19c1102a-fffe-46c4-9a85-016bec13e0ab is the bot client id.
6. Create a ZIP package with the `manifest.json`,`color.png`, and `outline.png`. The two image files are the icons for your app in Teams.
7. Make sure that the 3 files are the _top level_ of the ZIP package, with no nested folders.

![Create the Teams app packages](/Wiki/Images/ManifestUI.PNG)

## Step 7: Run the apps in Microsoft Teams

 1. If your tenant has side loading apps enabled, you can install your app by following the instructions
[here](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/apps/apps-upload#load-your-package-into-teams)

2. You can also upload it to your tenant's app catalog, so that it can be available for everyone in your tenant to install. See [here](https://docs.microsoft.com/en-us/microsoftteams/tenant-apps-catalog-teams)

* We recommend using [app permission policies](https://docs.microsoft.com/en-us/microsoftteams/teams-app-permission-policies) to restrict access to this app to the members of the experts team.
  
1. Install the app (the `NewHireOnboarding.zip` package) to your team.

### Troubleshooting
Please see our [Troubleshooting](Troubleshooting) page.