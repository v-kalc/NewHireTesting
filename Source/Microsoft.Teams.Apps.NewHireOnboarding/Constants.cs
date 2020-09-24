// <copyright file="Constants.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding
{
    /// <summary>
    /// A class that holds application constants that are used in multiple files.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Describes adaptive card version to be used. Version can be upgraded or changed using this value.
        /// </summary>
        public const string AdaptiveCardVersion = "1.2";

        /// <summary>
        /// default value for channel activity to send notifications.
        /// </summary>
        public const string TeamsBotFrameworkChannelId = "msteams";

        /// <summary>
        /// Represents the conversation type as personal.
        /// </summary>
        public const string PersonalConversationType = "personal";

        /// <summary>
        /// Represents the conversation type as channel.
        /// </summary>
        public const string ChannelConversationType = "channel";

        /// <summary>
        /// Microsoft Graph API base uri.
        /// </summary>
        public const string GraphAPIBaseURL = "https://graph.microsoft.com/";

        /// <summary>
        /// Introduce action.
        /// </summary>
        public const string IntroductionAction = "INTRODUCTION";

        /// <summary>
        /// List card content type.
        /// </summary>
        public const string ListCardContentType = "application/vnd.microsoft.teams.card.list";

        /// <summary>
        /// Submit introduction action.
        /// </summary>
        public const string SubmitIntroductionAction = "SUBMITINTRODUCTION";

        /// <summary>
        /// Review introduction action.
        /// </summary>
        public const string ReviewIntroductionAction = "REVIEW INTRODUCTIONS";

        /// <summary>
        /// View learning plan action.
        /// </summary>
        public const string ViewLearningAction = "LEARNINGS";

        /// <summary>
        /// Request more info action.
        /// </summary>
        public const string RequestMoreInfoAction = "REQUESTMOREINFO";

        /// <summary>
        /// Post team notification action.
        /// </summary>
        public const string PostTeamNotificationAction = "POSTTEAMNOTIFICATION";

        /// <summary>
        /// Task fetch action Type.
        /// </summary>
        public const string FetchActionType = "task/fetch";

        /// <summary>
        /// submit action Type.
        /// </summary>
        public const string SubmitActionType = "task/submit";

        /// <summary>
        /// Open URL Type.
        /// </summary>
        public const string OpenUrlType = "openUrl";

        /// <summary>
        /// Message back type.
        /// </summary>
        public const string MessageBack = "imback";

        /// <summary>
        /// Complete learning plan week.
        /// </summary>
        public const string LearningPlanWeek = "Week";

        /// <summary>
        /// Share Feedback action.
        /// </summary>
        public const string ShareFeedback = "SHARE FEEDBACK";

        /// <summary>
        /// On boarding check list action.
        /// </summary>
        public const string OnBoardingCheckListAction = "ONBOARDING CHECK LIST";

        /// <summary>
        /// Question unique id.
        /// </summary>
        public const string QuestionId = "QuestionId_";

        /// <summary>
        /// Approve action.
        /// </summary>
        public const string ApproveIntroductionAction = "APPROVE INTRODUCTION";

        /// <summary>
        /// Submit Feedback action.
        /// </summary>
        public const string SubmitFeedback = "SUBMITFEEDBACK";

        /// <summary>
        /// Help command action.
        /// </summary>
        public const string HelpAction = "HELP";

        /// <summary>
        /// Sets the cache duration.
        /// </summary>
        public const int CacheDurationInMinutes = 60;

        /// <summary>
        /// Feedback text input id.
        /// </summary>
        public const string FeedbackTextInputId = "FeedbackTextInput";

        /// <summary>
        /// Entity id of static onboarding journey tab.
        /// </summary>
        public const string OnboardingJourneyTabEntityId = "Journey";

        /// <summary>
        /// Pause all matches command text.
        /// </summary>
        public const string PauseAllMatches = "PAUSE ALL MATCHES";

        /// <summary>
        /// Resume all matches command text.
        /// </summary>
        public const string ResumeAllMatches = "RESUME ALL MATCHES";
    }
}
