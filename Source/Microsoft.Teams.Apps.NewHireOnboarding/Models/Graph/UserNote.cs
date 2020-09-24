// <copyright file="UserNote.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Models.Graph
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// User note model class for Microsoft Graph Api.
    /// </summary>
    public class UserNote
    {
        /// <summary>
        /// Gets or sets value of user note.
        /// </summary>
        [JsonProperty("value")]
#pragma warning disable CA2227 // Getting error to make collection property as read only but needs to assign values.
        public List<UserProfile> UserProfileNote { get; set; }
#pragma warning disable CA2227 // Getting error to make collection property as read only but needs to assign values.
    }
}
