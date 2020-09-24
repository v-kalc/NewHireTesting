// <copyright file="LearningPlanNotificationTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.NewHireOnboarding.Tests.BackgroundService
{
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.NewHireOnboarding.BackgroundService;
    using Microsoft.Teams.Apps.NewHireOnboarding.Interfaces;
    using Microsoft.Teams.Apps.NewHireOnboarding.Models;
    using Microsoft.Teams.Apps.NewHireOnboarding.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Threading.Tasks;

    // Class to test learning plan notification background service methods.
    [TestClass]
    public class LearningPlanNotificationTest
    {
        Mock<IUserStorageProvider> userStorageProvider;
        Mock<ILearningPlanHelper> learningPlanHelper;
        LearningPlanNotification learningPlanNotification;

        [TestInitialize]
        public void LearningPlanHelperTestSetup()
        {
            var logger = new Mock<ILogger<LearningPlanNotification>>().Object;
            userStorageProvider = new Mock<IUserStorageProvider>();
            IBotFrameworkHttpAdapter adapter = new BotFrameworkHttpAdapter();
            learningPlanHelper = new Mock<ILearningPlanHelper>();

            learningPlanNotification = new LearningPlanNotification(
                logger,
                LearningPlanHelperData.botOptions,
                adapter,
                userStorageProvider.Object,
                learningPlanHelper.Object,
                LearningPlanHelperData.sharePointOptions);
        }

        [TestMethod]
        public async Task NotificationSentSuccessAsync()
        {
            this.userStorageProvider
                 .Setup(x => x.GetAllUsersAsync(
                     (int)UserRole.NewHire))
                 .Returns(Task.FromResult(LearningPlanNotificationData.userEntities));

            this.learningPlanHelper
                .Setup(x => x.GetCompleteLearningPlansAsync())
                .Returns(Task.FromResult(LearningPlanNotificationData.learningPlanListDetail));

            var Result = await this.learningPlanNotification.SendWeeklyNotificationAsync();

            Assert.AreEqual(Result, true);
        }

        [TestMethod]
        public async Task LearningPlanNotExistAsync()
        {
            this.userStorageProvider
                 .Setup(x => x.GetAllUsersAsync(
                     (int)UserRole.NewHire))
                 .Returns(Task.FromResult(LearningPlanNotificationData.userEntities));

            this.learningPlanHelper
                .Setup(x => x.GetCompleteLearningPlansAsync())
                .Returns(Task.FromResult(LearningPlanNotificationData.learningPlanEmptyList));

            var Result = await this.learningPlanNotification.SendWeeklyNotificationAsync();

            Assert.AreEqual(Result, false);
        }
    }
}
