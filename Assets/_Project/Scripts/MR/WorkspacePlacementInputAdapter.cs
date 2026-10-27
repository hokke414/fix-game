using MRRobotRepair.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MRRobotRepair.MR
{
    /// <summary>OpenXRコントローラー入力を作業台配置のCore APIへ変換する。</summary>
    public sealed class WorkspacePlacementInputAdapter : MonoBehaviour
    {
        [SerializeField] private WorkspacePlacementController placementController;
        [SerializeField] private WorkspaceReachabilityChecker reachabilityChecker;
        [SerializeField] private WorkspaceGuideView guideView;
        [SerializeField] private Transform viewer;
        [SerializeField] private GameObject placedContentRoot;
        [SerializeField] private float initialHeightOffsetMeters = -0.75f;

        private InputAction moveAction;
        private InputAction heightAndYawAction;
        private InputAction confirmAction;
        private InputAction readjustAction;

        /// <summary>シーン参照を設定する。</summary>
        public void Configure(
            WorkspacePlacementController controller,
            WorkspaceReachabilityChecker reachability,
            WorkspaceGuideView guide,
            Transform viewerTransform,
            GameObject contentRoot)
        {
            placementController = controller;
            reachabilityChecker = reachability;
            guideView = guide;
            viewer = viewerTransform;
            placedContentRoot = contentRoot;
        }

        /// <summary>現在の視点を基準に初期配置する。</summary>
        public bool InitializePlacement()
        {
            if (placementController == null || viewer == null) return false;
            bool initialized = placementController.InitializeInFrontOf(
                viewer.position, viewer.forward, viewer.position.y + initialHeightOffsetMeters);
            if (!initialized) return false;
            guideView?.Show();
            if (placedContentRoot != null) placedContentRoot.SetActive(false);
            return true;
        }

        /// <summary>1フレーム分のスティック入力を適用する。</summary>
        public bool ProcessAdjustmentInput(Vector2 planarInput, Vector2 heightAndYawInput, float deltaTime)
        {
            if (placementController == null || viewer == null) return false;
            return placementController.AdjustPlacement(
                planarInput, heightAndYawInput.y, heightAndYawInput.x, viewer.forward, deltaTime);
        }

        /// <summary>現在の到達判定で配置確定を試みる。</summary>
        public bool TryConfirmPlacement()
        {
            if (placementController == null || reachabilityChecker == null) return false;
            bool confirmed = placementController.ConfirmPlacement(reachabilityChecker.IsWithinReach());
            if (confirmed && placedContentRoot != null) placedContentRoot.SetActive(true);
            return confirmed;
        }

        /// <summary>確定済み作業台を現在位置から再調整する。</summary>
        public bool BeginAdjustment()
        {
            if (placementController == null || !placementController.BeginAdjustment()) return false;
            if (placedContentRoot != null) placedContentRoot.SetActive(false);
            return true;
        }

        private void Awake() => CreateInputActions();
        private void Start()
        {
            if (!InitializePlacement())
            {
                Debug.LogError($"{nameof(WorkspacePlacementInputAdapter)} on '{name}' could not initialize.", this);
            }
        }
        private void OnEnable() => SetInputActionsEnabled(true);
        private void OnDisable() => SetInputActionsEnabled(false);
        private void OnDestroy()
        {
            moveAction?.Dispose();
            heightAndYawAction?.Dispose();
            confirmAction?.Dispose();
            readjustAction?.Dispose();
        }
        private void Update()
        {
            if (placementController == null || viewer == null || moveAction == null) return;
            bool reachable = reachabilityChecker != null && reachabilityChecker.IsWithinReach();
            guideView?.SetReachable(reachable);
            if (!placementController.IsPlaced)
            {
                ProcessAdjustmentInput(moveAction.ReadValue<Vector2>(), heightAndYawAction.ReadValue<Vector2>(), Time.unscaledDeltaTime);
                if (confirmAction.WasPressedThisFrame()) TryConfirmPlacement();
            }
            else if (readjustAction.WasPressedThisFrame())
            {
                BeginAdjustment();
            }
        }
        private void CreateInputActions()
        {
            moveAction = new InputAction("Workspace Move", InputActionType.Value,
                "<XRController>{LeftHand}/thumbstick", expectedControlType: "Vector2");
            heightAndYawAction = new InputAction("Workspace Height And Yaw", InputActionType.Value,
                "<XRController>{RightHand}/thumbstick", expectedControlType: "Vector2");
            confirmAction = new InputAction("Confirm Workspace", InputActionType.Button);
            confirmAction.AddBinding("<XRController>{RightHand}/primaryButton");
            confirmAction.AddBinding("<XRController>{LeftHand}/primaryButton");
            readjustAction = new InputAction("Readjust Workspace", InputActionType.Button,
                "<XRController>{LeftHand}/menuButton");
        }
        private void SetInputActionsEnabled(bool enabled)
        {
            if (moveAction == null) return;
            if (enabled)
            {
                moveAction.Enable(); heightAndYawAction.Enable(); confirmAction.Enable(); readjustAction.Enable();
            }
            else
            {
                moveAction.Disable(); heightAndYawAction.Disable(); confirmAction.Disable(); readjustAction.Disable();
            }
        }
    }
}
