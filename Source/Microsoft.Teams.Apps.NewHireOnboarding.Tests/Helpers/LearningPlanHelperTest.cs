// <copyright file="LearningPlanHelperTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Tests.Helpers
{
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.NewHireOnboarding.Helpers;
    using Microsoft.Teams.Apps.NewHireOnboarding.Interfaces;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.Graph;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models.SharePoint;
    using Microsoft.Teams.Apps.NewHireOnboarding.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    // Class to test learning plan helper methods.
    [TestClass]
    public class LearningPlanHelperTest
    {
        Mock<IGraphUtilityHelper> graphUtility;
        Mock<ISharePointHelper> sharePointHelper;
        LearningPlanHelper learningPlanHelper;

        GraphTokenResponse graphTokenResponse = new GraphTokenResponse()
        {
            AccessToken = "{Microsoft Graph access token}",
        };

        private readonly List<LearningPlanListItemField> learningPlanEmptyList = new List<LearningPlanListItemField>();

        [TestInitialize]
        public void LearningPlanHelperTestSetup()
        {
            var logger = new Mock<ILogger<LearningPlanHelper>>().Object;
            var localizer = new Mock<IStringLocalizer<Strings>>().Object;
            graphUtility = new Mock<IGraphUtilityHelper>();
            sharePointHelper = new Mock<ISharePointHelper>();

            learningPlanHelper = new LearningPlanHelper(
                logger,
                localizer,
                LearningPlanHelperData.botOptions,
                LearningPlanHelperData.sharePointOptions,
                graphUtility.Object,
                sharePointHelper.Object);
        }

        [TestMethod]
        public async Task LearningPlansExistAsync()
        {
            this.graphUtility
                 .Setup(x => x.ObtainApplicationTokenAsync(
                       "{Application tenant id}",
                     "{Application client id}",
                     "{Application client secret}"))
                 .Returns(Task.FromResult(graphTokenResponse));
             
            this.sharePointHelper
                .Setup(x => x.GetCompleteLearningPlanDataAsync(graphTokenResponse.AccessToken))
                .Returns(Task.FromResult(LearningPlanHelperData.learningPlanListDetail));

            var Result = await this.learningPlanHelper.GetCompleteLearningPlansAsync();

            Assert.AreEqual(Result.ToList().Any(), true);
        }

        [TestMethod]
        public async Task LearningPlansNotExistAsync()
        {
            this.graphUtility
                 .Setup(x => x.ObtainApplicationTokenAsync(
                    "{Application tenant id}",
                     "{Application client id}",
                     "{Application client secret}"))
                 .Returns(Task.FromResult(graphTokenResponse));

            this.sharePointHelper
                .Setup(x => x.GetCompleteLearningPlanDataAsync(graphTokenResponse.AccessToken))
                .Returns(Task.FromResult(learningPlanEmptyList));

            var Result = await this.learningPlanHelper.GetCompleteLearningPlansAsync();

            Assert.AreEqual(Result, null);
        }

        [TestMethod]
        public void LearningPlanListCardImagesExist()
        {
            var Result = this.learningPlanHelper.GetLearningPlanListCardImages();
            Assert.AreEqual(Result.Any(), true);
        }

        [TestMethod]
        public void LearningPlanAttachmentCardExist()
        {
            this.graphUtility
                 .Setup(x => x.ObtainApplicationTokenAsync(
                    "{Application tenant id}",
                     "{Application client id}",
                     "{Application client secret}"))
                 .Returns(Task.FromResult(graphTokenResponse));

            this.sharePointHelper
                .Setup(x => x.GetCompleteLearningPlanDataAsync(graphTokenResponse.AccessToken))
                .Returns(Task.FromResult(LearningPlanHelperData.learningPlanListDetail));

            var Result = this.learningPlanHelper.GetLearningPlanCardAsync("Week 1 : Technology : ReactJS").Result;

            Assert.AreEqual(Result.ContentType, "application/vnd.microsoft.card.adaptive");
        }
    }
}
