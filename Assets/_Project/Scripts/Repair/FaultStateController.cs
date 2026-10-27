using System;
using UnityEngine;

namespace MRRobotRepair.Repair
{
    /// <summary>単一故障の発生・修理・リセット状態を管理する。</summary>
    public sealed class FaultStateController : MonoBehaviour
    {
        [SerializeField] private string faultId;
        public string FaultId => faultId;
        public bool IsRepaired { get; private set; }
        public event Action Repaired;

        /// <summary>管理する故障IDを設定する。</summary>
        public void Configure(string id)
        {
            faultId = id;
            IsRepaired = false;
        }

        /// <summary>IDが一致する未修理故障を修理済みにする。</summary>
        public bool TryRepair(string id)
        {
            if (IsRepaired || string.IsNullOrWhiteSpace(faultId) || id != faultId)
            {
                return false;
            }

            IsRepaired = true;
            Repaired?.Invoke();
            return true;
        }

        /// <summary>未修理状態へ戻す。</summary>
        public void ResetFault()
        {
            IsRepaired = false;
        }
    }
}
