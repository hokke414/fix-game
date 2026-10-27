using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MRRobotRepair.Repair
{
    /// <summary>修理工程の設定検証、前提条件、順序、完了通知を一元管理する。</summary>
    public sealed class RepairFlowController : MonoBehaviour
    {
        [SerializeField] private List<RepairStepDefinition> steps = new();
        private readonly HashSet<string> completedStepIds = new();

        public int CurrentStepIndex { get; private set; }
        public RepairStepDefinition CurrentStep =>
            CurrentStepIndex < steps.Count ? steps[CurrentStepIndex] : null;
        public IReadOnlyCollection<string> CompletedStepIds => completedStepIds;

        public event Action<RepairStepDefinition> StepCompleted;
        public event Action AllStepsCompleted;

        /// <summary>工程一覧を設定し、設定を検証して先頭へ戻す。</summary>
        public bool Configure(IEnumerable<RepairStepDefinition> definitions)
        {
            steps = definitions?.ToList() ?? new List<RepairStepDefinition>();
            if (!ValidateConfiguration(out _))
            {
                return false;
            }

            ResetFlow();
            return true;
        }

        /// <summary>工程ID、重複、前提参照、定義順を検証する。</summary>
        public bool ValidateConfiguration(out string error)
        {
            error = null;
            var known = new HashSet<string>();
            foreach (RepairStepDefinition step in steps)
            {
                if (step == null || string.IsNullOrWhiteSpace(step.Id))
                {
                    error = "Repair step or its id is missing.";
                    return false;
                }

                if (!known.Add(step.Id))
                {
                    error = $"Duplicate repair step id: {step.Id}";
                    return false;
                }

                foreach (string prerequisite in step.PrerequisiteStepIds)
                {
                    if (!known.Contains(prerequisite))
                    {
                        error = $"Step '{step.Id}' has an unknown or later prerequisite '{prerequisite}'.";
                        return false;
                    }
                }
            }

            return steps.Count > 0;
        }

        /// <summary>現在工程と条件が一致する場合だけ完了させる。</summary>
        public bool TryCompleteStep(string stepId, RepairCompletionCondition condition)
        {
            RepairStepDefinition current = CurrentStep;
            if (current == null || current.Id != stepId || current.CompletionCondition != condition)
            {
                return false;
            }

            if (current.PrerequisiteStepIds.Any(id => !completedStepIds.Contains(id)))
            {
                return false;
            }

            completedStepIds.Add(current.Id);
            CurrentStepIndex++;
            StepCompleted?.Invoke(current);
            if (CurrentStepIndex >= steps.Count)
            {
                AllStepsCompleted?.Invoke();
            }

            return true;
        }

        /// <summary>完了状態を消去して先頭工程へ戻す。</summary>
        public void ResetFlow()
        {
            completedStepIds.Clear();
            CurrentStepIndex = 0;
        }
    }
}
