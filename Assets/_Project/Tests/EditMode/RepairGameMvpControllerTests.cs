using MRRobotRepair.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace MRRobotRepair.Tests
{
    public sealed class RepairGameMvpControllerTests
    {
        [Test]
        public void RepairFlow_RejectsOutOfOrderAndCompletesInSequence()
        {
            GameObject root = new("Repair Game Test");
            try
            {
                RepairGameMvpController controller = root.AddComponent<RepairGameMvpController>();
                controller.Configure(
                    null,
                    null,
                    System.Array.Empty<RepairTarget>(),
                    System.Array.Empty<RepairSelectStep>(),
                    System.Array.Empty<RepairTool>(),
                    new[] { "step 1", "step 2" },
                    new[] { "hint 1", "hint 2" });

                controller.BeginGame();

                Assert.IsFalse(controller.TryCompleteStep(1));
                Assert.AreEqual(1, controller.MistakeCount);
                Assert.IsTrue(controller.TryCompleteStep(0));
                Assert.AreEqual(1, controller.CurrentStepIndex);
                Assert.IsTrue(controller.TryCompleteStep(1));
                Assert.IsTrue(controller.IsComplete);
                Assert.Greater(controller.FinalScore.TotalScore, 0);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
