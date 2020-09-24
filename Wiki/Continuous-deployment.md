# Continuous deployment

Continuous Deployment service in Azure App Services offers merging latest code changes to Azure hosted environment from GitHub. It helps to seamlessly update the Azure services without need of new deployment.

**Continuous deployment in Azure App Services**

Please follow below steps to deploy latest changes to the app service:
- Log in to the Azure Portal for your subscription.
- Select App Services from left menu blade
- Search and select the app service name (search for the base resource name) which is created during first deployment. For e.g. constoso-newhireonboarding.azurewebsites.net
- Select Deployment Center under menu blade
- Click on Sync to synchronize the latest bits from GitHub master branch
note: please make sure that Repository name is pointing to correct OfficeDev repo git path.
- Wait for sync operation's success response.