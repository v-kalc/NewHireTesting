// <copyright file="LearningPlanNotification.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.BackgroundService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Rest.Azure;
    using Microsoft.Teams.Apps.NewHireOnboarding.Interfaces;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.Configuration;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.EntityModels;
    using Polly;
    using Polly.Contrib.WaitAndRetry;
    using Polly.Retry;

    /// <summary>
    /// This class inherits IHostedService and implements the methods related to background tasks for sending learning plan notifications.
    /// </summary>
    public class LearningPlanNotification : BackgroundService
    {
        /// <summary>
        /// Default learning plan in weeks.
        /// </summary>
        private readonly int defaultLearningPlanInWeek;

        /// <summary>
        /// Instance to send logs to the Application Insights service.
        /// </summary>
        private readonly ILogger<LearningPlanNotification> logger;

        /// <summary>
        /// A set of key/value application configuration properties for bot settings.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Bot adapter used to handle bot framework HTTP requests.
        /// </summary>
        private readonly IBotFrameworkHttpAdapter adapter;

        /// <summary>
        /// Provider for fetching information about user details from storage.
        /// </summary>
        private readonly IUserStorageProvider userStorageProvider;

        /// <summary>
        /// Instance of learning helper to get learning plan methods.
        /// </summary>
        private readonly ILearningPlanHelper learningPlanHelper;

        /// <summary>
        /// A set of key/value application configuration properties for SharePoint.
        /// </summary>
        private readonly IOptions<SharePointSettings> sharePointOptions;

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
        /// Initializes a new instance of the <see cref="LearningPlanNotification"/> class.
        /// BackgroundService class that inherits IHostedService and implements the methods related to sending notification tasks.
        /// </summary>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="botOptions">A set of key/value application configuration properties.</param>
        /// <param name="adapter">Bot adapter used to handle bot framework HTTP requests.</param>
        /// <param name="userStorageProvider">Provider for fetching information about user details from storage.</param>
        /// <param name="learningPlanHelper">Instance of learning plan helper.</param>
        /// <param name="sharePointOptions">A set of key/value application configuration properties for SharePoint.</param>
        public LearningPlanNotification(
            ILogger<LearningPlanNotification> logger,
            IOptions<BotSettings> botOptions,
            IBotFrameworkHttpAdapter adapter,
            IUserStorageProvider userStorageProvider,
            ILearningPlanHelper learningPlanHelper,
            IOptions<SharePointSettings> sharePointOptions)
        {
            this.logger = logger;
            this.botOptions = botOptions ?? throw new ArgumentNullException(nameof(botOptions));
            this.sharePointOptions = sharePointOptions ?? throw new ArgumentNullException(nameof(sharePointOptions));
            this.adapter = adapter;
            this.userStorageProvider = userStorageProvider;
            this.learningPlanHelper = learningPlanHelper;
            this.sharePointOptions = sharePointOptions;
            this.defaultLearningPlanInWeek = sharePointOptions.Value.NewHireLearningPlansInWeeks > 0 ? sharePointOptions.Value.NewHireLearningPlansInWeeks : 4;
        }

        /// <summary>
        /// Send learning plan notification card to new hire on weekly basis.
        /// </summary>
        /// <returns>A task that represents whether weekly notification sent successfully or not.</returns>
        public async Task<bool> SendWeeklyNotificationAsync()
        {
            var allNewHireUsers = await this.userStorageProvider.GetAllUsersAsync((int)UserRole.NewHire);
            var completeLearningPlan = await this.learningPlanHelper.GetCompleteLearningPlansAsync();

            if (completeLearningPlan == null || !completeLearningPlan.Any())
            {
                this.logger.LogInformation("Complete learning plans data is not available.");

                return false;
            }

            var batchStartDate = DateTime.UtcNow;
            var learningDurationInDays = 0;

            for (int i = 1; i <= this.defaultLearningPlanInWeek; i++)
            {
                // To calculate weekly users list to send learning plan notification.
                var users = allNewHireUsers.Where(user => (batchStartDate - user.BotInstalledOn).Days > learningDurationInDays && (batchStartDate - user.BotInstalledOn).Days <= learningDurationInDays + 7).ToList();

                // To send weekly learning plan notification to new hire employees.
                if (users.Any())
                {
                    var listCardAttachment = this.learningPlanHelper.GetLearningPlanListCard(completeLearningPlan, week: $"{Constants.LearningPlanWeek} {i}");
                    foreach (var userDetail in users)
                    {
                        await this.SendCardToUserAsync(userDetail, listCardAttachment);
                    }
                }

                learningDurationInDays += 7;
                batchStartDate.AddDays(7);
            }

            return true;
        }

        /// <summary>
        ///  This method is called when the Microsoft.Extensions.Hosting.IHostedService starts.
        ///  The implementation should return a task that represents the lifetime of the long
        ///  running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken) is called.</param>
        /// <returns>A System.Threading.Tasks.Task that represents the long running operations.</returns>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var currentDateTime = DateTimeOffset.UtcNow;
                    this.logger.LogInformation($"Learning plan notification Hosted Service is running at: {currentDateTime}.");

                    if (currentDateTime.DayOfWeek == DayOfWeek.Monday)
                    {
                        await this.SendWeeklyNotificationAsync();
                        this.logger.LogInformation($"Monday of the week: {currentDateTime} and learning plan notification sent.");
                    }
                }
                catch (CloudException ex)
                {
                    this.logger.LogError(ex, $"Error occurred while accessing user details from storage: {ex.Message} at: {DateTimeOffset.UtcNow}");
                }
#pragma warning disable CA1031 // Catching general exceptions that might arise during execution to avoid blocking next run.
                catch (Exception ex)
#pragma warning restore CA1031 // Catching general exceptions that might arise during execution to avoid blocking next run.
                {
                    this.logger.LogError(ex, "Error occurred while running learning plan notification service.");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
            }
        }

        /// <summary>
        /// Send card to new hire as per the weekly basis.
        /// </summary>
        /// <param name="userEntity">User entity value.</param>
        /// <param name="listCardAttachment">List card attachment.</param>
        private async Task SendCardToUserAsync(UserEntity userEntity, Attachment listCardAttachment)
        {
            if (userEntity != null && listCardAttachment != null)
            {
                var conversationReference = new ConversationReference()
                {
                    Bot = new ChannelAccount() { Id = $"28:{this.botOptions.Value.MicrosoftAppId}" },
                    Conversation = new ConversationAccount() { Id = userEntity.ConversationId },
                    ServiceUrl = userEntity.ServiceUrl,
                };

                this.logger.LogInformation($"sending learning plan notification to conversationId- {userEntity.ConversationId}");

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
                                await conversationTurnContext.SendActivityAsync(MessageFactory.Attachment(listCardAttachment));
                            },
                            CancellationToken.None);
                    }
#pragma warning disable CA1031 // Catching general exceptions that might arise during execution to avoid blocking next run.
                    catch (Exception ex)
#pragma warning restore CA1031 // Catching general exceptions that might arise during execution to avoid blocking next run.
                    {
                        this.logger.LogError(ex, $"Error while performing retry logic to send user notification for weekly learning plan: {userEntity.ConversationId}.");
                    }
                });
            }
        }
    }
}
