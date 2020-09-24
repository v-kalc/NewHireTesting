// <copyright file="ILearningPlanHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.SharePoint;

    /// <summary>
    /// This interface will contain the helper methods to fetch complete learning plan.
    /// </summary>
    public interface ILearningPlanHelper
    {
        /// <summary>
        /// Get complete learning plans details for new hire.
        /// </summary>
        /// <returns>Complete learning plans details.</returns>
        Task<IEnumerable<LearningPlanListItemField>> GetCompleteLearningPlansAsync();

        /// <summary>
        /// Get complete learning plans list card images.
        /// </summary>
        /// <returns>Complete learning plans list card images.</returns>
        List<string> GetLearningPlanListCardImages();

        /// <summary>
        /// Get learning plan card for selected week and item of the list card.
        /// </summary>
        /// <param name="learningPlan">Learning plan item value.</param>
        /// <returns>Learning plan card as attachment.</returns>
        Task<Attachment> GetLearningPlanCardAsync(string learningPlan);

        /// <summary>
        /// Send complete learning plan cards for selected week and item of the list card.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="completeLearningPlan">Complete learning plans data.</param>
        /// <returns>A task that represents whether learning plan list card is successfully send or not.</returns>
        Task<bool> SendCompleteLearningListCardsAsync(
            ITurnContext<IMessageActivity> turnContext,
            IEnumerable<LearningPlanListItemField> completeLearningPlan);

        /// <summary>
        /// Send complete learning plan cards for selected week and item of the list card.
        /// </summary>
        /// <param name="completeLearningPlan">Complete learning plans data.</param>
        /// <param name="week">Week to share to learning.</param>
        /// <returns>Learning plan card as attachment.</returns>
        Attachment GetLearningPlanListCard(
            IEnumerable<LearningPlanListItemField> completeLearningPlan,
            string week);
    }
}
