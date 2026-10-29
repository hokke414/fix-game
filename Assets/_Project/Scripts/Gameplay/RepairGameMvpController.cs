using System;
using MRRobotRepair.Core;
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
        private float messageClearTime;
        private bool gameStarted;
        private int lastDisplayedSecond = -1;
        private string currentInstruction = string.Empty;
        private string currentStatus = string.Empty;
        private string currentMessage = string.Empty;
        private Vector3[] toolSlotPositions = Array.Empty<Vector3>();
        private int[] lastToolOrder = Array.Empty<int>();

        public int CurrentStepIndex { get; private set; }
        public bool IsComplete { get; private set; }
        public float ElapsedSeconds { get; private set; }
        public int FinalScore { get; private set; }

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
            ElapsedSeconds = Mathf.Max(0f, Time.unscaledTime - startTime);
            int displayedSecond = Mathf.FloorToInt(ElapsedSeconds);
            if (displayedSecond != lastDisplayedSecond)
            {
                lastDisplayedSecond = displayedSecond;
                RefreshStatus();
            }

            if (hintTimer.Tick(Time.unscaledDeltaTime))
            {
                string hint = CurrentStepIndex < hints.Length ? hints[CurrentStepIndex] : "色と形の合う工具を使ってください";
                currentMessage = $"ヒント: {hint}";
                messageClearTime = 0f;
                RefreshStatus();
            }
            else if (messageClearTime > 0f && Time.unscaledTime >= messageClearTime)
            {
                currentMessage = string.Empty;
                messageClearTime = 0f;
                RefreshStatus();
            }
        }

        /// <summary>修理工程を最初から開始する。</summary>
        public void BeginGame()
        {
            gameStarted = true;
            IsComplete = false;
            CurrentStepIndex = 0;
            ElapsedSeconds = 0f;
            FinalScore = 0;
            startTime = Time.unscaledTime;
            lastDisplayedSecond = -1;
            currentMessage = string.Empty;
            messageClearTime = 0f;
            hintTimer.Reset();
            foreach (RepairTarget target in targets) target?.ResetTarget();
            foreach (RepairSelectStep step in selectableSteps) step?.ResetStep();
            CaptureToolSlots();
            foreach (RepairTool tool in tools) tool?.ResetTool();
            ShuffleToolPositions();
            ShowCurrentStep();
        }

        /// <summary>現在工程と一致する操作だけを完了として受け付ける。</summary>
        public bool TryCompleteStep(int stepIndex)
        {
            if (!gameStarted || IsComplete || stepIndex != CurrentStepIndex)
            {
                ShowTemporaryMessage("今の手順と違います");
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

        /// <summary>スコアへ影響しない短い操作案内を表示する。</summary>
        public void ShowTemporaryMessage(string message)
        {
            if (!gameStarted || IsComplete) return;
            currentMessage = message;
            messageClearTime = Time.unscaledTime + 2f;
            RefreshStatus();
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
            currentMessage = string.Empty;
            RefreshStatus();
        }

        private void CompleteGame()
        {
            IsComplete = true;
            ElapsedSeconds = Mathf.Max(0f, Time.unscaledTime - startTime);
            FinalScore = CalculateTimeScore(ElapsedSeconds);
            SetInstruction($"修理完了!  スコア {FinalScore}/100\nBまたはYで、もう一度");
            SetStatus($"クリアタイム: {FormatTime(ElapsedSeconds)}");
        }

        /// <summary>クリア時間だけから100点満点のスコアを計算する。5秒ごとに1点減点する。</summary>
        public static int CalculateTimeScore(float elapsedSeconds) =>
            Mathf.Clamp(100 - Mathf.FloorToInt(Mathf.Max(0f, elapsedSeconds) / 5f), 0, 100);

        private void RefreshStatus()
        {
            string timer = $"タイム: {FormatTime(ElapsedSeconds)}";
            SetStatus(string.IsNullOrEmpty(currentMessage) ? timer : $"{timer}\n{currentMessage}");
        }

        private static string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }

        private void CaptureToolSlots()
        {
            if (toolSlotPositions.Length == tools.Length) return;
            toolSlotPositions = new Vector3[tools.Length];
            lastToolOrder = new int[tools.Length];
            for (int index = 0; index < tools.Length; index++)
            {
                if (tools[index] != null)
                {
                    toolSlotPositions[index] = tools[index].transform.localPosition;
                }
                lastToolOrder[index] = index;
            }
        }

        private void ShuffleToolPositions()
        {
            if (tools.Length < 2) return;
            int[] order = new int[tools.Length];
            for (int index = 0; index < order.Length; index++) order[index] = index;
            for (int index = order.Length - 1; index > 0; index--)
            {
                int swapIndex = UnityEngine.Random.Range(0, index + 1);
                (order[index], order[swapIndex]) = (order[swapIndex], order[index]);
            }

            bool sameAsLast = true;
            for (int index = 0; index < order.Length; index++)
                sameAsLast &= order[index] == lastToolOrder[index];
            if (sameAsLast) (order[0], order[1]) = (order[1], order[0]);

            for (int index = 0; index < tools.Length; index++)
            {
                RepairTool tool = tools[index];
                if (tool == null) continue;
                int slot = order[index];
                tool.transform.localPosition = toolSlotPositions[slot];
            }
            lastToolOrder = order;
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
