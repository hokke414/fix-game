using UnityEngine;

namespace MRRobotRepair.Gameplay
{
    /// <summary>修理工程で使用する把持可能な工具または交換部品。</summary>
    public sealed class RepairTool : MonoBehaviour
    {
        [SerializeField] private string toolId;
        private Vector3 initialLocalPosition;
        private Quaternion initialLocalRotation;
        private Rigidbody cachedRigidbody;
        private bool recorded;

        public string ToolId => toolId;

        /// <summary>工具IDを設定する。</summary>
        public void Configure(string id) => toolId = id;

        private void Awake()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            RecordInitialTransform();
        }

        /// <summary>現在のローカル位置を再開始時の復帰先として記録する。</summary>
        public void RecordInitialTransform()
        {
            initialLocalPosition = transform.localPosition;
            initialLocalRotation = transform.localRotation;
            recorded = true;
        }

        /// <summary>工具をトレー上の初期位置へ戻す。</summary>
        public void ResetTool()
        {
            if (!recorded) RecordInitialTransform();
            transform.SetLocalPositionAndRotation(initialLocalPosition, initialLocalRotation);
            if (cachedRigidbody == null) return;
            cachedRigidbody.linearVelocity = Vector3.zero;
            cachedRigidbody.angularVelocity = Vector3.zero;
        }
    }
}
