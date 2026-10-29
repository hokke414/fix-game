using System;
using MRRobotRepair.Core;
using MRRobotRepair.Scoring;
using MRRobotRepair.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MRRobotRepair.Gameplay
{
    /// <summary>プリミティブ工具で最後まで遊べる最小の修理フローを管理する。</summary>
    public sealed class RepairGameMvpController : MonoBehaviour
    {
        [SerializeField] private WorkspacePlacementController placementController;
        [SerializeField] private TextMesh screenText;
        [SerializeField] private RepairTarget[] targets = Array.Empty<RepairTarget>();
        [SerializeField] private RepairSelectStep[] selectableSteps = Array.Empty<RepairSelectStep>();
        [SerializeField] private RepairTool[] tools = Array.Empty<RepairTool>();
        [SerializeField] private string[] instructions = Array.Empty<string>();
        [SerializeField] private string[] hints = Array.Empty<string>();

        private readonly GuidanceHintTimer hintTimer = new(30f);
        private InputAction restartAction;
        private float startTime;
        private float statusClearTime;
        private bool gameStarted;
        private string currentInstruction = string.Empty;
        private string currentStatus = string.Empty;

        public int CurrentStepIndex { get; private set; }
        public int MistakeCount { get; private set; }
        public bool IsComplete { get; private set; }
        public ScoreResult FinalScore { get; private set; }

        /// <summary>シーン生成時に表示と工程の参照を設定する。</summary>
        public void Configure(
            WorkspacePlacementController placement,
            TextMesh screen,
            RepairTarget[] repairTargets,
            RepairSelectStep[] selectionSteps,
            RepairTool[] repairTools,
            string[] stepInstructions,
            string[] stepHints)
        {
            placementController = placement;
            screenText = screen;
            targets = repairTargets ?? Array.Empty<RepairTarget>();
            selectableSteps = selectionSteps ?? Array.Empty<RepairSelectStep>();
            tools = repairTools ?? Array.Empty<RepairTool>();
            instructions = stepInstructions ?? Array.Empty<string>();
            hints = stepHints ?? Array.Empty<string>();
        }

        private void Awake()
        {
            restartAction = new InputAction("Restart Repair", InputActionType.Button);
            restartAction.AddBinding("<XRController>{RightHand}/secondaryButton");
            restartAction.AddBinding("<XRController>{LeftHand}/secondaryButton");
        }

        private void OnEnable()
        {
            restartAction?.Enable();
            if (placementController == null) return;
            placementController.PlacementConfirmed += OnPlacementConfirmed;
            placementController.PlacementAdjustmentStarted += ShowPlacementInstruction;
        }

        private void OnDisable()
        {
            restartAction?.Disable();
            if (placementController == null) return;
            placementController.PlacementConfirmed -= OnPlacementConfirmed;
            placementController.PlacementAdjustmentStarted -= ShowPlacementInstruction;
        }

        private void OnDestroy() => restartAction?.Dispose();

        private void Start()
        {
            if (placementController != null && placementController.IsPlaced) BeginGame();
            else ShowPlacementInstruction();
        }

        private void Update()
        {
            if (IsComplete && restartAction != null && restartAction.WasPressedThisFrame())
            {
                BeginGame();
                return;
            }

            if (!gameStarted || IsComplete) return;
            if (hintTimer.Tick(Time.unscaledDeltaTime))
            {
                string hint = CurrentStepIndex < hints.Length ? hints[CurrentStepIndex] : "色の合う工具を使ってください";
                SetStatus($"ヒント: {hint}");
                statusClearTime = 0f;
            }
            else if (statusClearTime > 0f && Time.unscaledTime >= statusClearTime)
            {
                SetStatus($"ミス: {MistakeCount}");
                statusClearTime = 0f;
            }
        }

        /// <summary>修理工程を最初から開始する。</summary>
        public void BeginGame()
        {
            gameStarted = true;
            IsComplete = false;
            CurrentStepIndex = 0;
            MistakeCount = 0;
            FinalScore = default;
            startTime = Time.unscaledTime;
            hintTimer.Reset();
            foreach (RepairTarget target in targets) target?.ResetTarget();
            foreach (RepairSelectStep step in selectableSteps) step?.ResetStep();
            foreach (RepairTool tool in tools) tool?.ResetTool();
            ShowCurrentStep();
        }

        /// <summary>現在工程と一致する操作だけを完了として受け付ける。</summary>
        public bool TryCompleteStep(int stepIndex)
        {
            if (!gameStarted || IsComplete || stepIndex != CurrentStepIndex)
            {
                RecordMistake("今の手順と違います");
                return false;
            }

            CurrentStepIndex++;
            hintTimer.Reset();
            if (CurrentStepIndex >= instructions.Length)
            {
                CompleteGame();
                return true;
            }

            ShowCurrentStep();
            return true;
        }

        /// <summary>誤操作を記録し、短い案内を表示する。</summary>
        public void RecordMistake(string message)
        {
            if (!gameStarted || IsComplete) return;
            MistakeCount++;
            SetStatus($"{message}  (ミス: {MistakeCount})");
            statusClearTime = Time.unscaledTime + 2f;
        }

        private void OnPlacementConfirmed(Vector3 position, Quaternion rotation)
        {
            if (!gameStarted) BeginGame();
            else ShowCurrentStep();
        }

        private void ShowPlacementInstruction()
        {
            SetInstruction("左スティック: 移動 / 右スティック: 高さ・回転\n手を作業台へ近づけ、AまたはXで確定");
            SetStatus("周囲の安全と現実の作業台を確認してください");
        }

        private void ShowCurrentStep()
        {
            if (CurrentStepIndex < instructions.Length)
                SetInstruction($"手順 {CurrentStepIndex + 1}/{instructions.Length}\n{instructions[CurrentStepIndex]}");
            SetStatus($"ミス: {MistakeCount}");
        }

        private void CompleteGame()
        {
            IsComplete = true;
            float elapsed = Mathf.Max(0f, Time.unscaledTime - startTime);
            float accuracy = instructions.Length / (float)Mathf.Max(1, instructions.Length + MistakeCount);
            FinalScore = ScoreCalculator.Calculate(new ScoreInput(accuracy, elapsed, 180f, true));
            SetInstruction($"修理完了!  スコア {FinalScore.TotalScore}/100\nBまたはYで、もう一度");
            SetStatus($"時間 {elapsed:0}秒 / ミス {MistakeCount}");
        }

        private void SetInstruction(string value)
        {
            currentInstruction = value;
            RenderScreen();
        }

        private void SetStatus(string value)
        {
            currentStatus = value;
            RenderScreen();
        }

        private void RenderScreen()
        {
            if (screenText == null) return;
            screenText.text = $"{currentInstruction}\n\n{currentStatus}";
        }
    }
}
