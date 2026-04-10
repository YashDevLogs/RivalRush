using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.Systems
{
    [CreateAssetMenu(menuName = "Game/VFX Library")]
    public sealed class VFXLibrary : ScriptableObject
    {
        [Header("Player")]
        public GameObject deathSmokePrefab;
    }

}