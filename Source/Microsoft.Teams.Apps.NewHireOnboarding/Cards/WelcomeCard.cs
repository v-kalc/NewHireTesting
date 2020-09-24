// <copyright file="WelcomeCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models;

    /// <summary>
    /// Class that helps to return welcome card as attachment.
    /// </summary>
    public static class WelcomeCard
    {
        /// <summary>
        /// Represent image width in pixel.
        /// </summary>
        private const uint ImageWidth = 424;

        /// <summary>
        /// Represent image height in pixel.
        /// </summary>
        private const uint ImageHeight = 158;

        /// <summary>
        /// This method will construct the new hire welcome card when bot is added in personal scope.
        /// </summary>
        /// <param name="applicationBasePath">Application base path to get the logo of the application.</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <returns>New hire welcome card attachment.</returns>
        public static Attachment GetNewHireWelcomeCard(
            string applicationBasePath,
            IStringLocalizer<Strings> localizer)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(Constants.AdaptiveCardVersion))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("WelcomeHeaderText"),
                        Spacing = AdaptiveSpacing.Medium,
                        Color = AdaptiveTextColor.Accent,
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Medium,
                    },
                    new AdaptiveTextBlock
                    {
                        Spacing = AdaptiveSpacing.None,
                        Size = AdaptiveTextSize.ExtraLarge,
                        Text = $"**{localizer.GetString("WelcomeSubHeaderText")}**",
                    },
                    new AdaptiveImage
                    {
                        Url = new Uri($"{applicationBasePath}/Artifacts/welcomeCardImage.png"),
                        AltText = localizer.GetString("AlternativeText"),
                        PixelHeight = ImageHeight,
                        PixelWidth = ImageWidth,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("WelcomeContentText"),
                        Spacing = AdaptiveSpacing.Small,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("AccessOnDemandBulletPoint"),
                        Spacing = AdaptiveSpacing.Small,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("HelpBulletPoint"),
                        Spacing = AdaptiveSpacing.Small,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("MoreInfoBulletPoint"),
                        Spacing = AdaptiveSpacing.Small,
                        Wrap = true,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = localizer.GetString("IntroduceButtonText"),
                        Data = new AdaptiveSubmitActionData
                        {
                            Msteams = new CardAction
                            {
                                Type = Constants.FetchActionType,
                                Text = Constants.IntroductionAction,
                            },
                            Command = Constants.IntroductionAction,
                        },
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = localizer.GetString("TakeaTourButtonText"),
                        Data = new AdaptiveSubmitActionData
                        {
                            Msteams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Text = Constants.HelpAction,
                            },
                            Command = Constants.HelpAction,
                        },
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = localizer.GetString("ViewLearningButtonText"),
                        Data = new AdaptiveSubmitActionData
                        {
                            Msteams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Text = Constants.ViewLearningAction,
                            },
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// This method will construct the team welcome card when bot is added in team scope.
        /// </summary>
        /// <param name="applicationBasePath">Application base path to get the logo of the application.</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <returns>Team welcome card attachment.</returns>
        public static Attachment GetTeamWelcomeCard(
            string applicationBasePath,
            IStringLocalizer<Strings> localizer)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(Constants.AdaptiveCardVersion))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Large,
                        Spacing = AdaptiveSpacing.None,
                        Text = localizer.GetString("WelcomeSubHeaderText"),
                    },
                    new AdaptiveImage
                    {
                        Url = new Uri($"{applicationBasePath}/Artifacts/welcomeCardImage.png"),
                        AltText = localizer.GetString("AlternativeText"),
                        PixelHeight = ImageHeight,
                        PixelWidth = ImageWidth,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("TeamWelcomeContentText"),
                        Spacing = AdaptiveSpacing.Small,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("TeamWelcomeCardBulletPointText"),
                        Spacing = AdaptiveSpacing.Small,
                        Wrap = true,
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// This method will construct the hiring manager card when bot is added in personal scope.
        /// </summary>
        /// <param name="applicationBasePath">Application base path to get the logo of the application.</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <returns>Hiring manager welcome card attachment.</returns>
        public static Attachment GetHiringManagerWelcomeCard(
            string applicationBasePath,
            IStringLocalizer<Strings> localizer)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(Constants.AdaptiveCardVersion))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Large,
                        Spacing = AdaptiveSpacing.None,
                        Text = localizer.GetString("WelcomeSubHeaderText"),
                    },
                    new AdaptiveImage
                    {
                        Url = new Uri($"{applicationBasePath}/Artifacts/welcomeCardImage.png"),
                        AltText = localizer.GetString("AlternativeText"),
                        PixelHeight = ImageHeight,
                        PixelWidth = ImageWidth,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("HiringManagerWelcomeContentText"),
                        Spacing = AdaptiveSpacing.Small,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("HiringManagerBulletPoint1Text"),
                        Spacing = AdaptiveSpacing.Small,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("HiringManagerBulletPoint2Text"),
                        Spacing = AdaptiveSpacing.Small,
                        Wrap = true,
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// This method will construct the HR welcome card when bot is added in team scope.
        /// </summary>
        /// <param name="applicationBasePath">Application base path to get the logo of the application.</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <returns>Human resource welcome card attachment.</returns>
        public static Attachment GetHumanResourceWelcomeCard(
            string applicationBasePath,
            IStringLocalizer<Strings> localizer)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(Constants.AdaptiveCardVersion))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Large,
                        Spacing = AdaptiveSpacing.None,
                        Text = localizer.GetString("WelcomeSubHeaderText"),
                    },
                    new AdaptiveImage
                    {
                        Url = new Uri($"{applicationBasePath}/Artifacts/welcomeCardImage.png"),
                        AltText = localizer.GetString("AlternativeText"),
                        PixelHeight = ImageHeight,
                        PixelWidth = ImageWidth,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("HumanResourceWelcomeContentText"),
                        Spacing = AdaptiveSpacing.Small,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("HumanResourceBulletPointText"),
                        Spacing = AdaptiveSpacing.Small,
                        Wrap = true,
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
