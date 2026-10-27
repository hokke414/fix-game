using System;
using UnityEngine;

namespace MRRobotRepair.Interaction
{
    /// <summary>部品を記録済みの初期Transformへ安全に戻す。</summary>
    public sealed class ResettableObject : MonoBehaviour
    {
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private bool recorded;
        private Rigidbody cachedRigidbody;

        public event Action ResetCompleted;

        private void Awake()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            RecordInitialTransform();
        }

        /// <summary>現在位置を復帰先として記録する。</summary>
        public void RecordInitialTransform()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            recorded = true;
        }

        /// <summary>位置、回転、物理速度を初期状態へ戻す。</summary>
        public bool ResetObject()
        {
            if (!recorded)
            {
                return false;
            }

            transform.SetPositionAndRotation(initialPosition, initialRotation);
            if (cachedRigidbody != null)
            {
                cachedRigidbody.linearVelocity = Vector3.zero;
                cachedRigidbody.angularVelocity = Vector3.zero;
            }

            ResetCompleted?.Invoke();
            return true;
        }
    }
}
