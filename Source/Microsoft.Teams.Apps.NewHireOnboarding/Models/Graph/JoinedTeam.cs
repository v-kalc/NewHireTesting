// <copyright file="JoinedTeam.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Models.Graph
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Teams details model class for Microsoft Graph Api.
    /// </summary>
    public class JoinedTeam
    {
        /// <summary>
        /// Gets or sets odataContext.
        /// </summary>
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        /// <summary>
        /// Gets or sets Joined Teams.
        /// </summary>
#pragma warning disable CA2227 // Getting error to make collection property as read only but needs to assign values.
        [JsonProperty("value")]
        public List<TeamDetail> MyJoinedTeams { get; set; }
#pragma warning restore CA2227 // Getting error to make collection property as read only but needs to assign values.
    }
}
