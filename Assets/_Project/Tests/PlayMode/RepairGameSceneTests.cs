using System.Collections;
using MRRobotRepair.Core;
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

            Assert.IsNotNull(placement);
            Assert.IsNotNull(adapter);
            Assert.IsNotNull(guide);
            Assert.IsTrue(guide.activeInHierarchy);
            Assert.IsNotNull(content);
            Assert.IsFalse(content.activeSelf);
            Assert.IsNotNull(xrOrigin);
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
