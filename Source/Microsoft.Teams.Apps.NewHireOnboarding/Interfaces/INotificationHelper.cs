// <copyright file="INotificationHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Interfaces
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for notification helper, which helps in sending list card notification on Monthly/Weekly basis as per the configured preference to different users.
    /// </summary>
    public interface INotificationHelper
    {
        /// <summary>
        /// Send survey notification to new hire on Weekly basis in a batch.
        /// </summary>
        /// <returns>A task that sends survey notification to new hire.</returns>
        Task SendSurveyNotificationToNewHireAsync();

        /// <summary>
        /// Send feedback notification to hiring manager in team on Monthly basis as per the configuration.
        /// </summary>
        /// <returns>A task that sends feedback notification to hiring manager.</returns>
        Task SendFeedbackNotificationInChannelAsync();
    }
}
