// <copyright file="NotificationSurveyCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;

    /// <summary>
    /// Class that helps to return survey notification card as attachment.
    /// </summary>
    public static class NotificationSurveyCard
    {
        /// <summary>
        /// Represent image width in pixel.
        /// </summary>
        private const uint ImageWidth = 280;

        /// <summary>
        /// Represent image height in pixel.
        /// </summary>
        private const uint ImageHeight = 100;

        /// <summary>
        /// This method will construct the survey notification card.
        /// </summary>
        /// <param name="applicationBasePath">Application base path to get the logo of the application.</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <param name="surveyNotificationSharePointPath">SharePoint path for Survey Notification.</param>
        /// <returns>Survey notification card attachment.</returns>
        public static Attachment GetSurveyNotificationCard(
            string applicationBasePath,
            IStringLocalizer<Strings> localizer,
            string surveyNotificationSharePointPath)
        {
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
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = localizer.GetString("CardHeaderText"),
                                        Spacing = AdaptiveSpacing.Small,
                                        Color = AdaptiveTextColor.Accent,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.Large,
                                        Spacing = AdaptiveSpacing.Medium,
                                        Text = localizer.GetString("CardSubHeaderText"),
                                        Wrap = true,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Spacing = AdaptiveSpacing.Medium,
                                        Text = localizer.GetString("CardContentText"),
                                        Wrap = true,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.Large,
                                        Spacing = AdaptiveSpacing.Medium,
                                        Text = localizer.GetString("QuestionsTitleText"),
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri($"{applicationBasePath}/Artifacts/notificationSurvey.png"),
                                        AltText = localizer.GetString("AlternativeText"),
                                        PixelHeight = ImageHeight,
                                        PixelWidth = ImageWidth,
                                    },
                                },
                            },
                        },
                    },
                },
            };

            card.Actions.Add(
                new AdaptiveOpenUrlAction
                {
                    Title = localizer.GetString("GetStartedButtonText"),
                    Url = new Uri(surveyNotificationSharePointPath),
                });

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}
