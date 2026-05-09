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
        public readonly string KillerName;
        public readonly string VictimName;
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
            KillerName = ResolveDisplayName(KillerIdentity, "Unknown");
            VictimName = ResolveDisplayName(VictimIdentity, "Unknown");
            PowerUpId = powerUpId;
        }

        public KillEventData(
            string killerName,
            string victimName,
            PowerUpId powerUpId)
        {
            Killer = null;
            Victim = null;
            KillerIdentity = null;
            VictimIdentity = null;
            KillerName = string.IsNullOrWhiteSpace(killerName) ? "Unknown" : killerName;
            VictimName = string.IsNullOrWhiteSpace(victimName) ? "Unknown" : victimName;
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

        private static string ResolveDisplayName(PlayerIdentity identity, string fallback)
        {
            if (identity == null || string.IsNullOrWhiteSpace(identity.DisplayName))
                return fallback;

            return identity.DisplayName;
        }
    }

}
