// <copyright file="IGraphApiHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.Graph;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.SharePoint;

    /// <summary>
    /// Interface to provide the helper methods to access Microsoft Graph Api.
    /// </summary>
    public interface IGraphApiHelper
    {
        /// <summary>
        /// Get user profile details from Microsoft Graph Api.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <param name="userId">Aad object id of user.</param>
        /// <returns>User profile details.</returns>
        Task<UserProfileDetail> GetUserProfileAsync(string token, string userId);

        /// <summary>
        /// Get user photo from Microsoft Graph.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <param name="userId">Aad object id of user.</param>
        /// <returns>User photo details.</returns>
        Task<byte[]> GetUserPhotoAsync(string token, string userId);

        /// <summary>
        /// Get user profile notes from Microsoft Graph.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <param name="userId">Aad object id of user.</param>
        /// <returns>User profile note.</returns>
        Task<string> GetUserProfileNoteAsync(string token, string userId);

        /// <summary>
        /// Get user manager from Microsoft Graph.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <returns>User manager profile details.</returns>
        Task<UserProfileDetail> GetUserManagerDetailsAsync(string token);

        /// <summary>
        /// Get joined teams from Microsoft Graph.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <returns>Joined teams details.</returns>
        Task<JoinedTeam> GetMyJoinedTeamsAsync(string token);

        /// <summary>
        /// Get joined teams from Microsoft Graph.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <param name="teamId">Unique id of Teams.</param>
        /// <returns>Channels details.</returns>
        Task<TeamChannel> GetChannelsAsync(string token, string teamId);

        /// <summary>
        /// Get group member details from Microsoft Graph.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <param name="groupId">Unique id of Azure Active Directory security group.</param>
        /// <returns>Channels details.</returns>
        Task<List<UserProfileDetail>> GetGroupMemberDetailsAsync(string token, string groupId);
    }
}
