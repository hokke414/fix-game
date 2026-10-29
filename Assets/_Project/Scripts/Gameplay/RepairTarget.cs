using UnityEngine;

namespace MRRobotRepair.Gameplay
{
    /// <summary>指定された工具との接触で対応する修理手順を完了する。</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class RepairTarget : MonoBehaviour
    {
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        [SerializeField] private RepairGameMvpController controller;
        [SerializeField] private int stepIndex;
        [SerializeField] private string requiredToolId;
        [SerializeField] private Renderer feedbackRenderer;
        [SerializeField] private Color completedColor = new(0.1f, 0.85f, 0.45f, 1f);

        private MaterialPropertyBlock propertyBlock;
        private Color initialColor = Color.white;
        private float nextAttemptTime;
        private Collider triggerCollider;

        public int StepIndex => stepIndex;
        public string RequiredToolId => requiredToolId;

        /// <summary>工程、工具、表示対象を設定する。</summary>
        public void Configure(
            RepairGameMvpController gameController,
            int index,
            string toolId,
            Renderer renderer)
        {
            controller = gameController;
            stepIndex = index;
            requiredToolId = toolId;
            feedbackRenderer = renderer;
        }

        private void Awake()
        {
            triggerCollider = GetComponent<Collider>();
            triggerCollider.isTrigger = true;
            if (feedbackRenderer != null && feedbackRenderer.sharedMaterial != null)
                initialColor = feedbackRenderer.sharedMaterial.color;
            SetColor(initialColor);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Time.unscaledTime < nextAttemptTime || controller == null) return;
            RepairTool tool = other.GetComponentInParent<RepairTool>();
            if (tool == null) return;

            nextAttemptTime = Time.unscaledTime + 0.5f;
            if (tool.ToolId != requiredToolId)
            {
                controller.ShowTemporaryMessage("違う工具です");
                return;
            }

            if (!controller.TryCompleteStep(stepIndex)) return;
            SetColor(completedColor);
            if (triggerCollider != null) triggerCollider.enabled = false;
        }

        /// <summary>工程を未完了表示へ戻す。</summary>
        public void ResetTarget()
        {
            nextAttemptTime = 0f;
            if (triggerCollider == null) triggerCollider = GetComponent<Collider>();
            triggerCollider.enabled = true;
            triggerCollider.isTrigger = true;
            SetColor(initialColor);
        }

        private void SetColor(Color color)
        {
            if (feedbackRenderer == null) return;
            propertyBlock ??= new MaterialPropertyBlock();
            feedbackRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorProperty, color);
            propertyBlock.SetColor(ColorProperty, color);
            feedbackRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
