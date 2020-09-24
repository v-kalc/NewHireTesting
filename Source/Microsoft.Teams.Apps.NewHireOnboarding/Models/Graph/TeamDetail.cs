// <copyright file="TeamDetail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Models.Graph
{
    using Newtonsoft.Json;

    /// <summary>
    /// Team details model class for Microsoft Graph Api.
    /// </summary>
    public class TeamDetail
    {
        /// <summary>
        /// Gets or sets odataContext.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
