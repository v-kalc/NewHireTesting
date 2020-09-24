﻿// <copyright file="BotSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Models.Configuration
{
    /// <summary>
    /// A class which helps to provide Bot settings for application.
    /// </summary>
    public class BotSettings
    {
        /// <summary>
        /// Gets or sets application base Uri which helps in generating customer token.
        /// </summary>
        public string AppBaseUri { get; set; }

        /// <summary>
        /// Gets or sets application tenant id.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets application id.
        /// </summary>
        public string MicrosoftAppId { get; set; }

        /// <summary>
        /// Gets or sets application password.
        /// </summary>
        public string MicrosoftAppPassword { get; set; }

        /// <summary>
        /// Gets or sets application manifest id.
        /// </summary>
        public string ManifestId { get; set; }

        /// <summary>
        /// Gets or sets human resource team id.
        /// </summary>
        public string HumanResourceTeamId { get; set; }

        /// <summary>
        /// Gets or sets authoration policy duration in minutes.
        /// </summary>
        public int AuthorizationPolicyDurationInMinutes { get; set; }
    }
}
