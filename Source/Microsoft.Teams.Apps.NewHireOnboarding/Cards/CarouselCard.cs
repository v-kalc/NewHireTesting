// <copyright file="CarouselCard.cs" company="Microsoft">
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
    /// Class that helps to return Carousel card for help command.
    /// </summary>
    public static class CarouselCard
    {
        /// <summary>
        /// Represents On-boardingCheckList text.
        /// </summary>
        private const string OnBoardingCheckListText = "On-boarding check list";

        /// <summary>
        /// Represents share feedback text.
        /// </summary>
        private const string ShareFeedbackText = "Share feedback";

        /// <summary>
        /// Represents carousel image height in pixel.
        /// </summary>
        private const int CarouselImageHeight = 197;

        /// <summary>
        /// Represents carousel image height in pixel.
        /// </summary>
        private const int CarouselImageWidth = 420;

        /// <summary>
        ///  Create the set of cards that comprise the user help carousel.
        /// </summary>
        /// <param name="applicationBasePath">Application base URL.</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <param name="applicationManifestId">Application manifest id.</param>
        /// <param name="isManager">True when request comes from manager.</param>
        /// <returns>The cards that comprise the user tour.</returns>
        public static IEnumerable<Attachment> GetUserHelpCards(
            string applicationBasePath,
            IStringLocalizer<Strings> localizer,
            string applicationManifestId = null,
            bool isManager = false)
        {
            var attachments = new List<Attachment>();

            attachments.Add(new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = GetCarouselCard(
                    localizer.GetString("OnBoardingCheckListTitle"),
                    localizer.GetString("LearningPlanBriefText"),
                    $"{applicationBasePath}/Artifacts/learningPlanImage.png",
                    localizer,
                    applicationManifestId),
            });

            attachments.Add(new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = GetCarouselCard(
                    localizer.GetString("ShareFeedbackTitle"),
                    localizer.GetString("ShareFeedbackBriefText"),
                    $"{applicationBasePath}/Artifacts/shareFeedback.png",
                    localizer,
                    applicationManifestId),
            });

            if (isManager)
            {
                attachments.Add(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = GetCarouselCard(
                        localizer.GetString("ReviewIntroductionsText"),
                        localizer.GetString("ReviewIntroductionsBriefText"),
                        $"{applicationBasePath}/Artifacts/reviewintrosCarouselImage.png",
                        localizer,
                        isManager: true),
                });
            }
            else
            {
                attachments.Add(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = GetCarouselCard(
                        localizer.GetString("IntroductionTitle"),
                        localizer.GetString("IntroductionBriefText"),
                        $"{applicationBasePath}/Artifacts/newHireIntroduction.png",
                        localizer),
                });
            }

            return attachments;
        }

        /// <summary>
        /// Create carousel card for user tour.
        /// </summary>
        /// <param name="title">Title of the card.</param>
        /// <param name="briefText">Brief information about the actions.</param>
        /// <param name="imageUri">Image url.</param>
        /// <param name="localizer">The current culture string localizer.</param>
        /// <param name="applicationManifestId">Application manifest id.</param>
        /// <param name="isManager">True when request comes from manager.</param>
        /// <returns>Carousel card.</returns>
        private static AdaptiveCard GetCarouselCard(
            string title,
            string briefText,
            string imageUri,
            IStringLocalizer<Strings> localizer,
            string applicationManifestId = null,
            bool isManager = false)
        {
            AdaptiveCard carouselCard = new AdaptiveCard(new AdaptiveSchemaVersion(Constants.AdaptiveCardVersion))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = title,
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Large,
                    },
                    new AdaptiveImage
                    {
                         Url = new Uri(imageUri),
                         PixelWidth = CarouselImageWidth,
                         PixelHeight = CarouselImageHeight,
                         HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = briefText,
                        Wrap = true,
                    },
                },
            };

            if (title == OnBoardingCheckListText)
            {
                carouselCard.Actions.Add(
                    new AdaptiveOpenUrlAction
                    {
                        Title = localizer.GetString("ViewLearningButtonText"),
                        Url = new Uri($"https://teams.microsoft.com/l/entity/{applicationManifestId}/{Constants.OnboardingJourneyTabEntityId}"),
                    });
            }
            else if (title == ShareFeedbackText)
            {
                carouselCard.Actions.Add(
                    new AdaptiveSubmitAction
                    {
                        Title = localizer.GetString("ShareFeedbackButtonText"),
                        Data = new AdaptiveSubmitActionData
                        {
                            Msteams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Text = Constants.ShareFeedback,
                            },
                            Command = Constants.ShareFeedback,
                        },
                    });
            }
            else if (isManager)
            {
                carouselCard.Actions.Add(
                    new AdaptiveSubmitAction
                    {
                        Title = localizer.GetString("ReviewIntroductionsText"),
                        Data = new AdaptiveSubmitActionData
                        {
                            Msteams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Text = Constants.ReviewIntroductionAction,
                            },
                            Command = Constants.ReviewIntroductionAction,
                        },
                    });
            }
            else
            {
                carouselCard.Actions.Add(
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
                    });
            }

            return carouselCard;
        }
    }
}
