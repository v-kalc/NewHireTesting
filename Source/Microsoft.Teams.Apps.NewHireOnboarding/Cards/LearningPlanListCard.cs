// <copyright file="LearningPlanListCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.Card;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.SharePoint;

    /// <summary>
    /// Class that helps to create learning plan list card.
    /// </summary>
    public static class LearningPlanListCard
    {
        /// <summary>
        /// Get list card for complete learning plan.
        /// </summary>
        /// <param name="learningPlans">Learning plans list object.</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <param name="learningPlanlistCardImages">Learning plans list card images.</param>
        /// <param name="cardTitle">Learning plan list card title.</param>
        /// <param name="applicationManifestId">Application manifest id.</param>
        /// <returns>An attachment card for learning plan.</returns>
        public static Attachment GetLearningPlanListCard(
            IEnumerable<LearningPlanListItemField> learningPlans,
            IStringLocalizer<Strings> localizer,
            List<string> learningPlanlistCardImages,
            string cardTitle,
            string applicationManifestId)
        {
            learningPlans = learningPlans ?? throw new ArgumentNullException(nameof(learningPlans));
            learningPlanlistCardImages = learningPlanlistCardImages ?? throw new ArgumentNullException(nameof(learningPlanlistCardImages));

            ListCard card = new ListCard
            {
                Title = cardTitle,
                Items = new List<ListCardItem>(),
                Buttons = new List<ListCardButton>(),
            };

            // To get random image from a list of images.
            Random random = new Random();

            int counter = 0;
            foreach (var learningPlan in learningPlans)
            {
                var imagePath = learningPlanlistCardImages[random.Next(0, learningPlanlistCardImages.Count - 1)];

                card.Items.Add(new ListCardItem
                {
                    Type = "resultItem",
                    Id = Convert.ToString(counter, CultureInfo.InvariantCulture),
                    Title = learningPlan.Topic,
                    Subtitle = learningPlan.TaskName,
                    Icon = imagePath,
                    Tap = new ListCardItemEvent
                    {
                        Type = Constants.MessageBack,
                        Value = $"{learningPlan.CompleteBy} => {learningPlan.Topic} => {learningPlan.TaskName}",
                    },
                });

                counter++;
            }

            var viewCompletePlanActionButton = new ListCardButton()
            {
                Title = localizer.GetString("ViewCompleteLearningPlanButtonText"),
                Type = Constants.OpenUrlType,
                Value = $"https://teams.microsoft.com/l/entity/{applicationManifestId}/{Constants.OnboardingJourneyTabEntityId}",
            };

            card.Buttons.Add(viewCompletePlanActionButton);

            var shareFeedbackActionButton = new ListCardButton()
            {
                Title = localizer.GetString("ShareFeedbackButtonText"),
                Type = Constants.MessageBack,
                Value = Constants.ShareFeedback,
            };

            card.Buttons.Add(shareFeedbackActionButton);

            return new Attachment
            {
                ContentType = Constants.ListCardContentType,
                Content = card,
            };
        }
    }
}
