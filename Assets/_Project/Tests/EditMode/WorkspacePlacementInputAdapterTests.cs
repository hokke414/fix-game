using MRRobotRepair.Core;
using MRRobotRepair.MR;
using NUnit.Framework;
using UnityEngine;

namespace MRRobotRepair.Tests
{
    public sealed class WorkspacePlacementInputAdapterTests
    {
        private GameObject container;
        private GameObject workspace;
        private GameObject guideRoot;
        private GameObject contentRoot;
        private GameObject viewer;
        private GameObject controller;
        private WorkspacePlacementController placement;
        private WorkspacePlacementInputAdapter adapter;

        [SetUp]
        public void SetUp()
        {
            container = new GameObject("Placement Test Root");
            workspace = new GameObject("Workspace");
            workspace.transform.SetParent(container.transform);
            placement = workspace.AddComponent<WorkspacePlacementController>();

            guideRoot = new GameObject("Guide");
            guideRoot.transform.SetParent(workspace.transform, false);
            BoxCollider bounds = guideRoot.AddComponent<BoxCollider>();
            bounds.size = new Vector3(0.8f, 0.75f, 0.5f);

            contentRoot = new GameObject("Content");
            contentRoot.transform.SetParent(workspace.transform, false);

            viewer = new GameObject("Viewer");
            viewer.transform.SetParent(container.transform);
            viewer.transform.position = new Vector3(0f, 1.65f, 0f);
            viewer.transform.forward = Vector3.forward;

            controller = new GameObject("Controller");
            controller.transform.SetParent(container.transform);

            WorkspaceReachabilityChecker reachability = workspace.AddComponent<WorkspaceReachabilityChecker>();
            reachability.Configure(bounds, controller.transform, null);
            WorkspaceGuideView guide = workspace.AddComponent<WorkspaceGuideView>();
            guide.Configure(placement, guideRoot, System.Array.Empty<Renderer>());
            adapter = workspace.AddComponent<WorkspacePlacementInputAdapter>();
            adapter.Configure(placement, reachability, guide, viewer.transform, contentRoot);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(container);

        [Test]
        public void InitializePlacement_ShowsGuideAndHidesPlacedContent()
        {
            Assert.IsTrue(adapter.InitializePlacement());
            Assert.IsTrue(guideRoot.activeSelf);
            Assert.IsFalse(contentRoot.activeSelf);
            Assert.AreEqual(1.25f, workspace.transform.position.z, 0.0001f);
            Assert.AreEqual(0.9f, workspace.transform.position.y, 0.0001f);
        }

        [Test]
        public void ProcessAdjustmentInput_MapsRightStickToHeightAndYaw()
        {
            adapter.InitializePlacement();
            float initialHeight = workspace.transform.position.y;

            Assert.IsTrue(adapter.ProcessAdjustmentInput(Vector2.zero, new Vector2(1f, 1f), 1f));

            Assert.AreEqual(initialHeight + 0.35f, workspace.transform.position.y, 0.0001f);
            Assert.AreEqual(60f, workspace.transform.rotation.eulerAngles.y, 0.001f);
        }

        [Test]
        public void TryConfirmPlacement_WhenControllerTouchesGuide_ShowsContent()
        {
            adapter.InitializePlacement();
            controller.transform.position = guideRoot.transform.position;

            Assert.IsTrue(adapter.TryConfirmPlacement());
            Assert.IsTrue(placement.IsPlaced);
            Assert.IsFalse(guideRoot.activeSelf);
            Assert.IsTrue(contentRoot.activeSelf);
        }

        [Test]
        public void TryConfirmPlacement_WhenControllerIsFar_KeepsGuideVisible()
        {
            adapter.InitializePlacement();
            controller.transform.position = Vector3.one * 10f;

            Assert.IsFalse(adapter.TryConfirmPlacement());
            Assert.IsFalse(placement.IsPlaced);
            Assert.IsTrue(guideRoot.activeSelf);
            Assert.IsFalse(contentRoot.activeSelf);
        }

        [Test]
        public void BeginAdjustment_AfterConfirmationRestoresGuideAndHidesContent()
        {
            adapter.InitializePlacement();
            controller.transform.position = guideRoot.transform.position;
            adapter.TryConfirmPlacement();

            Assert.IsTrue(adapter.BeginAdjustment());
            Assert.IsFalse(placement.IsPlaced);
            Assert.IsTrue(guideRoot.activeSelf);
            Assert.IsFalse(contentRoot.activeSelf);
        }
    }
}
