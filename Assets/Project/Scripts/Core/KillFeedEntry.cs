using UnityEngine;
using System.Collections;

namespace Game.Core
{
    public sealed class KillFeedEntry : MonoBehaviour
    {
        private KillFeedManager owner;
        private Coroutine autoDestroyCoroutine;
        private bool isDestroyed;

        public void Initialize(KillFeedManager owner, float lifetime)
        {
            this.owner = owner;
            BeginAutoDestroy(lifetime);
        }

        public void CancelAndDestroy()
        {
            if (isDestroyed)
                return;

            if (autoDestroyCoroutine != null)
            {
                StopCoroutine(autoDestroyCoroutine);
                autoDestroyCoroutine = null;
            }

            isDestroyed = true;
            Destroy(gameObject);
        }

        private void BeginAutoDestroy(float lifetime)
        {
            if (autoDestroyCoroutine != null)
                StopCoroutine(autoDestroyCoroutine);

            autoDestroyCoroutine = StartCoroutine(AutoDestroyAfter(lifetime));
        }

        private IEnumerator AutoDestroyAfter(float lifetime)
        {
            yield return new WaitForSeconds(lifetime);

            if (isDestroyed)
                yield break;

            isDestroyed = true;
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            owner?.NotifyEntryDestroyed(gameObject);
        }
    }
}
