// <copyright file="CardHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.NewHireOnboarding.Cards;
    using Microsoft.Teams.Apps.NewHireOnboarding.Interfaces;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.Card;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.Configuration;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.EntityModels;
    using Newtonsoft.Json;

    /// <summary>
    /// Class that helps to show cards in task module.
    /// </summary>
    public class CardHelper : ICardHelper
    {
        /// <summary>
        /// List card item type text.
        /// </summary>
        private const string ListCardItemTypeText = "person";

        /// <summary>
        /// Represents the introduction task module height in pixel.
        /// </summary>
        private const int InroductionTaskModuleHeight = 490;

        /// <summary>
        /// Represents the Validation introduction task module height in pixel.
        /// </summary>
        private const int ValidateInroductionTaskModuleHeight = 520;

        /// <summary>
        /// Represents the introduction task module width in pixel.
        /// </summary>
        private const int InroductionTaskModuleWidth = 870;

        /// <summary>
        /// Represents the validation message task module height in pixel.
        /// </summary>
        private const int ValidationMessageTaskModuleHeight = 150;

        /// <summary>
        /// Represents the validation message task module width in pixel.
        /// </summary>
        private const int ValidationMessageTaskModuleWidth = 400;

        /// <summary>
        /// Represents the approve detail task module width in pixel.
        /// </summary>
        private const int ApproveDetailTaskModuleWidth = 500;

        /// <summary>
        /// Represents the approve detail task module height in pixel.
        /// </summary>
        private const int ApproveDetailTaskModuleHeight = 350;

        /// <summary>
        /// The current culture's string localizer.
        /// </summary>
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// A set of key/value application configuration properties for bot settings.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Helper for working with Microsoft Graph API.
        /// </summary>
        private readonly IUserProfile graphApiHelper;

        /// <summary>
        /// Provider for fetching information about user details from storage.
        /// </summary>
        private readonly IUserStorageProvider userStorageProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardHelper"/> class.
        /// </summary>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <param name="botOptions">A set of key/value application configuration properties.</param>
        /// <param name="graphApiHelper">Helper for working with Microsoft Graph API.</param>
        /// <param name="userStorageProvider">Provider for fetching information about user details from storage.</param>
        public CardHelper(
            IStringLocalizer<Strings> localizer,
            IOptions<BotSettings> botOptions,
            IUserProfile graphApiHelper,
            IUserStorageProvider userStorageProvider)
        {
            this.localizer = localizer;
            this.botOptions = botOptions ?? throw new ArgumentNullException(nameof(botOptions));
            this.graphApiHelper = graphApiHelper;
            this.userStorageProvider = userStorageProvider;
        }

        /// <summary>
        /// Get introduction adaptive card.
        /// </summary>
        /// <param name="introductionEntity">New hire introduction details.</param>
        /// <param name="isAllQuestionAnswered">false if any of the question is not answered.</param>
        /// <returns>Envelope for Task Module Response.</returns>
        public TaskModuleResponse GetNewHireIntroductionCard(IntroductionEntity introductionEntity, bool isAllQuestionAnswered = true)
        {
            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Card = NewHireIntroductionCard.GetNewHireIntroductionCardAttachment(introductionEntity, this.localizer, this.botOptions.Value.AppBaseUri, isAllQuestionAnswered),
                        Height = isAllQuestionAnswered ? InroductionTaskModuleHeight : ValidateInroductionTaskModuleHeight,
                        Width = InroductionTaskModuleWidth,
                        Title = this.localizer.GetString("AppTitle"),
                    },
                },
            };
        }

        /// <summary>
        /// Gets introduction validation card to show in task module.
        /// </summary>
        /// <param name="introductionEntity">New hire introduction details.</param>
        /// <returns>Envelope for Task Module Response.</returns>
        public TaskModuleResponse GetIntroductionValidationCard(IntroductionEntity introductionEntity)
        {
            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Card = TellMeMoreCard.GetValidationMessageCard(introductionEntity, this.localizer),
                        Height = ValidationMessageTaskModuleHeight,
                        Width = ValidationMessageTaskModuleWidth,
                        Title = this.localizer.GetString("AppTitle"),
                    },
                },
            };
        }

        /// <summary>
        /// Get team confirmation adaptive card.
        /// </summary>
        /// <param name="teamChannelMapping">Teams/Channel mappings.</param>
        /// <param name="introductionEntity">New hire introduction details.</param>
        /// <param name="isTeamSelected">false if not team has selected.</param>
        /// <returns>Envelope for Task Module Response.</returns>
        public TaskModuleResponse GetApproveDetailCard(
            List<Models.TeamDetail> teamChannelMapping,
            IntroductionEntity introductionEntity,
            bool isTeamSelected = true)
        {
            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Card = HiringManagerNotificationCard.GetTeamConfirmationCard(teamChannelMapping, this.localizer, introductionEntity, isTeamSelected),
                        Height = ApproveDetailTaskModuleHeight,
                        Width = ApproveDetailTaskModuleWidth,
                        Title = this.localizer.GetString("AppTitle"),
                    },
                },
            };
        }

        /// <summary>
        /// Gets validation message details card.
        /// </summary>
        /// <param name="message">Message to show in card as validation.</param>
        /// <returns>Envelope for Task Module Response.</returns>
        public TaskModuleResponse GetValidationErrorCard(string message)
        {
            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Card = HiringManagerNotificationCard.GetValidationMessageCard(message),
                        Height = ValidationMessageTaskModuleHeight,
                        Width = ValidationMessageTaskModuleWidth,
                        Title = this.localizer.GetString("AppTitle"),
                    },
                },
            };
        }

        /// <summary>
        /// Get list card for pending review introductions.
        /// </summary>
        /// <param name="introductionEntities">List of introduction entities.</param>
        /// <param name="userGraphAccessToken">User access token.</param>
        /// <returns>Review introduction list card attachment.</returns>
        public async Task<Attachment> GetReviewIntroductionListCardAsync(
            IEnumerable<IntroductionEntity> introductionEntities,
            string userGraphAccessToken)
        {
            introductionEntities = introductionEntities ?? throw new ArgumentNullException(nameof(introductionEntities));

            ListCard card = new ListCard
            {
                Title = this.localizer.GetString("NewEmployeeTitleText"),
                Items = new List<ListCardItem>(),
            };

            var userProfileDetails = await this.graphApiHelper.GetUserProfileAsync(userGraphAccessToken, introductionEntities.Select(row => row.NewHireAadObjectId).ToList());

            foreach (var introduction in introductionEntities)
            {
                var userProfileDetail = userProfileDetails.Where(row => row.Id == introduction.NewHireAadObjectId).FirstOrDefault();

                // get user profile image url from user storage.
                var userDetails = await this.userStorageProvider.GetUserDetailAsync(introduction.NewHireAadObjectId);

                if (userDetails != null)
                {
                    introduction.UserProfileImageUrl = userDetails.UserProfileImageUrl;
                }

                card.Items.Add(new ListCardItem
                {
                    Type = ListCardItemTypeText,
                    Title = introduction.NewHireName,
                    Subtitle = string.IsNullOrEmpty(userProfileDetail?.JobTitle) ? string.Empty : userProfileDetail.JobTitle,
                    Icon = string.IsNullOrEmpty(introduction.UserProfileImageUrl) ? $"{this.botOptions.Value.AppBaseUri}/Artifacts/peopleAvatar.png" : introduction.UserProfileImageUrl,
                    Tap = new ListCardItemEvent
                    {
                        Type = Constants.MessageBack,
                        Value = $"{this.localizer.GetString("ReviewIntroductionCommandText")}:{introduction.NewHireName}",
                    },
                    Id = introduction.NewHireAadObjectId,
                });
            }

            return new Attachment
            {
                ContentType = Constants.ListCardContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Get Teams channel account detailing user Azure Active Directory details.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task<TeamsChannelAccount> GetUserDetailAsync(
          ITurnContext turnContext,
          CancellationToken cancellationToken)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));

            var members = await ((BotFrameworkAdapter)turnContext.Adapter).GetConversationMembersAsync(turnContext, cancellationToken);

            return JsonConvert.DeserializeObject<TeamsChannelAccount>(JsonConvert.SerializeObject(members[0]));
        }
    }
}