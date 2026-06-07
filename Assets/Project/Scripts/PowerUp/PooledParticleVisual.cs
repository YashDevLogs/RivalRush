using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.Systems
{
    public sealed class PooledParticleVisual : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] particles;

        private void Awake()
        {
            if (particles == null)
                particles = new ParticleSystem[0];

           if (particles.Length == 0)
    return;
        }

        public void Play()
        {
            foreach (var ps in particles)
            {
                if (ps == null) continue;
                ps.Clear(true);
                ps.Play(true);
            }
        }

        public void StopAndClear()
        {
            foreach (var ps in particles)
            {
                if (ps == null) continue;
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

}