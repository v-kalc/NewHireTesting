// <copyright file="GraphApiHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Helpers
{
    extern alias BetaLib;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.NewHireOnboarding.Interfaces;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.Configuration;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.Graph;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.SharePoint;
    using Newtonsoft.Json;
#pragma warning disable SA1135 // Application requires both Graph v1.0 and beta SDKs which needs to add extern reference. More details can be found here : https://github.com/microsoftgraph/msgraph-beta-sdk-dotnet
    using Beta = BetaLib.Microsoft.Graph;
#pragma warning restore SA1135 // Application requires both Graph v1.0 and beta SDKs which needs to add extern reference. More details can be found here : https://github.com/microsoftgraph/msgraph-beta-sdk-dotnet

    /// <summary>
    /// The class that represent the helper methods to access Microsoft Graph API.
    /// </summary>
    public class GraphApiHelper : ITeamMembership, IUserProfile
    {
        /// <summary>
        /// Instance to send logs to the Application Insights service..
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Cache for storing Microsoft Graph result.
        /// </summary>
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphApiHelper"/> class.
        /// </summary>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="memoryCache">MemoryCache instance for caching Microsoft Graph result.</param>
        public GraphApiHelper(ILogger<GraphApiHelper> logger, IMemoryCache memoryCache)
        {
            this.logger = logger;
            this.memoryCache = memoryCache;
        }

        /// <summary>
        /// Get user profile details from Microsoft Graph.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <param name="userIds">Aad object id of users.</param>
        /// <returns>List of user profile details.</returns>
        public async Task<List<User>> GetUserProfileAsync(string token, List<string> userIds)
        {
            if (userIds == null || !userIds.Any())
            {
                return null;
            }

            string query = string.Empty;
            foreach (var id in userIds)
            {
                query += $"id eq '{id}' or ";
            }

            query = query.TrimEnd().Remove(query.Length - 3);

            var graphClient = this.GetGraphServiceClient(token);
            var users = await graphClient.Users
                .Request()
                .Filter(query)
                .Select("displayName, id, jobTitle")
                .GetAsync();

            if (users == null)
            {
                return null;
            }

            var userProfiles = users.ToList().Select(row => new User()
            {
                Id = row.Id,
                DisplayName = row.DisplayName,
                AboutMe = row.AboutMe,
                JobTitle = row.JobTitle,
            }).ToList();

            return userProfiles;
        }

        /// <summary>
        /// Get user photo from Microsoft Graph.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <param name="userId">Aad object id of user.</param>
        /// <returns>User photo details.</returns>
        public async Task<Stream> GetUserPhotoAsync(string token, string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return null;
                }

                this.memoryCache.TryGetValue(this.GetUserProfilePictureCacheKey(userId), out Stream cacheImage);
                if (cacheImage != null)
                {
                    return cacheImage;
                }

                var graphClient = this.GetGraphServiceClient(token);
                var stream = await graphClient
                    .Users[userId]
                    .Photo
                    .Content
                    .Request()
                    .GetAsync();

                if (stream == null)
                {
                    return null;
                }

                this.memoryCache.Set(this.GetUserProfilePictureCacheKey(userId), stream, TimeSpan.FromMinutes(Microsoft.Teams.Apps.NewHireOnboarding.Constants.CacheDurationInMinutes));

                return stream;
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                this.logger.LogInformation($"Graph API getting user photo error- {ex.Message}");
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound && ex.RawResponseBody.Contains("The photo wasn't found.", StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return null;
        }

        /// <summary>
        /// Get user profile notes from Microsoft Graph.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <param name="userId">Aad object id of user.</param>
        /// <returns>User profile note.</returns>
        public async Task<string> GetUserProfileNoteAsync(string token, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            var graphClient = this.GetGraphServiceClientBeta(token);
            var notes = await graphClient
                .Users[userId]
                .Profile
                .Notes
                .Request()
                .GetAsync();

            if (notes == null)
            {
                return null;
            }

            return notes.First().Detail?.Content;
        }

        /// <summary>
        /// Get user manager details from Microsoft Graph.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <returns>User manager details.</returns>
        public async Task<UserProfileDetail> GetUserManagerDetailsAsync(string token)
        {
            var graphClient = this.GetGraphServiceClient(token);
            var manager = await graphClient.Me.Manager
                .Request()
                .GetAsync();

            if (manager == null)
            {
                return null;
            }

            return new UserProfileDetail()
            {
                Id = manager.Id,
            };
        }

        /// <summary>
        /// Get joined teams from Microsoft Graph.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <returns>Joined teams details.</returns>
        public async Task<List<Team>> GetMyJoinedTeamsAsync(string token)
        {
            var graphClient = this.GetGraphServiceClient(token);
            var joinedTeams = await graphClient.Me.JoinedTeams
                .Request()
                .GetAsync();

            if (joinedTeams == null)
            {
                return null;
            }

            return joinedTeams.Select(row => row).ToList();
        }

        /// <summary>
        /// GET all channels of a team from Microsoft Graph.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <param name="teamId">Unique id of Teams.</param>
        /// <returns>Channels details.</returns>
        public async Task<List<Channel>> GetChannelsAsync(string token, string teamId)
        {
            if (string.IsNullOrWhiteSpace(teamId))
            {
                return null;
            }

            var graphClient = this.GetGraphServiceClient(token);
            var channels = await graphClient
                .Teams[teamId]
                .Channels
                .Request()
                .GetAsync();

            return channels.Select(row => row).ToList();
        }

        /// <summary>
        /// Get group member details from Microsoft Graph.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <param name="groupId">Unique id of Azure Active Directory security group.</param>
        /// <returns>Group members details.</returns>
        public async Task<List<User>> GetGroupMemberDetailsAsync(string token, string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                return null;
            }

            this.memoryCache.TryGetValue(this.GetSecurityGroupCacheKey(groupId), out List<User> cacheGroupMembers);
            if (cacheGroupMembers != null)
            {
                return cacheGroupMembers;
            }

            var graphClient = this.GetGraphServiceClient(token);
            var members = await graphClient
                .Groups[groupId]
                .Members
                .Request()
                .Select("id")
                .GetAsync();

            if (members != null)
            {
                var groupMembers = members.ToList().Select(row => new User() { Id = row.Id }).ToList();
                this.memoryCache.Set(this.GetSecurityGroupCacheKey(groupId), groupMembers, TimeSpan.FromMinutes(Microsoft.Teams.Apps.NewHireOnboarding.Constants.CacheDurationInMinutes));

                return groupMembers;
            }

            return null;
        }

        /// <summary>
        /// Get Microsoft Graph service client.
        /// </summary>
        /// <param name="accessToken">Token to access MS graph.</param>
        /// <returns>Returns a graph service client object.</returns>
        private GraphServiceClient GetGraphServiceClient(string accessToken)
        {
            return new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        await Task.Run(() =>
                        {
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                                "Bearer",
                                accessToken);
                        });
                    }));
        }

        /// <summary>
        /// Get Microsoft Graph service client beta.
        /// </summary>
        /// <param name="accessToken">Token to access MS graph.</param>
        /// <returns>Returns a graph service client object.</returns>
        private Beta.GraphServiceClient GetGraphServiceClientBeta(string accessToken)
        {
            return new Beta.GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        await Task.Run(() =>
                        {
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                                "Bearer",
                                accessToken);
                        });
                    }));
        }

        /// <summary>
        /// Get security group cache key value.
        /// </summary>
        /// <param name="groupId">Unique id of Azure Active Directory security group.</param>
        /// <returns>Returns a security group cache key value.</returns>
        private string GetSecurityGroupCacheKey(string groupId)
        {
            return CacheKeysConstants.SecurityGroup + groupId;
        }

        /// <summary>
        /// Get user profile picture cache key value.
        /// </summary>
        /// <param name="userId">Azure Active Directory id of user.</param>
        /// <returns>Returns cache key value for user profile picture.</returns>
        private string GetUserProfilePictureCacheKey(string userId)
        {
            return CacheKeysConstants.Image + userId;
        }
    }
}
