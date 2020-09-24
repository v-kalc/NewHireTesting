// <copyright file="NotificationHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Helpers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.NewHireOnboarding.Cards;
    using Microsoft.Teams.Apps.NewHireOnboarding.Interfaces;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.Configuration;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.EntityModels;
    using Polly;
    using Polly.Contrib.WaitAndRetry;
    using Polly.Retry;

    /// <summary>
    /// A class that contains helper methods for sending survey notifications.
    /// </summary>
    public class NotificationHelper : INotificationHelper
    {
        /// <summary>
        /// Sets the batch size of different new hire users.
        /// </summary>
        private const int SendSurveyNotificationBatchLimit = 5;

        /// <summary>
        /// Provider for fetching information about new hire introduction details from storage.
        /// </summary>
        private readonly IIntroductionStorageProvider introductionStorageProvider;

        /// <summary>
        /// Provider for fetching information about user details from storage.
        /// </summary>
        private readonly IUserStorageProvider userStorageProvider;

        /// <summary>
        /// A set of key/value application configuration properties for bot settings.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Instance to send logs to the logger service.
        /// </summary>
        private readonly ILogger<NotificationHelper> logger;

        /// <summary>
        /// Bot adapter.
        /// </summary>
        private readonly IBotFrameworkHttpAdapter adapter;

        /// <summary>
        /// The current cultures' string localizer.
        /// </summary>
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// A set of key/value application configuration properties of SharePoint.
        /// </summary>
        private readonly IOptions<SharePointSettings> sharePointOptions;

        /// <summary>
        /// Provider for fetching information about team details from storage.
        /// </summary>
        private readonly ITeamStorageProvider teamStorageProvider;

        /// <summary>
        /// Retry policy with jitter, retry twice with a jitter delay of up to 1 sec. Retry for HTTP 429(transient error)/502 bad gateway.
        /// </summary>
        /// <remarks>
        /// Reference: https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#new-jitter-recommendation.
        /// </remarks>
        private readonly AsyncRetryPolicy retryPolicy = Policy.Handle<ErrorResponseException>(
            ex => ex.Response.StatusCode == HttpStatusCode.TooManyRequests || ex.Response.StatusCode == HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(1000), 2));

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationHelper"/> class.
        /// </summary>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <param name="botOptions">A set of key/value application configuration properties for bot.</param>
        /// <param name="adapter">Bot adapter.</param>
        /// <param name="storageProvider">Storage provider for Introduction storage.</param>
        /// <param name="userStorageProvider">User Storage provider for Introduction storage.</param>
        /// <param name="sharePointOptions">A set of key/value pair configuration properties for SharePoint.</param>
        /// <param name="teamStorageProvider">Provider for fetching information about team details from storage.</param>
        public NotificationHelper(
            IIntroductionStorageProvider storageProvider,
            IUserStorageProvider userStorageProvider,
            ILogger<NotificationHelper> logger,
            IStringLocalizer<Strings> localizer,
            IBotFrameworkHttpAdapter adapter,
            IOptions<BotSettings> botOptions,
            IOptions<SharePointSettings> sharePointOptions,
            ITeamStorageProvider teamStorageProvider)
        {
            this.introductionStorageProvider = storageProvider;
            this.userStorageProvider = userStorageProvider;
            this.logger = logger;
            this.localizer = localizer;
            this.adapter = adapter;
            this.botOptions = botOptions ?? throw new ArgumentNullException(nameof(botOptions));
            this.sharePointOptions = sharePointOptions ?? throw new ArgumentNullException(nameof(sharePointOptions));
            this.teamStorageProvider = teamStorageProvider;
        }

        /// <summary>
        /// Send survey notification to new hire on Weekly basis in a batch.
        /// </summary>
        /// <returns>A task that sends survey notification to new hire.</returns>
        public async Task SendSurveyNotificationToNewHireAsync()
        {
            this.logger.LogInformation($"Send notification Timer trigger function executed at: {DateTime.UtcNow}");
            var introductionEntities = await this.introductionStorageProvider.GetAllPendingSurveyIntroductionAsync();

            if (introductionEntities != null && introductionEntities.Any())
            {
                var notificationCard = NotificationSurveyCard.GetSurveyNotificationCard(
                               this.botOptions.Value.AppBaseUri,
                               this.localizer,
                               this.sharePointOptions.Value.ShareFeedbackFormUrl);

                var batchCount = (int)Math.Ceiling((double)introductionEntities.Count() / SendSurveyNotificationBatchLimit);
                for (int batchIndex = 0; batchIndex < batchCount; batchIndex++)
                {
                    var introductionEntitiesBatch = introductionEntities
                        .Skip(batchIndex * SendSurveyNotificationBatchLimit)
                        .Take(SendSurveyNotificationBatchLimit);

                    foreach (var introductionEntity in introductionEntitiesBatch)
                    {
                        try
                        {
                            await this.SendCardToUserAsync(introductionEntity, notificationCard);
                            introductionEntity.SurveyNotificationSentStatus = (int)SurveyNotificationStatus.Sent;
                            introductionEntity.SurveyNotificationSentOn = DateTimeOffset.UtcNow.DateTime;
                            await this.introductionStorageProvider.StoreOrUpdateIntroductionDetailAsync(introductionEntity);
                        }
#pragma warning disable CA1031 // Catching general exception for any errors occurred during send survey notification card to user.
                        catch (Exception ex)
#pragma warning disable CA1031 // Catching general exception for any errors occurred during send survey notification card to user.
                        {
                            this.logger.LogError(ex, $"Error while performing retry logic to send survey notification to user: {introductionEntity.NewHireAadObjectId}.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Send feedback notification to hiring manager in team on Monthly basis as per the configuration.
        /// </summary>
        /// <returns>A task that sends feedback notification to hiring manager.</returns>
        public async Task SendFeedbackNotificationInChannelAsync()
        {
            this.logger.LogInformation($"Send notification Timer trigger function executed at: {DateTime.UtcNow}");
            string teamsChannelId = this.botOptions.Value.HumanResourceTeamId;
            var teamEntity = await this.teamStorageProvider.GetTeamDetailAsync(teamsChannelId);

            if (teamEntity != null)
            {
                var notificationCard = ViewFeedbackCard.GetFeedbackCard(
                               this.botOptions.Value.AppBaseUri,
                               this.localizer);

                try
                {
                    await this.SendCardToTeamAsync(notificationCard, teamEntity);
                }
#pragma warning disable CA1031 // Catching general exception for any errors occurred during send survey notification card to user.
                catch (Exception ex)
#pragma warning disable CA1031 // Catching general exception for any errors occurred during send survey notification card to user.
                {
                    this.logger.LogError(ex, $"Error while performing retry logic to send survey notification to user: {teamEntity.TeamId}.");
                }
            }
        }

        /// <summary>
        /// Send notification card to new hire as per the configured preference in different users.
        /// <param name="introductionEntity"> New hire introduction entity value.</param>
        /// <param name="surveyNotificationCard">Survey notification card attachment.</param>
        /// </summary>
        private async Task SendCardToUserAsync(IntroductionEntity introductionEntity, Attachment surveyNotificationCard)
        {
            if (introductionEntity != null)
            {
                var userConversationDetails = await this.userStorageProvider.GetUserDetailAsync(introductionEntity.NewHireAadObjectId);
                var conversationReference = new ConversationReference()
                {
                    Bot = new ChannelAccount() { Id = $"28:{this.botOptions.Value.MicrosoftAppId}" },
                    Conversation = new ConversationAccount() { Id = introductionEntity.NewHireConversationId },
                    ServiceUrl = userConversationDetails.ServiceUrl,
                };
                this.logger.LogInformation($"sending notification to conversationId- {introductionEntity.NewHireConversationId}");

                // Retry it in addition to the original call.
                await this.retryPolicy.ExecuteAsync(async () =>
                {
                    try
                    {
                        await ((BotFrameworkAdapter)this.adapter).ContinueConversationAsync(
                            this.botOptions.Value.MicrosoftAppId,
                            conversationReference,
                            async (conversationTurnContext, conversationCancellationToken) =>
                            {
                                await conversationTurnContext.SendActivityAsync(MessageFactory.Attachment(surveyNotificationCard));
                            },
                            CancellationToken.None);
                    }
#pragma warning disable CA1031 // Catching general exception for any errors occurred during retry logic to send survey notification to user.
                    catch (Exception ex)
#pragma warning disable CA1031 // Catching general exception for any errors occurred during retry logic to send survey notification to user.
                    {
                        this.logger.LogError(ex, $"Error while performing retry logic to send survey notification to user: {introductionEntity.NewHireAadObjectId}.");
                    }
                });
            }
        }

        /// <summary>
        /// Send the given attachment to the specified team.
        /// </summary>
        /// <param name="cardToSend">The attachment card to send.</param>
        /// <param name="teamEntity">Team preference model object.</param>
        /// <returns>A task that sends notification card in channel.</returns>
        private async Task SendCardToTeamAsync(
            Attachment cardToSend,
            TeamEntity teamEntity)
        {
            var serviceUrl = teamEntity.ServiceUrl;
            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl);
            var conversationReference = new ConversationReference()
            {
                ChannelId = Constants.TeamsBotFrameworkChannelId,
                Bot = new ChannelAccount() { Id = $"28:{this.botOptions.Value.MicrosoftAppId}" },
                ServiceUrl = serviceUrl,
                Conversation = new ConversationAccount() { Id = teamEntity.TeamId },
            };

            this.logger.LogInformation($"sending notification to channelId- {teamEntity.TeamId}");

            // Retry it in addition to the original call.
            await this.retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    await ((BotFrameworkAdapter)this.adapter).ContinueConversationAsync(
                            this.botOptions.Value.MicrosoftAppId,
                            conversationReference,
                            async (conversationTurnContext, conversationCancellationToken) =>
                            {
                                await conversationTurnContext.SendActivityAsync(MessageFactory.Attachment(cardToSend));
                            },
                            default);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"Error while performing retry logic to send view notification to channel for team: {teamEntity.TeamId}.");
                }
            });
        }
    }
}
