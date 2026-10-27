using System;
using UnityEngine;

namespace MRRobotRepair.Interaction
{
    /// <summary>部品ID、位置、角度が許容範囲内の場合だけ吸着させる。</summary>
    public sealed class SnapZone : MonoBehaviour
    {
        [SerializeField] private string acceptedPartId;
        [SerializeField] private Transform snapTarget;
        [SerializeField, Min(0f)] private float positionToleranceMeters = 0.08f;
        [SerializeField, Range(0f, 180f)] private float angleToleranceDegrees = 15f;

        public event Action<Transform> Snapped;

        /// <summary>EditorまたはテストからSnap設定を指定する。</summary>
        public void Configure(string partId, Transform target, float positionTolerance, float angleTolerance)
        {
            acceptedPartId = partId;
            snapTarget = target;
            positionToleranceMeters = Mathf.Max(0f, positionTolerance);
            angleToleranceDegrees = Mathf.Clamp(angleTolerance, 0f, 180f);
        }

        /// <summary>候補が正しいID、位置、角度を満たすか返す。</summary>
        public bool CanSnap(string partId, Transform candidate)
        {
            if (candidate == null || snapTarget == null || partId != acceptedPartId)
            {
                return false;
            }

            return Vector3.Distance(candidate.position, snapTarget.position) <= positionToleranceMeters
                && Quaternion.Angle(candidate.rotation, snapTarget.rotation) <= angleToleranceDegrees;
        }

        /// <summary>条件を満たす部品を吸着位置へ移動する。</summary>
        public bool TrySnap(string partId, Transform candidate)
        {
            if (!CanSnap(partId, candidate))
            {
                return false;
            }

            candidate.SetPositionAndRotation(snapTarget.position, snapTarget.rotation);
            Snapped?.Invoke(candidate);
            return true;
        }
    }
}
