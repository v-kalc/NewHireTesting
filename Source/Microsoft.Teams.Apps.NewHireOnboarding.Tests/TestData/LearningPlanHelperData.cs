// <copyright file="LearningPlanHelperTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Tests.TestData
{
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.Configuration;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.SharePoint;
    using System.Collections.Generic;

    // Learning plan helper test data.
    public static class LearningPlanHelperData
    {
        public static readonly IOptions<BotSettings> botOptions = Options.Create(new BotSettings()
        {
            MicrosoftAppId = "{Application id}",
            MicrosoftAppPassword = "{Application password or secret}",
            TenantId = "{Application tenant id}",
            AppBaseUri = "{Application base Url}"
        });

        public static readonly IOptions<SharePointSettings> sharePointOptions = Options.Create(new SharePointSettings()
        {
            CompleteLearningPlanUrl = "{Complete learning plan Url}",
            ShareFeedbackFormUrl = "{Share feedback form Url}",
        });

        public static readonly List<LearningPlanListItemField> learningPlanListDetail = new List<LearningPlanListItemField>()
        {
            new LearningPlanListItemField()
            {
                CompleteBy = "Week 1",
                Topic = "Technology",
                TaskName = "ReactJS"
            },
            new LearningPlanListItemField()
            {
                CompleteBy = "Week 1",
                Topic = "Technology",
                TaskName = "Azure"
            },
            new LearningPlanListItemField()
            {
                CompleteBy = "Week 2",
                Topic = "Management",
                TaskName = "Team management"
            }
        };
    }
}
