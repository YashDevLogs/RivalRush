using UnityEngine;

namespace Game.Audio
{
    public sealed class FollowTarget : MonoBehaviour
    {
        private Transform target;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void ClearTarget()
        {
            target = null;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            transform.position = target.position;
        }
    }
}