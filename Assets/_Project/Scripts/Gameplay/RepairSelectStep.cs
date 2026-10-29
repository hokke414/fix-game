using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MRRobotRepair.Gameplay
{
    /// <summary>電源スイッチなど、直接選択する修理手順。</summary>
    public sealed class RepairSelectStep : MonoBehaviour
    {
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        [SerializeField] private RepairGameMvpController controller;
        [SerializeField] private int stepIndex;
        [SerializeField] private XRBaseInteractable interactable;
        [SerializeField] private Renderer feedbackRenderer;
        [SerializeField] private Color completedColor = new(0.1f, 0.85f, 0.45f, 1f);

        private MaterialPropertyBlock propertyBlock;
        private Color initialColor = Color.white;

        public int StepIndex => stepIndex;

        private void Awake()
        {
            CaptureInitialColor();
        }

        /// <summary>工程とXRI操作対象を設定する。</summary>
        public void Configure(
            RepairGameMvpController gameController,
            int index,
            XRBaseInteractable repairInteractable,
            Renderer renderer)
        {
            controller = gameController;
            stepIndex = index;
            interactable = repairInteractable;
            feedbackRenderer = renderer;
            CaptureInitialColor();
        }

        private void OnEnable()
        {
            if (interactable != null) interactable.selectEntered.AddListener(OnSelected);
        }

        private void OnDisable()
        {
            if (interactable != null) interactable.selectEntered.RemoveListener(OnSelected);
        }

        /// <summary>選択工程を未完了表示へ戻す。</summary>
        public void ResetStep() => SetColor(initialColor);

        private void CaptureInitialColor()
        {
            if (feedbackRenderer != null && feedbackRenderer.sharedMaterial != null)
                initialColor = feedbackRenderer.sharedMaterial.color;
        }

        private void OnSelected(SelectEnterEventArgs args)
        {
            if (controller != null && controller.TryCompleteStep(stepIndex)) SetColor(completedColor);
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
