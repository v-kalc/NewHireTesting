// <copyright file="UserProfile.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Models.Graph
{
    using Newtonsoft.Json;

    /// <summary>
    /// User profile model class for Microsoft Graph Api.
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// Gets or sets user profile detail.
        /// </summary>
        [JsonProperty("detail")]
        public UserDetail UserDetail { get; set; }
    }
}
