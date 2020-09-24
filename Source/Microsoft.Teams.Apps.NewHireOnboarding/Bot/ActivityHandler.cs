// <copyright file="ActivityHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Bot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.NewHireOnboarding.Cards;
    using Microsoft.Teams.Apps.NewHireOnboarding.Interfaces;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The ActivityHandler is responsible for reacting to incoming events from Teams sent from BotFramework.
    /// </summary>
    /// <typeparam name="T">Generic class.</typeparam>
    public sealed class ActivityHandler<T> : TeamsActivityHandler
        where T : Dialog
    {
        /// <summary>
        /// State management object for maintaining user conversation state.
        /// </summary>
        private readonly BotState userState;

        /// <summary>
        /// State management object for maintaining conversation state.
        /// </summary>
        private readonly BotState conversationState;

        /// <summary>
        /// A set of key/value application configuration properties for bot settings.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Instance to send logs to the logger service.
        /// </summary>
        private readonly ILogger<ActivityHandler> logger;

        /// <summary>
        /// Bot adapter used to handle bot framework HTTP requests.
        /// </summary>
        private readonly IBotFrameworkHttpAdapter adapter;

        /// <summary>
        /// The current culture's string localizer.
        /// </summary>
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// Instance of telemetry client.
        /// </summary>
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Provider for fetching information about team details from storage.
        /// </summary>
        private readonly ITeamStorageProvider teamStorageProvider;

        /// <summary>
        /// Provider for fetching information about user details from storage.
        /// </summary>
        private readonly IUserStorageProvider userStorageProvider;

        /// <summary>
        /// Provider for fetching information about new hire introduction details from storage.
        /// </summary>
        private readonly IIntroductionStorageProvider introductionStorageProvider;

        /// <summary>
        /// Instance of learning helper to get learning plan methods.
        /// </summary>
        private readonly ILearningPlanHelper learningPlanHelper;

        /// <summary>
        /// Helper for working with user token.
        /// </summary>
        private readonly ITokenHelper tokenHelper;

        /// <summary>
        /// Base class for all bot dialogs.
        /// </summary>
        private readonly Dialog dialog;

        /// <summary>
        /// Helper for working with bot activity handler.
        /// </summary>
        private readonly IActivityHelper activityHelper;

        /// <summary>
        /// Helper for working with cards.
        /// </summary>
        private readonly ICardHelper cardHelper;

        /// <summary>
        /// A set of key/value application configuration properties for storage.
        /// </summary>
        private readonly IOptions<StorageSettings> storageOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityHandler{T}"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <param name="telemetryClient">Instance of telemetry client. </param>
        /// <param name="userState">State management object for maintaining user conversation state.</param>
        /// <param name="adapter">Bot adapter used to handle bot framework HTTP requests.</param>
        /// <param name="conversationState">State management object for maintaining conversation state.</param>
        /// <param name="botOptions">A set of key/value application configuration properties for bot.</param>
        /// <param name="storageOptions">A set of key/value application configuration properties for storage.</param>
        /// <param name="teamStorageProvider">Provider for fetching information about team details from storage.</param>
        /// <param name="userStorageProvider">Provider for fetching information about user details from storage.</param>
        /// <param name="learningPlanHelper">Instance of learning plan helper.</param>
        /// <param name="introductionStorageProvider">Provider for fetching information about new hire introduction details from storage.</param>
        /// <param name="tokenHelper">Helper for JWT token generation and validation.</param>
        /// <param name="activityHelper">Helper for working with bot activity handler.</param>
        /// <param name="cardHelper">Helper for working with cards.</param>
        /// <param name="dialog">Base class for all bot dialogs.</param>
        public ActivityHandler(
            ILogger<ActivityHandler> logger,
            IStringLocalizer<Strings> localizer,
            TelemetryClient telemetryClient,
            UserState userState,
            IBotFrameworkHttpAdapter adapter,
            ConversationState conversationState,
            IOptions<BotSettings> botOptions,
            IOptions<StorageSettings> storageOptions,
            ITeamStorageProvider teamStorageProvider,
            IUserStorageProvider userStorageProvider,
            ILearningPlanHelper learningPlanHelper,
            IIntroductionStorageProvider introductionStorageProvider,
            ITokenHelper tokenHelper,
            IActivityHelper activityHelper,
            ICardHelper cardHelper,
            T dialog)
        {
            this.logger = logger;
            this.localizer = localizer;
            this.telemetryClient = telemetryClient;
            this.userState = userState;
            this.adapter = adapter;
            this.conversationState = conversationState;
            this.botOptions = botOptions ?? throw new ArgumentNullException(nameof(botOptions));
            this.storageOptions = storageOptions ?? throw new ArgumentNullException(nameof(storageOptions));
            this.teamStorageProvider = teamStorageProvider;
            this.userStorageProvider = userStorageProvider;
            this.learningPlanHelper = learningPlanHelper;
            this.introductionStorageProvider = introductionStorageProvider;
            this.tokenHelper = tokenHelper;
            this.activityHelper = activityHelper;
            this.cardHelper = cardHelper;
            this.dialog = dialog;
        }

        /// <summary>
        /// Handles an incoming activity.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// Reference link: https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.activityhandler.onturnasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            this.RecordEvent(nameof(this.OnTurnAsync), turnContext);

            await base.OnTurnAsync(turnContext, cancellationToken);

            await this.conversationState.SaveChangesAsync(turnContext: turnContext, force: false, cancellationToken: cancellationToken);
            await this.userState.SaveChangesAsync(turnContext: turnContext, force: false, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Invoked when task module fetch event is received from the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="taskModuleRequest">Task module invoke request value payload.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(
            ITurnContext<IInvokeActivity> turnContext,
            TaskModuleRequest taskModuleRequest,
            CancellationToken cancellationToken)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            taskModuleRequest = taskModuleRequest ?? throw new ArgumentNullException(nameof(taskModuleRequest));
            this.RecordEvent(nameof(this.OnTeamsTaskModuleFetchAsync), turnContext);

            var activity = turnContext.Activity;
            if (taskModuleRequest.Data == null)
            {
                this.telemetryClient.TrackTrace("Request data obtained on task module fetch action is null.");
                await turnContext.SendActivityAsync(this.localizer.GetString("WelcomeCardContent"));

                return null;
            }

            var postedValues = JsonConvert.DeserializeObject<AdaptiveSubmitActionData>(taskModuleRequest.Data.ToString());
            var command = postedValues.Command;

            var userGraphAccessToken = await this.tokenHelper.GetUserTokenAsync(activity.From.Id, Constants.GraphAPIBaseURL);
            if (userGraphAccessToken == null)
            {
                await this.dialog.RunAsync(turnContext, this.conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);

                return null;
            }
            else
            {
                switch (command.ToUpperInvariant())
                {
                    // Fetch task module to show introduction card.
                    case Constants.IntroductionAction:
                        return await this.activityHelper.GetIntroductionCardAsync(userGraphAccessToken, turnContext, cancellationToken);

                    // Fetch task module to show approve introduction card.
                    case Constants.ApproveIntroductionAction:
                        return await this.activityHelper.ApproveIntroductionActionAsync(userGraphAccessToken, turnContext);

                    default:
                        this.logger.LogInformation($"Invalid command for task module fetch activity.Command is : {command} ");
                        await turnContext.SendActivityAsync(this.localizer.GetString("UnsupportedBotPersonalCommandText"));

                        return null;
                }
            }
        }

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A Task resolving to either a login card or the adaptive card of the Reddit post.</returns>
        /// <remarks>
        /// For more information on bot messaging in Teams, see the documentation
        /// https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/conversation-basics?tabs=dotnet#receive-a-message .
        /// </remarks>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));

            this.RecordEvent(nameof(this.OnMessageActivityAsync), turnContext);
            var activity = turnContext.Activity;
            var command = activity.Text.ToUpperInvariant().Trim();
            await this.SendTypingIndicatorAsync(turnContext);

            var userGraphAccessToken = await this.tokenHelper.GetUserTokenAsync(activity.From.Id, Constants.GraphAPIBaseURL);
            if (userGraphAccessToken == null)
            {
                await this.dialog.RunAsync(turnContext, this.conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);

                return;
            }

            var valuesfromCard = activity.Value != null ? ((JObject)activity.Value).ToObject<AdaptiveSubmitActionData>() : null;
            if (activity.Conversation.ConversationType == Constants.PersonalConversationType)
            {
                // Command to send feedback card.
                if (command == this.localizer.GetString("ShareFeedbackText").ToString().ToUpperInvariant())
                {
                    var shareFeedbackCardActivity = MessageFactory.Attachment(FeedbackCard.GetFeedbackCardAttachment(this.localizer));
                    await turnContext.SendActivityAsync(shareFeedbackCardActivity, cancellationToken);

                    return;
                }

                // Command to save feedback.
                else if (command == this.localizer.GetString("SubmitFeedbackText").ToString().ToUpperInvariant())
                {
                    await this.activityHelper.SubmitFeedbackAsync(turnContext);

                    return;
                }

                // Command to send on-boarding checklist card.
                else if (command == this.localizer.GetString("OnBoardingCheckListText").ToString().ToUpperInvariant())
                {
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(OnBoardingCheckListCard.GetCard(
                            this.localizer,
                            this.botOptions.Value.ManifestId)));

                    return;
                }

                // Bot sign-out command.
                else if (command == this.localizer.GetString("LogoutText").ToString().ToUpperInvariant())
                {
                    await this.dialog.RunAsync(turnContext, this.conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);

                    return;
                }

                // Command to send more info card to new hire employee.
                else if (command == this.localizer.GetString("RequestMoreInfoText").ToString().ToUpperInvariant())
                {
                    await this.activityHelper.RequestMoreInfoActionAsync(turnContext, valuesfromCard, cancellationToken);

                    return;
                }

                // Command to send user tour based on his role.
                else if (command == this.localizer.GetString("HelpText").ToString().ToUpperInvariant())
                {
                    var userDetail = await this.userStorageProvider.GetUserDetailAsync(activity.From.AadObjectId);
                    bool isManager = userDetail?.UserRole == (int)UserRole.HiringManager;

                    // Send help cards based on their role.
                    await turnContext.SendActivityAsync(MessageFactory.Carousel(CarouselCard.GetUserHelpCards(
                        this.botOptions.Value.AppBaseUri,
                        this.localizer,
                        this.botOptions.Value.ManifestId,
                        isManager)));

                    return;
                }

                // Command to send pending review introduction list card.
                else if (command == this.localizer.GetString("ReviewIntroductionText").ToString().ToUpperInvariant())
                {
                    var user = await this.userStorageProvider.GetUserDetailAsync(activity.From.AadObjectId);

                    if (user != null && user.UserRole != (int)UserRole.HiringManager)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(HelpCard.GetCard(this.localizer)));

                        return;
                    }

                    var introductionEntities = await this.introductionStorageProvider.GetFilteredIntroductionsAsync(activity.From.AadObjectId);
                    if (!introductionEntities.Any())
                    {
                        await turnContext.SendActivityAsync(this.localizer.GetString("NoPendingIntroductionText"));
                        return;
                    }

                    var listCardAttachment = await this.cardHelper.GetReviewIntroductionListCardAsync(introductionEntities, userGraphAccessToken);
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(listCardAttachment));

                    return;
                }

                // Command to send week wise learning plan cards.
                else if (command == this.localizer.GetString("ViewLearningText").ToString().ToUpperInvariant())
                {
                    // Complete learning plan for weeks.
                    var completeLearningPlan = await this.learningPlanHelper.GetCompleteLearningPlansAsync();

                    if (completeLearningPlan == null || !completeLearningPlan.Any())
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text(this.localizer.GetString("CompleteLearningPlanNotExistText")), cancellationToken);
                    }
                    else
                    {
                        // Send complete learning list cards as per weeks.
                        await this.learningPlanHelper.SendCompleteLearningListCardsAsync(turnContext, completeLearningPlan);
                    }

                    return;
                }

                // Command to resume all matches
                else if (command == Constants.ResumeAllMatches.ToUpperInvariant())
                {
                    var userAadId = turnContext.Activity.From.AadObjectId;
                    this.logger.LogInformation($"Updating user choice for resuming matches in storage for userId: {userAadId}");
                    var userEntity = await this.userStorageProvider.GetUserDetailAsync(userAadId);
                    userEntity.OptedIn = true;
                    var operationResult = await this.userStorageProvider.StoreOrUpdateUserDetailAsync(userEntity);

                    if (!operationResult)
                    {
                        await turnContext.SendActivityAsync(this.localizer.GetString("ErrorInUpdatingUserChoice"), cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(this.localizer.GetString("PausedMatchesCardContent"), cancellationToken: cancellationToken);
                    }
                }

                // Command to pause all matches
                else if (command == Constants.PauseAllMatches.ToUpperInvariant())
                {
                    var userId = turnContext.Activity.From.AadObjectId;
                    this.logger.LogInformation($"Sending resume all matches card and updating user choice for pausing matches in storage for userId: {userId}");
                    var opteduserDetail = await this.userStorageProvider.GetUserDetailAsync(userId);
                    if (opteduserDetail != null)
                    {
                        opteduserDetail.OptedIn = false;
                        var operationStatus = await this.userStorageProvider.StoreOrUpdateUserDetailAsync(opteduserDetail);
                        if (operationStatus)
                        {
                            var resumeMatchesCardAttachment = MessageFactory.Attachment(ResumeMatchesCard.GetResumeMatchesCard(this.localizer));
                            await turnContext.SendActivityAsync(resumeMatchesCardAttachment, cancellationToken);
                        }
                        else
                        {
                            await turnContext.SendActivityAsync(this.localizer.GetString("ErrorInUpdatingUserChoice"), cancellationToken: cancellationToken);
                        }
                    }
                }
                else
                {
                    // If message is from complete learning plan list item tap event.
                    if (command.Contains(this.localizer.GetString("ViewWeeklyLearningPlanCommandText"), StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Get learning plan card selected from complete learning plan list card.
                        var learningCard = await this.learningPlanHelper.GetLearningPlanCardAsync(command);

                        // Send learning plan data card.
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(learningCard));
                    }
                    else if (command.Contains(this.localizer.GetString("ReviewIntroductionCommandText"), StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Get all Introductions for given Azure Active directory id.
                        var result = await this.introductionStorageProvider.GetAllIntroductionsAsync(activity.From.AadObjectId);
                        var introductionEntity = result.Where(entity => entity.NewHireName.ToUpperInvariant() == command.Split(":")[1].ToUpperInvariant()).FirstOrDefault();
                        if (introductionEntity != null && (introductionEntity.ApprovalStatus == (int)IntroductionStatus.Approved))
                        {
                            // Send already approved message to hiring manager.
                            await turnContext.SendActivityAsync(this.localizer.GetString("ManagerApprovalValidationText"));
                        }
                        else
                        {
                            await turnContext.SendActivityAsync(MessageFactory.Attachment(HiringManagerNotificationCard.GetNewEmployeeIntroductionCard(this.botOptions.Value.AppBaseUri, this.localizer, introductionEntity)));
                        }
                    }
                    else
                    {
                        // Send help card for un supported bot command.
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(HelpCard.GetCard(this.localizer)));
                    }

                    return;
                }
            }
            else
            {
                await turnContext.SendActivityAsync(this.localizer.GetString("UnSupportedBotCommand"));
            }
        }

        /// <summary>
        /// Method that checks teams signin verify state, check if token exists.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            // Run the Dialog with the new Teams Signin Verify State  Activity.
            await this.dialog.RunAsync(turnContext, this.conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        /// <summary>
        /// Invoked when task module submit event is received from the bot.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <param name="taskModuleRequest">Task module invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents a task module response.</returns>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            taskModuleRequest = taskModuleRequest ?? throw new ArgumentNullException(nameof(taskModuleRequest));

            this.RecordEvent(nameof(this.OnTeamsTaskModuleSubmitAsync), turnContext);
            var activity = turnContext.Activity;

            if (turnContext == null || taskModuleRequest == null)
            {
                this.logger.LogInformation("Request data obtained on task module submit action is null.");
                await turnContext.SendActivityAsync(this.localizer.GetString("ErrorMessage"));

                return null;
            }

            var valuesFromTaskModule = JsonConvert.DeserializeObject<TaskModuleDetail>(taskModuleRequest.Data?.ToString());
            if (valuesFromTaskModule == null)
            {
                this.logger.LogInformation("Request data obtained on task module submit action is null.");
                await turnContext.SendActivityAsync(this.localizer.GetString("ErrorMessage"));

                return null;
            }

            var userGraphAccessToken = await this.tokenHelper.GetUserTokenAsync(activity.From.Id, Constants.GraphAPIBaseURL);

            if (userGraphAccessToken == null)
            {
                await this.dialog.RunAsync(turnContext, this.conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);

                return default;
            }
            else
            {
                var command = valuesFromTaskModule.Command;
                switch (command.ToUpperInvariant().Trim())
                {
                    // Command to post team notification card.
                    case Constants.PostTeamNotificationAction:

                        if (valuesFromTaskModule.TeamId == null)
                        {
                            List<Models.TeamDetail> teamChannelMapping = await this.activityHelper.GetTeamMappingDetailsAsync(turnContext, userGraphAccessToken);
                            return this.cardHelper.GetApproveDetailCard(teamChannelMapping, valuesFromTaskModule.IntroductionEntity, false);
                        }

                        valuesFromTaskModule.IntroductionEntity.ApprovalStatus = (int)IntroductionStatus.Approved;
                        valuesFromTaskModule.IntroductionEntity.ApprovedOn = DateTime.UtcNow;
                        bool isIntroductionApproved = await this.introductionStorageProvider.StoreOrUpdateIntroductionDetailAsync(valuesFromTaskModule.IntroductionEntity);

                        if (isIntroductionApproved)
                        {
                            // get user profile image url from user storage.
                            var userDetails = await this.userStorageProvider.GetUserDetailAsync(valuesFromTaskModule.IntroductionEntity.NewHireAadObjectId);
                            if (userDetails != null)
                            {
                                valuesFromTaskModule.IntroductionEntity.UserProfileImageUrl = userDetails.UserProfileImageUrl;
                            }

                            // Send notification to selected teams; Splitting team id and Channel id by ; (semicolon)
                            var teamNotificationAttachment = TeamIntroductionCard.GetCard(
                                this.botOptions.Value.AppBaseUri,
                                this.localizer,
                                valuesFromTaskModule.IntroductionEntity);

                            await this.PostCardToTeamAsync(turnContext, teamNotificationAttachment, valuesFromTaskModule.TeamId.Split(";")[1], cancellationToken);

                            await turnContext.SendActivityAsync(this.localizer.GetString("SuccessfulPostedMessage"));
                        }

                        break;

                    // Command to submit new hire introduction.
                    case Constants.SubmitIntroductionAction:
                        return await this.activityHelper.SubmitIntroductionActionAsync(userGraphAccessToken, turnContext, taskModuleRequest, cancellationToken);

                    default:
                        this.logger.LogInformation($"Invalid command for task module submit activity.Command is : {command} ");

                        break;
                }

                return null;
            }
        }

        /// <summary>
        /// Overriding to send welcome card once Bot is installed in personal/team.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation, as described by the conversation update activity.</param>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Welcome card when bot is added first time by user.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));

            var activity = turnContext.Activity;
            this.logger.LogInformation($"conversationType: {activity.Conversation?.ConversationType}, membersAdded: {membersAdded?.Count}");
            await this.activityHelper.SendWelcomeNotificationAsync(membersAdded, turnContext, cancellationToken);
        }

        /// <summary>
        /// Overriding to delete team details when uninstalled the bot.
        /// </summary>
        /// <param name="membersRemoved">A member removed from team, as described by the conversation update activity.</param>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMembersRemovedAsync(IList<ChannelAccount> membersRemoved, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            var activity = turnContext.Activity;
            this.logger.LogInformation($"conversationType: {activity.Conversation?.ConversationType}, membersRemoved: {membersRemoved?.Count}");

            // Deleting team information from storage when bot is uninstalled from a team.
            if (membersRemoved.Any(member => member.Id == activity.Recipient.Id) && activity.Conversation.ConversationType == Constants.ChannelConversationType)
            {
                this.logger.LogInformation($"Bot removed {activity.Conversation.Id}");
                var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
                var teamEntity = await this.teamStorageProvider.GetTeamDetailAsync(teamsChannelData.Team.Id);

                if (teamEntity == null)
                {
                    this.logger.LogWarning($"No team is found for team id {teamsChannelData.Team.Id} to delete team details");
                    return;
                }

                bool operationStatus = await this.teamStorageProvider.DeleteTeamDetailAsync(teamEntity);

                if (!operationStatus)
                {
                    this.logger.LogError("Unable to remove team details from table storage.");
                }
            }
        }

        /// <summary>
        /// Records event data to Application Insights telemetry client
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        private void RecordEvent(string eventName, ITurnContext turnContext)
        {
            var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            this.telemetryClient.TrackEvent(eventName, new Dictionary<string, string>
            {
                { "userId", turnContext.Activity.From.AadObjectId },
                { "tenantId", turnContext.Activity.Conversation.TenantId },
                { "teamId", teamsChannelData?.Team?.Id },
                { "channelId", teamsChannelData?.Channel?.Id },
            });
        }

        /// <summary>
        /// Send typing indicator to the user.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <returns>A task that represents typing indicator activity.</returns>
        private async Task SendTypingIndicatorAsync(ITurnContext turnContext)
        {
            var typingActivity = turnContext.Activity.CreateReply();
            typingActivity.Type = ActivityTypes.Typing;
            await turnContext.SendActivityAsync(typingActivity);
            this.logger.LogInformation("Sent a typing indicator.");
        }

        /// <summary>
        /// Post the given attachment to the specified team.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cardToPost">Notification card to Post.</param>
        /// <param name="teamId">Team id to which the message is being sent.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns><see cref="Task"/>That resolves to a <see cref="ConversationResourceResponse"/>Send a attachment.</returns>
        private async Task<ResourceResponse> PostCardToTeamAsync(
            ITurnContext turnContext,
            Attachment cardToPost,
            string teamId,
            CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);
            ResourceResponse response = null;
            var conversationReference = new ConversationReference()
            {
                ChannelId = Constants.TeamsBotFrameworkChannelId,
                Bot = new ChannelAccount() { Id = $"28:{this.botOptions.Value.MicrosoftAppId}" },
                ServiceUrl = activity.ServiceUrl,
                Conversation = new ConversationAccount() { Id = teamId },
            };

            this.logger.LogInformation($"Sending notification to team TeamID: {teamId}");

            await ((BotFrameworkAdapter)this.adapter).ContinueConversationAsync(
                this.botOptions.Value.MicrosoftAppId,
                conversationReference,
                async (conversationTurnContext, conversationCancellationToken) =>
                {
                    response = await conversationTurnContext.SendActivityAsync(MessageFactory.Attachment(cardToPost));
                },
                cancellationToken);

            return response;
        }
    }
}