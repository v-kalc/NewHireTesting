# Cost estimates

## Assumptions

The estimate below assumes:

- Tenant has 1 team containing 100 users.

## SKU recommendations
The recommended SKU for a production environment is:

- App Service: Standard (S1)
- Create up to 14 knowledge bases.

## [](/wiki/Cost-estimate#estimated-load)Estimated load

**Data storage**: up to 1 GB usage of azure table storage.

**Table data operations (monthly)**:
- Storage is called to add new project.
- 1 team member adds 3 projects per day = 90 projects per month = 90 write calls to storage
- Total number of write calls for ProjectEntity table = 100 users * 90 posts = 9000 calls.
- Storage is called to update existing project.
- 1 team member updates 1 projects per day = 30 projects per month = 30 write calls to storage
- Total number of write calls for ProjectEntity table = 100 users * 30 posts = 3000 calls.
- Storage is called to delete existing project.
- 1 team member deletes 10 projects per month = 10 delete calls to storage
- Total number of delete calls for ProjectEntity table = 100 users * 10 posts = 1000 calls.
 - Storage is called to join a project.
- 1 team member joins 20 posts per day = 600 per month = 600 write calls to storage
- Total number of write calls for ProjectEntity table = 100 users * 600 posts = 60000 calls.
- Storage is called to set up skills for users.
- 1 team updates skill sets 2 times a month = 2 per month = 2 write calls to storage
- Total number of write calls for AcquiredSkillEntity table = 1 team * 2 calls for update/insert = 2 calls.
- Storage is called to add project to created project list.
- 1 team member adds 1 project to created project list per day = 30 posts per month = 30 write calls to storage
- Total number of write calls for ProjectEntity table = 100 users * 30 posts = 3000 calls.
- Storage is called to close project .
- 1 team member closes 1 project from project list per day = 30 posts per month = 30 delete calls to storage
- Total number of delete calls for ProjectEntity table = 100 users * 30 posts = 3000 calls.
- Storage is called to retrieve user's project list.
- 1 team member views created/joined project list 10 times per day = 300 times per month = 300 read calls to storage
- Total number of read calls for ProjectEntity table = 100 users * 300 posts = 30000 calls.
- Storage is called to fetch skills of users.
- For viewing skills, existing record is fetched = 2 per month = 2 read calls to storage
- Total number of read calls for AcquiredSkillEntity table = 1 team * 2 calls = 2 calls.
- Considering all write calls mentioned previously:
- Total number of read calls for ProjectEntity table = 73000 write calls = 73000 read calls
- Total estimated read calls: 103,002
- Total estimated write calls: 79,002
- Total storage calls: 182,004

## Estimated cost

**IMPORTANT:** This is only an estimate, based on the assumptions above. Your actual costs may vary.

Prices were taken from the [Pricing](https://azure.microsoft.com/en-us/pricing/) on 23 Sept 2020, for the West US 2 region.

Use the [Azure Pricing Calculator](https://azure.com/e/02a078fd3c594aae9eb092db058b55f7) to model different service tiers and usage patterns.

|**Resource**|**Tier**|**Load**|**Monthly price**|
|--------------------------|-----------------|-------------------------|--------------------------------------
|Bot Channels Registration|F0|N/A|Free|
|App Service Plan|S1 |744 hours|$74.40|
|Application Insights (Bot)|||(free up to 5 GB)|
|Storage account (Table)| Standard_LRS|< 1GB data & 182,004 operations| $0.08 + $1.06 = $1.14 |
|Total|||**$75.54**|