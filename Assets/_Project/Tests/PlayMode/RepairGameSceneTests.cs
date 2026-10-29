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

        [UnityTest]
        public IEnumerator RepairGameScene_ResetKeepsEachMachinePartColor()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("RepairGame", LoadSceneMode.Single);
            while (!load.isDone) yield return null;

            GameObject content = FindEvenIfInactive("Placed Robot And Tools Root");
            Assert.IsNotNull(content);
            content.SetActive(true);
            yield return null;

            RepairSelectStep powerStep = GameObject.Find("Power Switch").GetComponent<RepairSelectStep>();
            RepairTarget purpleTarget = GameObject.Find("Wire Diagnostic").GetComponent<RepairTarget>();
            RepairTarget pinkTarget = GameObject.Find("Fuse Slot").GetComponent<RepairTarget>();

            powerStep.ResetStep();
            purpleTarget.ResetTarget();
            pinkTarget.ResetTarget();

            AssertColor(GameObject.Find("Power Switch").GetComponent<Renderer>(), new Color(0.9f, 0.12f, 0.1f));
            AssertColor(GameObject.Find("Wire Diagnostic").GetComponent<Renderer>(), new Color(0.45f, 0.16f, 0.85f));
            AssertColor(GameObject.Find("Fuse Slot").GetComponent<Renderer>(), new Color(1f, 0.18f, 0.55f));
        }

        private static void AssertColor(Renderer renderer, Color expected)
        {
            MaterialPropertyBlock block = new();
            renderer.GetPropertyBlock(block);
            Color actual = block.GetColor(Shader.PropertyToID("_BaseColor"));
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.01f));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.01f));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.01f));
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
