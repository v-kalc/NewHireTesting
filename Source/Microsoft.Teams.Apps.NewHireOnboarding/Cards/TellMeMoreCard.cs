// <copyright file="TellMeMoreCard.cs" company="Microsoft">
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
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.EntityModels;

    /// <summary>
    /// Class that helps to return edit introduction card as attachment.
    /// </summary>
    public static class TellMeMoreCard
    {
        /// <summary>
        /// Represent image width in pixel.
        /// </summary>
        private const uint ImageWidth = 250;

        /// <summary>
        /// Represent image height in pixel.
        /// </summary>
        private const uint ImageHeight = 128;

        /// <summary>
        /// Represent icon edit card left column width.
        /// </summary>
        private const string EditCardLeftColumnWidth = "70";

        /// <summary>
        /// Represent icon edit card right column width.
        /// </summary>
        private const string EditCardRightColumnWidth = "30";

        /// <summary>
        /// This method will construct the edit introduction card for new hire employee.
        /// </summary>
        /// <param name="applicationBasePath">Application base path to get the logo of the application.</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <param name="introductionEntity">New hire introduction details.</param>
        /// <returns>Tell me more card attachment.</returns>
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
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = EditCardLeftColumnWidth,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.Large,
                                        Text = localizer.GetString("EditIntroCardHeaderText"),
                                        Spacing = AdaptiveSpacing.Small,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Spacing = AdaptiveSpacing.Small,
                                        Text = localizer.GetString("EditIntroCardSubHeaderText"),
                                        Wrap = true,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.Medium,
                                        Spacing = AdaptiveSpacing.Small,
                                        Text = localizer.GetString("ManagerCommentsTitleText"),
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Spacing = AdaptiveSpacing.Small,
                                        Text = introductionEntity.Comments,
                                        Wrap = true,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = EditCardRightColumnWidth,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri($"{applicationBasePath}/Artifacts/moreInformationImage.png"),
                                        AltText = localizer.GetString("AlternativeText"),
                                        PixelHeight = ImageHeight,
                                        PixelWidth = ImageWidth,
                                    },
                                },
                            },
                        },
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = localizer.GetString("EditIntroButtonText"),
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
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Construct validation message card to show on task module.
        /// </summary>
        /// <param name="introductionEntity">New hire introduction details.</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <returns>Validation message card attachment.</returns>
        public static Attachment GetValidationMessageCard(IntroductionEntity introductionEntity, IStringLocalizer<Strings> localizer)
        {
            introductionEntity = introductionEntity ?? throw new ArgumentNullException(nameof(introductionEntity));

            AdaptiveCard validationCard = new AdaptiveCard(new AdaptiveSchemaVersion(Constants.AdaptiveCardVersion))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                        Text = introductionEntity.ApprovalStatus == (int)IntroductionStatus.PendingForApproval ? localizer.GetString("PendingMessageText") : localizer.GetString("ApprovedMessageText"),
                        Wrap = true,
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = validationCard,
            };
        }
    }
}
