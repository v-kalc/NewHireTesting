// <copyright file="PairUpNotificationBackgroundService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.BackgroundService
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Hosting;
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
    /// BackgroundService class that inherits IHostedService and implements the methods related to background tasks for sending pair-up message once a day.
    /// </summary>
    public class PairUpNotificationBackgroundService : BackgroundService
    {
        /// <summary>
        /// Represents retry delay.
        /// </summary>
        private const int RetryDelay = 1000;

        /// <summary>
        /// Represents retry count.
        /// </summary>
        private const int RetryCount = 2;

        /// <summary>
        /// Retry policy with jitter, retry twice with a jitter delay of up to 1 sec. Retry for HTTP 429(transient error)/502 bad gateway.
        /// </summary>
        /// <remarks>
        /// Reference: https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#new-jitter-recommendation.
        /// </remarks>
        private readonly AsyncRetryPolicy retryPolicy = Policy.Handle<ErrorResponseException>(
            ex => ex.Response.StatusCode == HttpStatusCode.TooManyRequests || ex.Response.StatusCode == HttpStatusCode.BadGateway)
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(RetryDelay), RetryCount));

        /// <summary>
        /// Microsoft application credentials.
        /// </summary>
        private readonly MicrosoftAppCredentials microsoftAppCredentials;

        /// <summary>
        /// Bot adapter.
        /// </summary>
        private readonly IBotFrameworkHttpAdapter adapter;

        /// <summary>
        /// Instance to send logs to the Application Insights service.
        /// </summary>
        private readonly ILogger<PairUpNotificationBackgroundService> logger;

        /// <summary>
        /// The current cultures' string localizer.
        /// </summary>
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// Gets configuration setting for max pair-ups and notification duration.
        /// </summary>
        private readonly IOptionsMonitor<PairUpBackgroundServiceSettings> pairUpBackgroundServiceOption;

        /// <summary>
        /// Provider for fetching information about user details from storage.
        /// </summary>
        private readonly IUserStorageProvider userStorageProvider;

        /// <summary>
        /// Provider for fetching information about user introduction details from storage.
        /// </summary>
        private readonly IIntroductionStorageProvider introductionStorageProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PairUpNotificationBackgroundService"/> class.
        /// BackgroundService class that inherits IHostedService and implements the methods related to sending notification tasks.
        /// </summary>
        /// <param name="microsoftAppCredentials">Instance for Microsoft application credentials.</param>
        /// <param name="adapter">An instance of bot adapter.</param>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <param name="backgroundServiceOption">Configuration setting for max pair-ups and notification duration.</param>
        /// <param name="userStorageProvider">Provider for fetching information about user details from storage.</param>
        /// <param name="introductionStorageProvider">Provider for fetching information about user introduction details from storage.</param>
        public PairUpNotificationBackgroundService(
            MicrosoftAppCredentials microsoftAppCredentials,
            IBotFrameworkHttpAdapter adapter,
            ILogger<PairUpNotificationBackgroundService> logger,
            IStringLocalizer<Strings> localizer,
            IOptionsMonitor<PairUpBackgroundServiceSettings> backgroundServiceOption,
            IUserStorageProvider userStorageProvider,
            IIntroductionStorageProvider introductionStorageProvider)
        {
            this.microsoftAppCredentials = microsoftAppCredentials;
            this.adapter = adapter;
            this.logger = logger;
            this.localizer = localizer;
            this.pairUpBackgroundServiceOption = backgroundServiceOption;
            this.userStorageProvider = userStorageProvider;
            this.introductionStorageProvider = introductionStorageProvider;
        }

        /// <summary>
        /// This method is called when the Microsoft.Extensions.Hosting.IHostedService starts.
        /// The implementation should return a task that represents the lifetime of the long
        /// running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when Microsoft.Extensions.Hosting.IHostedService. StopAsync(System.Threading.CancellationToken) is called.</param>
        /// <returns>A System.Threading.Tasks.Task that represents the long running operations.</returns>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                int delayInNextPairUpNotification = this.pairUpBackgroundServiceOption.CurrentValue.DelayInPairUpNotificationInDays > 0 ? this.pairUpBackgroundServiceOption.CurrentValue.DelayInPairUpNotificationInDays : 1;
                try
                {
                    this.logger.LogInformation("Pair notification background job execution has started.");
                    await this.MakePairAndSendNotificationAsync();
                }
#pragma warning disable CA1031 // Catching general exceptions that might arise during execution to avoid blocking next run.
                catch (Exception ex)
#pragma warning restore CA1031 // Catching general exceptions that might arise during execution to avoid blocking next run.
                {
                    this.logger.LogError(ex, $"Error while sending pair-up message card at {nameof(this.MakePairAndSendNotificationAsync)}: {ex}");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromDays(delayInNextPairUpNotification), stoppingToken);
                }
            }
        }

        /// <summary>
        /// Make pair-up with random users and send notification once in a day for each team where app is installed.
        /// </summary>
        /// <returns>A task that make pair-up and send notification to random users for each team where app is installed.</returns>
        private async Task MakePairAndSendNotificationAsync()
        {
            this.logger.LogInformation("Making pair-ups");

            // Now notify each pair found in 1:1 and ask them to reach out to the other person
            // When contacting the user in 1:1, give them the button to opt-out
            try
            {
                // get all users who opted for pair up meetings
                var optedInUsers = await this.userStorageProvider.GetUsersOptedForPairUpMeetingAsync();

                if (optedInUsers != null)
                {
                    this.logger.LogInformation($"Total users: {optedInUsers.Count()} found for pair up meetings.");

                    // 1:1 pair (existing employee : new hire)
                    var pair = await this.MakePairsAsync(optedInUsers.ToList());
                    if (pair == null)
                    {
                        this.logger.LogInformation($"Pairs could not be made because there is no match found for pair up meetings.");
                    }
                    else
                    {
                        await this.NotifyPairAsync(pair.Item1, pair.Item2);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error pairing up members: {ex.Message}", SeverityLevel.Warning);
            }
        }

        /// <summary>
        /// Make pair based on users who opted for pair up meetings.
        /// </summary>
        /// <param name="users">All users who opted for pair up meetings.</param>
        /// <returns>Returns pair-up users.</returns>
        private async Task<Tuple<UserEntity, UserEntity>> MakePairsAsync(List<UserEntity> users)
        {
            // selecting a random existing user to create a 1:1 pair up with New hire
            var optedInExistingUsers = this.Randomize(users.Where(row => (DateTime.UtcNow - row.BotInstalledOn).Days > this.pairUpBackgroundServiceOption.CurrentValue.NewHireRetentionPeriodInDays).ToList()).FirstOrDefault();
            if (optedInExistingUsers == null)
            {
                this.logger.LogInformation($"Pairs could not be made because there is no existing users who opted for pair up meetings.");
                return null;
            }

            // get new hire employees who are within retention period
            var optedInNewHire = users
                .Where(row => (DateTime.UtcNow - row.BotInstalledOn).Days <= this.pairUpBackgroundServiceOption.CurrentValue.NewHireRetentionPeriodInDays).ToList();

            // pick one random new hire to pair up with existing users.
            var optedInNewHireForPairUp = this.Randomize(optedInNewHire).FirstOrDefault();
            if (optedInNewHireForPairUp == null)
            {
                this.logger.LogInformation($"Pairs could not be made because there is no new hire who opted for pair up meetings.");
                return null;
            }

            return new Tuple<UserEntity, UserEntity>(item1: optedInExistingUsers, item2: optedInNewHireForPairUp);
        }

        /// <summary>
        /// Select random users.
        /// </summary>
        /// <param name="items">Items.</param>
        /// <returns>Randomized list</returns>
        private IList<UserEntity> Randomize(IList<UserEntity> items)
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());

            // For each spot in the array, pick
            // a random item to swap into that spot.
            for (int i = 0; i < items.Count - 1; i++)
            {
                int j = rand.Next(i, items.Count);
                UserEntity temp = items[i];
                items[i] = items[j];
                items[j] = temp;
            }

            return items;
        }

        /// <summary>
        /// Notify a pair-up.
        /// </summary>
        /// <param name="person1">The pair-up person 1.</param>
        /// <param name="person2">The pair-up person 2.</param>
        /// <returns>Number of users notified successfully.</returns>
        private async Task<int> NotifyPairAsync(UserEntity person1, UserEntity person2)
        {
            this.logger.LogInformation($"Sending pair-up notification to {person1.AadObjectId} and {person2.AadObjectId}");

            // Fill in person2's info in the card for person1
            var cardForPerson1 = PairUpNotificationAdaptiveCard.GetPairUpNotificationCard(person1, person2, this.localizer);

            // Fill in person1's info in the card for person2
            var cardForPerson2 = PairUpNotificationAdaptiveCard.GetPairUpNotificationCard(person2, person1, this.localizer);

            // Send notifications and return the number that was successful
            var notifyResults = await Task.WhenAll(
                this.NotifyUserAsync(person1, cardForPerson1),
                this.NotifyUserAsync(person2, cardForPerson2));
            this.logger.LogInformation($"Pair-up notification sent to {person1.AadObjectId} and {person2.AadObjectId}");

            return notifyResults.Count(wasNotified => wasNotified);
        }

        /// <summary>
        /// Send notification to user.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="cardToSend">Card to send.</param>
        /// <returns>Task represents notification has been sent to user.</returns>
        private async Task<bool> NotifyUserAsync(UserEntity user, Attachment cardToSend)
        {
            cardToSend = cardToSend ?? throw new ArgumentNullException(nameof(cardToSend));

            string serviceUrl = user.ServiceUrl;
            var credentials = new MicrosoftAppCredentials(this.microsoftAppCredentials.MicrosoftAppId, this.microsoftAppCredentials.MicrosoftAppPassword);
            var conversationReference = new ConversationReference()
            {
                Bot = new ChannelAccount() { Id = $"28:{this.microsoftAppCredentials.MicrosoftAppId}" },
                Conversation = new ConversationAccount() { Id = user.ConversationId },
                ServiceUrl = user.ServiceUrl,
            };

            try
            {
                await this.retryPolicy.ExecuteAsync(async () =>
                {
                    await ((BotFrameworkAdapter)this.adapter).ContinueConversationAsync(
                            this.microsoftAppCredentials.MicrosoftAppId,
                            conversationReference,
                            async (conversationTurnContext, conversationCancellationToken) =>
                            {
                                this.logger.LogInformation($"Sending pair-up notification to user: {user.AadObjectId} for conversationId: {user.ConversationId}");
                                await conversationTurnContext.SendActivityAsync(MessageFactory.Attachment(cardToSend));
                            },
                            CancellationToken.None);
                });

                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error sending notification to user: {ex.Message}", SeverityLevel.Error);
                return false;
            }
        }
    }
}