# Data stores

The app uses the following data store:
All these resources are created in your Azure subscription. None are hosted directly by Microsoft.

- **Azure Storage Account**
	- [TeamConfigurationEntity] to store the teams that have the app installed.
	- [UserConfigurationEntity] to store that have the app installed, either personal scope or they are members of team.
	- [NewHireIntroductionEntity] to store data which is posted by the new hire.
	- [FeedbackEntity] to store feedback data which is posted by the new hire.

## Storage account tables
**1. TeamConfigurationEntity**
The table has following rows:

|Attribute|Comment |
|--|--|
|PartitionKey|Conversation Id of Team Channel [TeamId].|
|RowKey|Conversation Id of Team Channel [TeamId].|
|TimeStamp|Timestamp of actual record insertion (Done by Azure).|
|TeamId|Id of the team for Bot.|
|Name|Team display name.|
|ServiceUrl|Service URL for the team.|
|AadGroupId| Unique Group Id of team.|
|InstalledByAadObjectId|AadObjectId of Admin/HR who installed.|

**2. UserConfigurationEntity**
The table has following rows:

|Attribute|Comment |
|--|--|
|PartitionKey|AadObjectId of the User [AadObjectId].|
|RowKey|AadObjectId of the User [AadObjectId].|
|TimeStamp|Timestamp of actual record insertion (Done by Azure).|
|AadObjectId|AadObjectId of user who installed app.|
|ConversationId|Thread ID of the 1:1 conversation between the bot and the user.|
|BotInstalledOn|Date and time when user installed the bot.|
|UserProfileImageUrl|User profile image URL.|
|ServiceUrl|Service URL for the user.|
|UserRole|Role of the user.|
|OptedIn|Value indicating whether the user is opted in to pair-ups.|
|Name|Name of the user.|
|UserPrincipalName|Unique user principal name.|
|Email|Email Id of the user.|

**3. NewHireIntroductionEntity**

The table has following rows:

|Attribute|Comment |
|--|--|
|PartitionKey|AAD object ID of the manager [ManagerAadObjectId].|
|RowKey|AAD object ID of the new hire [NewHireAadObjectId].|
|TimeStamp|Timestamp of actual record insertion (Done by Azure).|
|ApprovalStatus|Introduction approval status shared by the Hiring Manager.|
|ApprovedOn|Date of the introduction approval.|
|ManagerAadObjectId|AAD object ID of the hiring manager.|
|ManagerConversationId|Thread ID of the 1:1 conversation between the bot and the hiring manager.|
|NewHireAadObjectId|AAD object ID of the new hire.|
|NewHireConversationId|Thread ID of the 1:1 conversation between the bot and the new hire.|
|NewHireName|Name of the new hire user.|
|NewHireProfileNote|Profile Note of the new hire user.|
|NewHireQuestionnaire|Question and answers shared by the new hire.|
|NewHireUserPrincipalName|User principal name of the new hire user.|
|SurveyNotificationSentOn|Date of survey notification sent.|
|SurveyNotificationSentStatus|status of survey notification sent.|


  **4. FeedbackEntity**
The table has following rows:

|Attribute|Comment |
|--|--|
|PartitionKey|Monthly generated id (BatchId).|
|RowKey|Unique identifier(GUID) for each feedback.|
|TimeStamp| Timestamp of actual record insertion (Done by Azure).|
|BatchId|Monthly generated ID with a combination of motnh and year.|
|Feedback| Feedback text submitted by new hire|
|Id| Unique identifier(GUID) for each feedback.|
|NewHireAadObjectId| Aad Object Id of new hire who shared a feedback.|
|NewHireName|Name of the new hire user.|
|SubmittedOn|Date of the feedback shared by new hire.|

