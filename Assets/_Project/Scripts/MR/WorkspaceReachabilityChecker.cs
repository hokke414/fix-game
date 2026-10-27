using UnityEngine;

namespace MRRobotRepair.MR
{
    /// <summary>XRコントローラーとガイドの距離から到達可能性を判定する。</summary>
    public sealed class WorkspaceReachabilityChecker : MonoBehaviour
    {
        [SerializeField] private Collider workspaceBounds;
        [SerializeField] private Transform leftController;
        [SerializeField] private Transform rightController;
        [SerializeField, Min(0f)] private float maximumDistanceMeters = 0.2f;

        public float MaximumDistanceMeters => maximumDistanceMeters;

        /// <summary>判定参照を設定する。</summary>
        public void Configure(Collider bounds, Transform left, Transform right)
        {
            workspaceBounds = bounds;
            leftController = left;
            rightController = right;
        }

        /// <summary>左右どちらかのコントローラーがガイドへ届く場合にtrueを返す。</summary>
        public bool IsWithinReach()
        {
            if (workspaceBounds == null || !workspaceBounds.enabled)
            {
                return false;
            }

            float maximumDistanceSquared = maximumDistanceMeters * maximumDistanceMeters;
            return IsControllerWithinReach(leftController, maximumDistanceSquared)
                || IsControllerWithinReach(rightController, maximumDistanceSquared);
        }

        private bool IsControllerWithinReach(Transform controller, float maximumDistanceSquared)
        {
            if (controller == null || !controller.gameObject.activeInHierarchy)
            {
                return false;
            }

            Vector3 closestPoint = workspaceBounds.ClosestPoint(controller.position);
            return (closestPoint - controller.position).sqrMagnitude <= maximumDistanceSquared;
        }
    }
}
