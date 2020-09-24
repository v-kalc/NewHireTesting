// <copyright file="LearningPlanCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.SharePoint;

    /// <summary>
    /// Class that helps to return learning card for new hire as attachment.
    /// </summary>
    public static class LearningPlanCard
    {
        /// <summary>
        /// Represents image height in pixel.
        /// </summary>
        private const int ImageHeight = 132;

        /// <summary>
        /// Represents image width in pixel.
        /// </summary>
        private const int ImageWidth = 500;

        /// <summary>
        /// Get learning card attachment for new hire to show on Microsoft Teams personal scope.
        /// </summary>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <param name="appBasePath">Application base uri to create image path.</param>
        /// <param name="completeLearningPlanPath">Complete learning plan SharePoint URL.</param>
        /// <param name="learningPlan">Learning plan details.</param>
        /// <returns>New hire learning card attachment.</returns>
        public static Attachment GetNewHireLearningCard(
            IStringLocalizer<Strings> localizer,
            string appBasePath,
            string completeLearningPlanPath,
            LearningPlanListItemField learningPlan)
        {
            learningPlan = learningPlan ?? throw new ArgumentNullException(nameof(learningPlan));

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(Constants.AdaptiveCardVersion))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Stretch,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Spacing = AdaptiveSpacing.Medium,
                                        Text = $"**{learningPlan.Topic}**",
                                        Wrap = true,
                                        Size = AdaptiveTextSize.Large,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.Large,
                                        Spacing = AdaptiveSpacing.Medium,
                                        Text = learningPlan.TaskName,
                                        Wrap = true,
                                    },
                                    new AdaptiveImage
                                    {
                                        Url = string.IsNullOrEmpty(learningPlan.TaskImage?.Url)
                                        ? new Uri($"{appBasePath}/Artifacts/learningPlan.png")
                                        : new Uri(learningPlan.TaskImage?.Url),
                                        AltText = learningPlan.Notes,
                                        PixelHeight = ImageHeight,
                                        PixelWidth = ImageWidth,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Spacing = AdaptiveSpacing.Medium,
                                        Text = learningPlan.Notes,
                                        Wrap = true,
                                    },
                                },
                            },
                        },
                    },
                },
                Actions = new List<AdaptiveAction>(),
            };

            card.Actions.Add(
                new AdaptiveOpenUrlAction
                {
                    Title = localizer.GetString("ViewLearningPlanButtonText"),
                    Url = string.IsNullOrEmpty(learningPlan?.Link?.Url)
                    ? new Uri(completeLearningPlanPath)
                    : new Uri(learningPlan.Link.Url),
                });

            card.Actions.Add(
                new AdaptiveSubmitAction
                {
                    Title = localizer.GetString("LearningPlanShareFeedbackButtonText"),
                    Data = new AdaptiveSubmitActionData
                    {
                        Msteams = new CardAction
                        {
                            Type = ActionTypes.MessageBack,
                            Text = Constants.ShareFeedback,
                        },
                    },
                });

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}
