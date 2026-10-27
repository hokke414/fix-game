using System;
using UnityEngine;

namespace MRRobotRepair.Core
{
    /// <summary>MR空間における作業台の位置・回転・確定状態を管理する。</summary>
    public sealed class WorkspacePlacementController : MonoBehaviour
    {
        private const float MinimumForwardMagnitudeSquared = 0.0001f;

        [SerializeField] private Vector3 defaultPosition = Vector3.zero;
        [SerializeField] private Quaternion defaultRotation = Quaternion.identity;
        [SerializeField, Range(1f, 1.5f)] private float initialDistanceMeters = 1.25f;
        [SerializeField, Min(0f)] private float translationSpeedMetersPerSecond = 0.5f;
        [SerializeField, Min(0f)] private float heightSpeedMetersPerSecond = 0.35f;
        [SerializeField, Min(0f)] private float rotationSpeedDegreesPerSecond = 60f;

        public bool IsPlaced { get; private set; }
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
        public float InitialDistanceMeters => initialDistanceMeters;

        public event Action<Vector3, Quaternion> PlacementConfirmed;
        public event Action PlacementConfirmationRejected;
        public event Action PlacementAdjustmentStarted;
        public event Action<Vector3, Quaternion> PlacementChanged;
        public event Action PlacementReset;

        private void Awake()
        {
            SetPositionAndRotation(defaultPosition, defaultRotation);
        }

        /// <summary>プレイヤーの水平前方へ作業台を初期配置する。</summary>
        public bool InitializeInFrontOf(Vector3 viewerPosition, Vector3 viewerForward, float workspaceHeight)
        {
            Vector3 horizontalForward = Vector3.ProjectOnPlane(viewerForward, Vector3.up);
            if (horizontalForward.sqrMagnitude < MinimumForwardMagnitudeSquared)
            {
                return false;
            }

            horizontalForward.Normalize();
            Vector3 targetPosition = viewerPosition + horizontalForward * initialDistanceMeters;
            targetPosition.y = workspaceHeight;
            IsPlaced = false;
            SetPositionAndRotation(targetPosition, Quaternion.LookRotation(horizontalForward, Vector3.up));
            return true;
        }

        /// <summary>視点基準の水平移動、高さ、水平回転入力を適用する。</summary>
        public bool AdjustPlacement(
            Vector2 planarInput,
            float heightInput,
            float yawInput,
            Vector3 viewerForward,
            float deltaTime)
        {
            if (IsPlaced || deltaTime <= 0f)
            {
                return false;
            }

            Vector3 horizontalForward = Vector3.ProjectOnPlane(viewerForward, Vector3.up);
            if (horizontalForward.sqrMagnitude < MinimumForwardMagnitudeSquared)
            {
                return false;
            }

            horizontalForward.Normalize();
            Vector3 horizontalRight = Vector3.Cross(Vector3.up, horizontalForward);
            Vector2 limitedPlanarInput = Vector2.ClampMagnitude(planarInput, 1f);
            Vector3 horizontalDelta =
                (horizontalRight * limitedPlanarInput.x + horizontalForward * limitedPlanarInput.y)
                * translationSpeedMetersPerSecond * deltaTime;
            Vector3 verticalDelta = Vector3.up * Mathf.Clamp(heightInput, -1f, 1f)
                * heightSpeedMetersPerSecond * deltaTime;
            float yawDelta = Mathf.Clamp(yawInput, -1f, 1f)
                * rotationSpeedDegreesPerSecond * deltaTime;

            transform.position += horizontalDelta + verticalDelta;
            transform.Rotate(Vector3.up, yawDelta, Space.World);
            PlacementChanged?.Invoke(Position, Rotation);
            return true;
        }

        /// <summary>作業台の位置と回転を更新する。</summary>
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            PlacementChanged?.Invoke(Position, Rotation);
        }

        /// <summary>到達可能範囲の確認結果を受け取り、配置を確定する。</summary>
        public bool ConfirmPlacement(bool isWithinReach)
        {
            if (IsPlaced)
            {
                return false;
            }

            if (!isWithinReach)
            {
                PlacementConfirmationRejected?.Invoke();
                return false;
            }

            IsPlaced = true;
            PlacementConfirmed?.Invoke(Position, Rotation);
            return true;
        }

        /// <summary>確定済みの作業台を現在位置から再調整できる状態へ戻す。</summary>
        public bool BeginAdjustment()
        {
            if (!IsPlaced)
            {
                return false;
            }

            IsPlaced = false;
            PlacementAdjustmentStarted?.Invoke();
            return true;
        }

        /// <summary>確定状態を解除し、初期位置へ戻す。</summary>
        public void ResetPlacement()
        {
            IsPlaced = false;
            SetPositionAndRotation(defaultPosition, defaultRotation);
            PlacementReset?.Invoke();
        }
    }
}
