using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using Game.Core;
using UnityEngine;
using Unity.Netcode;

namespace Game.Systems
{
    [RequireComponent(typeof(Collider2D))]
    public class EndPoint : MonoBehaviour, IEndPoint
    {
        [SerializeField] private Collider2D triggerCollider;

        [Tooltip("Optional: disable player input on finish")]
        public bool disablePlayerOnFinish = true;

        private void Reset()
        {
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<IPlayerController>(out var player))
                return;
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
                return;

            RaceManager.Instance?.RegisterFinish(player);
            TriggerEnd();
        }

        public void TriggerEnd()
        {
            Debug.Log("EndPoint reached - finish forwarded to RaceManager.");
        }
    }

}
