using System.Collections.Generic;
using UnityEngine;

namespace MRRobotRepair.Repair
{
    /// <summary>データ駆動の修理工程定義。</summary>
    [CreateAssetMenu(menuName = "MR Robot Repair/Repair Step")]
    public sealed class RepairStepDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string title;
        [SerializeField, TextArea] private string instructionText;
        [SerializeField, TextArea] private string hintText;
        [SerializeField] private string requiredToolId;
        [SerializeField] private string targetObjectId;
        [SerializeField] private List<string> prerequisiteStepIds = new();
        [SerializeField] private RepairCompletionCondition completionCondition;
        [SerializeField, Min(0f)] private float scoreWeight = 1f;
        [SerializeField, Min(0f)] private float timeTargetSeconds = 60f;

        public string Id => id;
        public string Title => title;
        public string InstructionText => instructionText;
        public string HintText => hintText;
        public string RequiredToolId => requiredToolId;
        public string TargetObjectId => targetObjectId;
        public IReadOnlyList<string> PrerequisiteStepIds => prerequisiteStepIds;
        public RepairCompletionCondition CompletionCondition => completionCondition;
        public float ScoreWeight => scoreWeight;
        public float TimeTargetSeconds => timeTargetSeconds;

        /// <summary>テストまたはEditor生成処理から定義値を設定する。</summary>
        public void Initialize(
            string stepId,
            string stepTitle,
            IEnumerable<string> prerequisites = null,
            RepairCompletionCondition condition = RepairCompletionCondition.Interaction)
        {
            id = stepId;
            title = stepTitle;
            prerequisiteStepIds = prerequisites == null
                ? new List<string>()
                : new List<string>(prerequisites);
            completionCondition = condition;
        }
    }
}
