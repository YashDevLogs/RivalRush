using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using System.Collections.Generic;

namespace Game.Systems
{
    /// <summary>
    /// Central object pool for all projectile types.
    /// 
    /// SETUP:
    /// 1. Create an empty GameObject in LevelPrototype scene named "ProjectilePool"
    /// 2. Add this script to it
    /// 3. Assign all prefab references in the Inspector
    /// 4. Make sure prefabs do NOT have Destroy calls — they use ReturnXxx() instead
    /// </summary>
    public sealed class ProjectilePool : MonoBehaviour
    {
        public static ProjectilePool Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private RocketController rocketPrefab;
        [SerializeField] private RevolverBulletController bulletPrefab;
        [SerializeField] private SawbladeController sawbladePrefab;
        [SerializeField] private ShockerEffectController shockerPrefab;
        [SerializeField] private TrapController trapPrefab;

        [Header("Pool Sizes")]
        [SerializeField] private int rocketPoolSize    = 4;
        [SerializeField] private int bulletPoolSize    = 16;
        [SerializeField] private int sawbladePoolSize  = 4;
        [SerializeField] private int shockerPoolSize   = 4;
        [SerializeField] private int trapPoolSize      = 6;

        private readonly Queue<RocketController>          rockets    = new();
        private readonly Queue<RevolverBulletController>  bullets    = new();
        private readonly Queue<SawbladeController>        sawblades  = new();
        private readonly Queue<ShockerEffectController>   shockers   = new();
        private readonly Queue<TrapController>            traps      = new();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            PreWarm(rocketPrefab,   rockets,   rocketPoolSize);
            PreWarm(bulletPrefab,   bullets,   bulletPoolSize);
            PreWarm(sawbladePrefab, sawblades, sawbladePoolSize);
            PreWarm(shockerPrefab,  shockers,  shockerPoolSize);
            PreWarm(trapPrefab,     traps,     trapPoolSize);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void PreWarm<T>(T prefab, Queue<T> pool, int count) where T : MonoBehaviour
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[ProjectilePool] No prefab assigned for {typeof(T).Name}");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                T obj = Instantiate(prefab, transform);
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        // ---- Get ----

        public RocketController GetRocket(Vector3 pos, Quaternion rot)
            => Get(rockets, rocketPrefab, pos, rot);

        public RevolverBulletController GetBullet(Vector3 pos, Quaternion rot)
            => Get(bullets, bulletPrefab, pos, rot);

        public SawbladeController GetSawblade(Vector3 pos, Quaternion rot)
            => Get(sawblades, sawbladePrefab, pos, rot);

        public ShockerEffectController GetShocker(Vector3 pos, Quaternion rot)
            => Get(shockers, shockerPrefab, pos, rot);

        public TrapController GetTrap(Vector3 pos, Quaternion rot)
            => Get(traps, trapPrefab, pos, rot);

        private T Get<T>(Queue<T> pool, T prefab, Vector3 pos, Quaternion rot)
            where T : MonoBehaviour
        {
            T obj = pool.Count > 0
                ? pool.Dequeue()
                : Instantiate(prefab, transform); // grow if exhausted

            obj.transform.SetPositionAndRotation(pos, rot);
            obj.gameObject.SetActive(true);
            return obj;
        }

        // ---- Return ----

        public void ReturnRocket(RocketController obj)         => Return(obj, rockets);
        public void ReturnBullet(RevolverBulletController obj) => Return(obj, bullets);
        public void ReturnSawblade(SawbladeController obj)     => Return(obj, sawblades);
        public void ReturnShocker(ShockerEffectController obj) => Return(obj, shockers);
        public void ReturnTrap(TrapController obj)             => Return(obj, traps);

        private void Return<T>(T obj, Queue<T> pool) where T : MonoBehaviour
        {
            if (obj == null) return;
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(transform);
            pool.Enqueue(obj);
        }
    }

}