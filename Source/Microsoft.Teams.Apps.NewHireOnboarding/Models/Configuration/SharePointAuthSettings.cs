// <copyright file="SharePointAuthSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Models.Configuration
{
    /// <summary>
    /// A class which helps to provide SharePoint auth settings for application.
    /// </summary>
    public class SharePointAuthSettings
    {
        /// <summary>
        /// Gets or sets application tenant id.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets application client id.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets application secret.
        /// </summary>
        public string ClientSecret { get; set; }
    }
}
