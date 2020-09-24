# Solution overview

New Hire Onboarding app will simplify and ease the process of onboarding the new hires into the organization. This app will prompt new hires to complete some immediate activities when they join, allow them to experience predefined Onboarding Journey of activities and trainings ,nudging them along to complete onboarding at a proper pace.

![Solution Overview](/wiki/images/ArchitectureDiagram.png)

**NEO Bot:**
- This is a web application built using the [Bot Framework SDK v4 for .NET](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0) and [ASP.NET Core 3.1](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-3.1). NEO is a bot that will ease the process of onboarding the new hires into the organization. It also implements an Onboarding tab to show the training list week wise.

**Azure solution:**
- The app service implements the bot and tab experience by providing end points for user communication. App service hosts React application for tab which loads complete learning plan for new hire to view, using Graph APIs to read SharePoint list configured by HR.
- App endpoint is registered as messaging end point in bot registration portal and provides an endpoint /api/messages to process bot and tab requests/response.
- The app leverage Microsoft Graph APIs for accessing data from SharePoint lists used for all activities of NEO App. Also, we will be caching SharePoint data in the application cache.
- This App implements different scheduled Background services to send proactive notification related to learning plan, feedback & survey. Also, Icebreaker functionality is implemented using Background services which randomly does pairing up of new connections outside their teams.
- Single Sign On experience is implemented in React application for seamless user authentication.

**Azure bot service:**
- Azure bot service is developed using BOT SDK v4 and .NET Core 3.1.NEO web app endpoint is registered as messaging end point in bot registration portal.
- AAD v2 service provider is used for authentication and registered in OAuth connection string settings in bot registration portal.  


**Azure table storage:**
- Azure table storage is used to store user configuration, New hire Introduction, feedback etc. Details are provided in section  [Data stores]().

**Microsoft Graph APIs:**
- Application data is accessed from SharePoint online which requires application level permission and requires tenant admin to provide consent. 

**PairUpNotification BackgroundService:**
- Weekly scheduled background service which loops through the users and randomly does pairing up of members every week to meet for coffee or propose a meeting call.

**LearningPlanNotification BackgroundService:**
- Weekly scheduled background service which loops through the users and sends an adaptive card with the training to be done for that weeek.

**SurveyNotification BackgroundService:**
- The survey background service will look up in the Azure table storage for the user and send adaptive card with the link to the survey will be sent to the user after 2 weeks of his Indroduction approved date.

**Feedback Background service:**
- The feedback background service will look up in the Azure table storage for the Feedback entries for that current month batch and send notification to pre-configured HR teams' channel.


**Application Insights:**
- Application insights is used for tracking and logging. Details are provided in section [Telemetry](/wiki/Telemetry.md).

**Data stores:**
- The web app is using Azure table storage for data storage due to its cost-effective pricing model and providing support for No-SQL data models.