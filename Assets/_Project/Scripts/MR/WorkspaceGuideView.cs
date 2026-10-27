using MRRobotRepair.Core;
using UnityEngine;

namespace MRRobotRepair.MR
{
    /// <summary>作業台配置ガイドの表示と到達可否色を管理する。</summary>
    public sealed class WorkspaceGuideView : MonoBehaviour
    {
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        [SerializeField] private WorkspacePlacementController placementController;
        [SerializeField] private GameObject guideRoot;
        [SerializeField] private Renderer[] guideRenderers = System.Array.Empty<Renderer>();
        [SerializeField] private Color reachableColor = new(0.1f, 0.85f, 0.45f, 0.35f);
        [SerializeField] private Color unreachableColor = new(1f, 0.65f, 0.1f, 0.3f);
        private MaterialPropertyBlock propertyBlock;
        private bool subscribed;

        /// <summary>表示参照を設定する。</summary>
        public void Configure(WorkspacePlacementController controller, GameObject root, Renderer[] renderers)
        {
            Unsubscribe();
            placementController = controller;
            guideRoot = root;
            guideRenderers = renderers ?? System.Array.Empty<Renderer>();
            Subscribe();
        }

        /// <summary>到達可能なら緑、未到達なら橙へ更新する。</summary>
        public void SetReachable(bool isReachable)
        {
            propertyBlock ??= new MaterialPropertyBlock();
            Color color = isReachable ? reachableColor : unreachableColor;
            foreach (Renderer renderer in guideRenderers)
            {
                if (renderer == null) continue;
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorProperty, color);
                propertyBlock.SetColor(ColorProperty, color);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        public void Show() { if (guideRoot != null) guideRoot.SetActive(true); }
        public void Hide() { if (guideRoot != null) guideRoot.SetActive(false); }

        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();
        private void Subscribe()
        {
            if (subscribed || placementController == null) return;
            placementController.PlacementConfirmed += OnConfirmed;
            placementController.PlacementAdjustmentStarted += Show;
            placementController.PlacementReset += Show;
            subscribed = true;
        }
        private void Unsubscribe()
        {
            if (!subscribed || placementController == null) return;
            placementController.PlacementConfirmed -= OnConfirmed;
            placementController.PlacementAdjustmentStarted -= Show;
            placementController.PlacementReset -= Show;
            subscribed = false;
        }
        private void OnConfirmed(Vector3 position, Quaternion rotation) => Hide();
    }
}
