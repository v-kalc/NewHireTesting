// <copyright file="LearningPlanHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.NewHireOnboarding.Cards;
    using Microsoft.Teams.Apps.NewHireOnboarding.Interfaces;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.Configuration;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.SharePoint;

    /// <summary>
    /// Implements the methods that are defined in <see cref="ILearningPlanHelper"/>.
    /// </summary>
    public class LearningPlanHelper : ILearningPlanHelper
    {
        /// <summary>
        /// Instance to log details in application insights.
        /// </summary>
        private readonly ILogger<LearningPlanHelper> logger;

        /// <summary>
        /// A set of key/value application configuration properties for bot settings.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Instance to work with Microsoft Graph methods.
        /// </summary>
        private readonly IGraphUtilityHelper graphUtility;

        /// <summary>
        /// Instance to get the SharePoint utility methods.
        /// </summary>
        private readonly ISharePointHelper sharePointHelper;

        /// <summary>
        /// The current culture's string localizer.
        /// </summary>
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// A set of key/value application configuration properties of SharePoint.
        /// </summary>
        private readonly IOptions<SharePointSettings> sharePointOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="LearningPlanHelper"/> class.
        /// </summary>
        /// <param name="logger">Instance of ILogger</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <param name="botOptions">A set of key/value application configuration properties.</param>
        /// <param name="sharePointOptions">A set of key/value application configuration properties for SharePoint.</param>
        /// <param name="graphUtility">Instance of Microsoft Graph utility helper.</param>
        /// <param name="sharePointHelper">Instance of SharePoint utility helper.</param>
        public LearningPlanHelper(
            ILogger<LearningPlanHelper> logger,
            IStringLocalizer<Strings> localizer,
            IOptions<BotSettings> botOptions,
            IOptions<SharePointSettings> sharePointOptions,
            IGraphUtilityHelper graphUtility,
            ISharePointHelper sharePointHelper)
        {
            this.logger = logger;
            this.localizer = localizer;
            this.botOptions = botOptions ?? throw new ArgumentNullException(nameof(botOptions));
            this.sharePointOptions = sharePointOptions ?? throw new ArgumentNullException(nameof(sharePointOptions));
            this.graphUtility = graphUtility;
            this.sharePointHelper = sharePointHelper;
        }

        /// <summary>
        /// Get complete learning plans details for new hire from SharePoint using Microsoft Graph.
        /// </summary>
        /// <returns>Complete learning plans details.</returns>
        public async Task<IEnumerable<LearningPlanListItemField>> GetCompleteLearningPlansAsync()
        {
            this.logger.LogInformation("Get complete learning plans initiated.");

            // Get Microsoft Graph token response.
            var response = await this.graphUtility.ObtainApplicationTokenAsync(
              this.botOptions.Value.TenantId,
              this.botOptions.Value.MicrosoftAppId,
              this.botOptions.Value.MicrosoftAppPassword);

            var result = await this.sharePointHelper.GetCompleteLearningPlanDataAsync(response.AccessToken);

            if (result == null)
            {
                this.logger.LogInformation("Get complete learning plans failed.");

                return null;
            }

            this.logger.LogInformation("Get complete learning plans succeed.");

            return result;
        }

        /// <summary>
        /// Get complete learning plans list card images.
        /// </summary>
        /// <returns>Complete learning plans list card images.</returns>
        public List<string> GetLearningPlanListCardImages()
        {
            return new List<string>()
            {
                $"{this.botOptions.Value.AppBaseUri}/Artifacts/listCardImage1.png",
                $"{this.botOptions.Value.AppBaseUri}/Artifacts/listCardImage2.png",
                $"{this.botOptions.Value.AppBaseUri}/Artifacts/listCardImage3.png",
                $"{this.botOptions.Value.AppBaseUri}/Artifacts/listCardImage4.png",
            };
        }

        /// <summary>
        /// Get learning plan card for selected week and item of the list card.
        /// </summary>
        /// <param name="learningPlan">Learning plan item value.</param>
        /// <returns>Learning plan card as attachment.</returns>
        public async Task<Attachment> GetLearningPlanCardAsync(string learningPlan)
        {
            learningPlan = learningPlan ?? throw new ArgumentNullException(nameof(learningPlan));

            if (learningPlan.Split("=>").Length == 3)
            {
                // Get learning plan data for selected learning content.
                var learningPlans = await this.GetCompleteLearningPlansAsync();

                if (learningPlans == null)
                {
                    this.logger.LogInformation("Get complete learning plans failed.");

                    return null;
                }

                var selectedWeekLearningPlan = learningPlans.Where(x => x.CompleteBy.ToUpperInvariant() == learningPlan.Split("=>")[0].Trim().ToUpperInvariant());

                if (selectedWeekLearningPlan == null)
                {
                    this.logger.LogInformation("Selected week learning plan is not available.");

                    return null;
                }

                var weeklyLearningPlan = selectedWeekLearningPlan.Where(
                    listItem => listItem.Topic.ToUpperInvariant() == learningPlan.Split("=>")[1].Trim().ToUpperInvariant()
                    && listItem.TaskName.Contains(
                        learningPlan.Split("=>")[2].Trim(), StringComparison.InvariantCultureIgnoreCase))?.FirstOrDefault();

                if (weeklyLearningPlan == null)
                {
                    this.logger.LogInformation("Learning plan content data not available.");

                    return null;
                }

                // Create learning plan data card.
                var learningCard = LearningPlanCard.GetNewHireLearningCard(
                    this.localizer,
                    this.botOptions.Value.AppBaseUri,
                    this.sharePointOptions.Value.CompleteLearningPlanUrl,
                    weeklyLearningPlan);

                return learningCard;
            }

            this.logger.LogInformation("Learning plan content data is not valid.");

            return null;
        }

        /// <summary>
        /// Send complete learning plan cards for selected week and learning content of the list card.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="completeLearningPlan">Complete learning plans data.</param>
        /// <returns>A task that represents whether learning plan list card is successfully send or not.</returns>
        public async Task<bool> SendCompleteLearningListCardsAsync(
            ITurnContext<IMessageActivity> turnContext,
            IEnumerable<LearningPlanListItemField> completeLearningPlan)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));

            var learningWeeks = completeLearningPlan.Where(
                            learningPlan => learningPlan.CompleteBy.Contains(this.localizer.GetString("ViewWeeklyLearningPlanCommandText"), StringComparison.InvariantCultureIgnoreCase))
                            .Select(learningPlan => learningPlan.CompleteBy)
                            .Distinct()
                            .OrderBy(learningWeek => learningWeek);

            foreach (var week in learningWeeks)
            {
                try
                {
                    var weekLearningPlanCard = LearningPlanListCard.GetLearningPlanListCard(
                        completeLearningPlan.Where(learningPlan => learningPlan.CompleteBy.ToUpperInvariant() == week.ToUpperInvariant()),
                        this.localizer,
                        this.GetLearningPlanListCardImages(),
                        $"{week} {this.localizer.GetString("LearningPlanWeekListCardTitleText")}",
                        this.botOptions.Value.ManifestId);

                    await turnContext.SendActivityAsync(MessageFactory.Attachment(weekLearningPlanCard));
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"Unable to send week: {week} learning plan content for user: {turnContext.Activity.From.AadObjectId}.");
                    throw;
                }
            }

            return true;
        }

        /// <summary>
        /// Send complete learning plan card for selected week and item of the list card.
        /// </summary>
        /// <param name="completeLearningPlan">Complete learning plan data.</param>
        /// <param name="week">Week to share to learning.</param>
        /// <returns>Learning plan card as attachment.</returns>
        public Attachment GetLearningPlanListCard(
            IEnumerable<LearningPlanListItemField> completeLearningPlan,
            string week)
        {
            var learningWeeks = completeLearningPlan.Where(
                            learningPlan => learningPlan.CompleteBy.Contains(this.localizer.GetString("ViewWeeklyLearningPlanCommandText"), StringComparison.InvariantCultureIgnoreCase))
                            .Select(learningPlan => learningPlan.CompleteBy)
                            .Distinct()
                            .OrderBy(learningWeek => learningWeek);

            return LearningPlanListCard.GetLearningPlanListCard(
                    completeLearningPlan.Where(learningPlan => learningPlan.CompleteBy.ToUpperInvariant() == week.ToUpperInvariant()),
                    this.localizer,
                    this.GetLearningPlanListCardImages(),
                    $"{week} {this.localizer.GetString("LearningPlanWeekListCardTitleText")}",
                    this.botOptions.Value.ManifestId);
        }
    }
}
