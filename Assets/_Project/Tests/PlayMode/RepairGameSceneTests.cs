using System.Collections;
using MRRobotRepair.Core;
using MRRobotRepair.Gameplay;
using MRRobotRepair.MR;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

namespace MRRobotRepair.Tests
{
    public sealed class RepairGameSceneTests
    {
        [UnityTest]
        public IEnumerator RepairGameScene_HasStationaryPlacementRigAndHiddenContent()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("RepairGame", LoadSceneMode.Single);
            while (!load.isDone) yield return null;
            yield return null;

            WorkspacePlacementController placement = Object.FindFirstObjectByType<WorkspacePlacementController>();
            WorkspacePlacementInputAdapter adapter = Object.FindFirstObjectByType<WorkspacePlacementInputAdapter>();
            GameObject guide = GameObject.Find("Semi Transparent Workspace Guide");
            GameObject content = FindEvenIfInactive("Placed Robot And Tools Root");
            GameObject xrOrigin = GameObject.Find("XR Origin (Controller MR)");
            GameObject instructionBoard = GameObject.Find("Instruction Board");
            GameObject robotBody = FindEvenIfInactive("Robot Body");

            Assert.IsNotNull(placement);
            Assert.IsNotNull(adapter);
            Assert.IsNotNull(guide);
            Assert.IsTrue(guide.activeInHierarchy);
            Assert.IsNotNull(content);
            Assert.IsFalse(content.activeSelf);
            Assert.IsNotNull(xrOrigin);
            Assert.IsNotNull(instructionBoard);
            Assert.IsNotNull(robotBody);
            Assert.IsNotNull(Object.FindFirstObjectByType<RepairGameMvpController>());
            Assert.AreEqual(6, Object.FindObjectsByType<RepairTool>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length);
            Assert.AreEqual(6, Object.FindObjectsByType<RepairTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length);
            Assert.AreEqual(2, Object.FindObjectsByType<RepairSelectStep>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length);
            Assert.IsEmpty(Object.FindObjectsByType<LocomotionProvider>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        }

        private static GameObject FindEvenIfInactive(string objectName)
        {
            foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.scene.IsValid() && candidate.name == objectName) return candidate;
            }
            return null;
        }
    }
}
