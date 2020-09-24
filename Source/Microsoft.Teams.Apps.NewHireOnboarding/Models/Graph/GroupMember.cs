// <copyright file="GroupMember.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Models.Graph
{
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.SharePoint;
    using Newtonsoft.Json;

    /// <summary>
    /// Model class for Security group member for Microsoft Graph Api.
    /// </summary>
    public class GroupMember
    {
        /// <summary>
        /// Gets or sets odataContext.
        /// </summary>
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        /// <summary>
        /// Gets or sets list of security group members.
        /// </summary>
#pragma warning disable CA2227 // Getting error to make collection property as read only but needs to assign values.
        [JsonProperty("value")]
        public List<UserProfileDetail> SecurityGroupMembers { get; set; }
#pragma warning restore CA2227 // Getting error to make collection property as read only but needs to assign values.
    }
}
