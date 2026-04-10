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
    /// Replaces AutoDestroy on VFX prefabs.
    /// Holds a reference to its own pool so it can return itself
    /// automatically when the particle system finishes — no Destroy calls.
    /// </summary>
    public sealed class VFXEntry : MonoBehaviour
    {
        private VFXPool owner;
        private Queue<VFXEntry> homePool;
        [SerializeField] private ParticleSystem[] particles;
        private float lifetime;
        private float timer;
        private bool playing;

        // Updated signature — takes the pool queue so it can return itself
        public void Initialize(VFXPool pool, Queue<VFXEntry> pool_queue)
        {
            owner = pool;
            homePool = pool_queue;
            if (particles == null)
                particles = new ParticleSystem[0];

            if (particles.Length == 0)
                Debug.LogWarning($"[VFXEntry] Particle systems are not assigned on {name}.");

            lifetime = 0.1f;
            foreach (var ps in particles)
            {
                var main = ps.main;
                float psLife = main.duration + main.startLifetime.constantMax;
                if (psLife > lifetime) lifetime = psLife;
            }

            gameObject.SetActive(false);
            playing = false;
        }

        public void Play()
        {
            timer = lifetime;
            playing = true;

            foreach (var ps in particles)
            {
                ps.Clear(true);
                ps.Play(true);
            }
        }

        private void Update()
        {
            if (!playing) return;

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                playing = false;
                // Return directly to home pool — no Destroy
                if (homePool != null)
                {
                    gameObject.SetActive(false);
                    transform.SetParent(owner.transform);
                    homePool.Enqueue(this);
                }
            }
        }
    }

}