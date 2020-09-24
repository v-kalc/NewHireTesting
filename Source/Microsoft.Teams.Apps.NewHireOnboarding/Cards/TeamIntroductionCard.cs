// <copyright file="TeamIntroductionCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.EntityModels;

    /// <summary>
    /// Class that helps to team introduction card as attachment.
    /// </summary>
    public static class TeamIntroductionCard
    {
        /// <summary>
        /// Represent image width in pixel.
        /// </summary>
        private const uint ImageWidth = 200;

        /// <summary>
        /// Represent image width in pixel.
        /// </summary>
        private const uint ImageHeight = 200;

        /// <summary>
        /// Get notification card after approved introduction from hiring manager.
        /// </summary>
        /// <param name="applicationBasePath">Application base path to get the logo of the application.</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <param name="introductionEntity">New hire introduction details.</param>
        /// <returns>Team introduction card attachment.</returns>
        public static Attachment GetCard(
            string applicationBasePath,
            IStringLocalizer<Strings> localizer,
            IntroductionEntity introductionEntity)
        {
            introductionEntity = introductionEntity ?? throw new ArgumentNullException(nameof(introductionEntity));

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(Constants.AdaptiveCardVersion))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("TeamNotificationHeaderText", introductionEntity.NewHireName),
                        Spacing = AdaptiveSpacing.Small,
                        Color = AdaptiveTextColor.Accent,
                    },
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Large,
                        Spacing = AdaptiveSpacing.None,
                        Text = introductionEntity.NewHireName,
                    },
                    new AdaptiveImage
                    {
                        Url = new Uri(!string.IsNullOrEmpty(introductionEntity.UserProfileImageUrl) ? introductionEntity.UserProfileImageUrl : $"{applicationBasePath}/Artifacts/peopleAvatar.png"),
                        AltText = localizer.GetString("AlternativeText"),
                        Spacing = AdaptiveSpacing.ExtraLarge,
                        PixelHeight = ImageHeight,
                        PixelWidth = ImageWidth,
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                        Style = AdaptiveImageStyle.Person,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = !string.IsNullOrEmpty(introductionEntity.NewHireProfileNote) ? introductionEntity.NewHireProfileNote : localizer.GetString("IntroductionGreetText", introductionEntity.NewHireName),
                        Spacing = AdaptiveSpacing.Small,
                        Wrap = true,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Title = localizer.GetString("ChatButtonText", introductionEntity.NewHireName),
                        Url = new Uri($"https://teams.microsoft.com/l/chat/0/0?users={introductionEntity.NewHireUserPrincipalName}"),
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}
