// <copyright file="UserDetail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Models.Graph
{
    using Newtonsoft.Json;

    /// <summary>
    /// User detail model class for Microsoft Graph Api.
    /// </summary>
    public class UserDetail
    {
        /// <summary>
        /// Gets or sets content.
        /// </summary>
        [JsonProperty("content")]
        public string ProfileNote { get; set; }
    }
}
