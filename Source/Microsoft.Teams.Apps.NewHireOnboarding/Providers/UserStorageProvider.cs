// <copyright file="UserStorageProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.NewHireOnboarding.Interfaces;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.Configuration;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.EntityModels;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Implements storage provider which helps to storage user information in Azure Table Storage.
    /// </summary>
    public class UserStorageProvider : BaseStorageProvider, IUserStorageProvider
    {
        private const string UserConfigurationTable = "UserConfiguration";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserStorageProvider"/> class.
        /// </summary>
        /// <param name="options">A set of key/value application configuration properties for Azure Table Storage.</param>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        public UserStorageProvider(
            IOptions<StorageSettings> options,
            ILogger<UserStorageProvider> logger)
            : base(options?.Value.ConnectionString, UserConfigurationTable, logger)
        {
        }

        /// <summary>
        /// Store or update user detail in Azure Table Storage.
        /// </summary>
        /// <param name="userEntity">Represents user entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents user entity is saved or updated.</returns>
        public async Task<bool> StoreOrUpdateUserDetailAsync(UserEntity userEntity)
        {
            var result = await this.StoreOrUpdateEntityAsync(userEntity);

            return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Get already saved user entity from Azure Table Storage.
        /// </summary>
        /// <param name="userAadObjectId">Azure Active Directory object id of user.</param>
        /// <returns><see cref="Task"/>Returns user entity.</returns>
        public async Task<UserEntity> GetUserDetailAsync(string userAadObjectId)
        {
            await this.EnsureInitializedAsync();

            if (string.IsNullOrWhiteSpace(userAadObjectId))
            {
                return null;
            }

            var operation = TableOperation.Retrieve<UserEntity>(userAadObjectId, userAadObjectId);
            var data = await this.CloudTable.ExecuteAsync(operation);

            return data.Result as UserEntity;
        }

        /// <summary>
        /// Get all user details based on role.
        /// </summary>
        /// <param name="userRole">User role like 0:New Hire, 1:Hiring Manager.</param>
        /// <returns>List of users details based on role.</returns>
        public async Task<IEnumerable<UserEntity>> GetAllUsersAsync(int userRole)
        {
            await this.EnsureInitializedAsync();

            var userDetail = new List<UserEntity>();
            string userRoleCondition = TableQuery.GenerateFilterConditionForInt("UserRole", QueryComparisons.Equal, userRole);
            TableQuery<UserEntity> query = new TableQuery<UserEntity>().Where(userRoleCondition);
            TableContinuationToken tableContinuationToken = null;

            do
            {
                var queryResponse = await this.CloudTable.ExecuteQuerySegmentedAsync(query, tableContinuationToken);
                tableContinuationToken = queryResponse.ContinuationToken;
                userDetail.AddRange(queryResponse?.Results);
            }
            while (tableContinuationToken != null);

            return userDetail as List<UserEntity>;
        }

        /// <summary>
        /// Get all users who opted for pair-up meeting.
        /// </summary>
        /// <returns>List of users details.</returns>
        public async Task<IEnumerable<UserEntity>> GetUsersOptedForPairUpMeetingAsync()
        {
            await this.EnsureInitializedAsync();

            var userDetail = new List<UserEntity>();
            string optedInCondition = TableQuery.GenerateFilterConditionForBool("OptedIn", QueryComparisons.Equal, true);
            TableQuery<UserEntity> query = new TableQuery<UserEntity>().Where(optedInCondition);
            TableContinuationToken tableContinuationToken = null;

            do
            {
                var queryResponse = await this.CloudTable.ExecuteQuerySegmentedAsync(query, tableContinuationToken);
                tableContinuationToken = queryResponse.ContinuationToken;
                userDetail.AddRange(queryResponse?.Results);
            }
            while (tableContinuationToken != null);

            return userDetail;
        }

        /// <summary>
        /// Stores or update user details data in Azure Table Storage.
        /// </summary>
        /// <param name="entity">Holds user detail entity data.</param>
        /// <returns>A task that represents user entity data is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateEntityAsync(UserEntity entity)
        {
            await this.EnsureInitializedAsync();
            entity = entity ?? throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.AadObjectId)
                || string.IsNullOrWhiteSpace(entity.ConversationId)
                || string.IsNullOrWhiteSpace(entity.ServiceUrl))
            {
                return null;
            }

            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(entity);

            return await this.CloudTable.ExecuteAsync(addOrUpdateOperation);
        }
    }
}
