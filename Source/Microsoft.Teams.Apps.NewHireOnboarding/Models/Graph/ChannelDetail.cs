// <copyright file="ChannelDetail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Models.Graph
{
    using Newtonsoft.Json;

    /// <summary>
    /// Channel details model class for Microsoft Graph Api.
    /// </summary>
    public class ChannelDetail
    {
        /// <summary>
        /// Gets or sets odataContext.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets description.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets displayName.
        /// </summary>
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets email.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets webUrl.
        /// </summary>
        [JsonProperty("webUrl")]
        public string WebUrl { get; set; }
    }
}
