// <copyright file="IUserStorageProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.EntityModels;

    /// <summary>
    /// Interface for user storage provider.
    /// </summary>
    public interface IUserStorageProvider
    {
        /// <summary>
        /// Store or update user details in the storage.
        /// </summary>
        /// <param name="userEntity">Represents user entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> Returns the status whether user entity is stored or not.</returns>
        Task<bool> StoreOrUpdateUserDetailAsync(UserEntity userEntity);

        /// <summary>
        /// Get already saved user entity from Azure Table Storage.
        /// </summary>
        /// <param name="userAadObjectId">Azure Active Directory object id of user.</param>
        /// <returns><see cref="Task"/>Returns user entity.</returns>
        Task<UserEntity> GetUserDetailAsync(string userAadObjectId);

        /// <summary>
        /// Get all user details based on role.
        /// </summary>
        /// <param name="userRole">User role like 0:New Hire, 1:Hiring Manager.</param>
        /// <returns>List of users details based on role.</returns>
        Task<IEnumerable<UserEntity>> GetAllUsersAsync(int userRole);

        /// <summary>
        /// Get all users who opted for pair-up meeting.
        /// </summary>
        /// <returns>List of users details.</returns>
        Task<IEnumerable<UserEntity>> GetUsersOptedForPairUpMeetingAsync();
    }
}
