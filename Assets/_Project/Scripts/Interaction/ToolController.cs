using System;
using UnityEngine;

namespace MRRobotRepair.Interaction
{
    /// <summary>工具IDと対象IDを照合し、正しい用途だけを通知する。</summary>
    public sealed class ToolController : MonoBehaviour
    {
        [SerializeField] private string toolId;
        [SerializeField] private string validTargetId;

        public string ToolId => toolId;
        public event Action<string> ValidUse;
        public event Action<string> InvalidUse;

        /// <summary>工具IDと正しい対象IDを設定する。</summary>
        public void Configure(string id, string targetId)
        {
            toolId = id;
            validTargetId = targetId;
        }

        /// <summary>正しい対象だけ成功として通知する。</summary>
        public bool TryUse(string targetId)
        {
            if (!string.IsNullOrWhiteSpace(toolId) && targetId == validTargetId)
            {
                ValidUse?.Invoke(targetId);
                return true;
            }

            InvalidUse?.Invoke(targetId);
            return false;
        }
    }
}
