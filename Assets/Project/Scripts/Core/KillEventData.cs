using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using Game.Core;

namespace Game.Core
{
    public readonly struct KillEventData
    {
        public readonly IPlayerEntity Killer;
        public readonly IPlayerEntity Victim;
        public readonly PlayerIdentity KillerIdentity;
        public readonly PlayerIdentity VictimIdentity;
        public readonly PowerUpId PowerUpId;

        public KillEventData(
            IPlayerEntity killer,
            IPlayerEntity victim,
            PowerUpId powerUpId)
        {
            Killer = killer;
            Victim = victim;
            KillerIdentity = ResolveIdentity(killer);
            VictimIdentity = ResolveIdentity(victim);
            PowerUpId = powerUpId;
        }

        private static PlayerIdentity ResolveIdentity(IPlayerEntity player)
        {
            return player switch
            {
                PlayerController playerController => playerController.PlayerIdentity,
                AIPlayerEntity aiPlayerEntity => aiPlayerEntity.PlayerIdentity,
                _ => null
            };
        }
    }

}