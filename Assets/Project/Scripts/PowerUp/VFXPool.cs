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
    /// Central VFX pool for all visual effects.
    /// Add to ProjectilePool GameObject in LevelPrototype scene.
    /// Assign all prefab references in Inspector.
    /// </summary>
    public sealed class VFXPool : MonoBehaviour
    {
        public static VFXPool Instance { get; private set; }

        [Header("Death Smoke")]
        [SerializeField] private VFXEntry deathSmokePrefab;
        [SerializeField] private int deathSmokePoolSize = 8;

        [Header("Shield Visual")]
        [SerializeField] private PooledParticleVisual shieldVisualPrefab;
        [SerializeField] private int shieldVisualPoolSize = 4;

        [Header("Explosion VFX")]
        [SerializeField] private VFXEntry explosionVfxPrefab;
        [SerializeField] private int explosionPoolSize = 6;

        private readonly Queue<VFXEntry> deathSmokePool  = new();
        private readonly Queue<VFXEntry> explosionPool   = new();
        private readonly Queue<PooledParticleVisual> shieldPool = new();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            PreWarmEntry(deathSmokePrefab,  deathSmokePool,  deathSmokePoolSize);
            PreWarmEntry(explosionVfxPrefab, explosionPool,  explosionPoolSize);
            PreWarmShield(shieldVisualPrefab, shieldPool,    shieldVisualPoolSize);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ---- Pre-warm helpers ----

        private void PreWarmEntry(VFXEntry prefab, Queue<VFXEntry> pool, int count)
        {
            if (prefab == null) { Debug.LogWarning($"[VFXPool] Missing prefab for pool"); return; }

            for (int i = 0; i < count; i++)
            {
                var entry = Instantiate(prefab, transform);
                entry.gameObject.SetActive(false);
                entry.Initialize(this, pool);
                pool.Enqueue(entry);
            }
        }

        private void PreWarmShield(PooledParticleVisual prefab, Queue<PooledParticleVisual> pool, int count)
        {
            if (prefab == null) { Debug.LogWarning("[VFXPool] No shield visual prefab assigned"); return; }

            for (int i = 0; i < count; i++)
            {
                var visual = Instantiate(prefab, transform);
                visual.gameObject.SetActive(false);
                pool.Enqueue(visual);
            }
        }

        // ---- Death Smoke ----

        public GameObject GetDeathSmoke(Vector3 position)
            => GetEntry(deathSmokePool, deathSmokePrefab, position);

        public void ReturnDeathSmoke(VFXEntry entry)
            => ReturnEntry(entry, deathSmokePool);

        // ---- Explosion ----

        public GameObject GetExplosion(Vector3 position)
            => GetEntry(explosionPool, explosionVfxPrefab, position);

        public void ReturnExplosion(VFXEntry entry)
            => ReturnEntry(entry, explosionPool);

        // ---- Shield ----

        public PooledParticleVisual GetShieldVisual(Vector3 position, Transform parent)
        {
            PooledParticleVisual visual = shieldPool.Count > 0
                ? shieldPool.Dequeue()
                : Instantiate(shieldVisualPrefab, transform);

            GameObject go = visual.gameObject;
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            go.SetActive(true);

            visual.Play();

            return visual;
        }

        public void ReturnShieldVisual(PooledParticleVisual visual)
        {
            if (visual == null) return;

            GameObject go = visual.gameObject;
            visual.StopAndClear();

            go.SetActive(false);
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            shieldPool.Enqueue(visual);
        }

        // ---- Generic entry helpers ----

        private GameObject GetEntry(Queue<VFXEntry> pool, VFXEntry prefab, Vector3 position)
        {
            VFXEntry entry;

            if (pool.Count > 0)
            {
                entry = pool.Dequeue();
            }
            else
            {
                Debug.LogWarning($"[VFXPool] Pool exhausted for {prefab?.name}, growing.");
                entry = Instantiate(prefab, transform);
                entry.Initialize(this, pool);
            }

            entry.transform.position = position;
            entry.gameObject.SetActive(true);
            entry.Play();
            return entry.gameObject;
        }

        private void ReturnEntry(VFXEntry entry, Queue<VFXEntry> pool)
        {
            if (entry == null) return;
            entry.gameObject.SetActive(false);
            entry.transform.SetParent(transform);
            pool.Enqueue(entry);
        }
    }

}