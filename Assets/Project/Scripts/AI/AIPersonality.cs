using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;

namespace Game.AI
{
    public enum AIPersonality
    {
        Aggressive,   // Uses power-ups early, jumps late
        Defensive,    // Saves shield, jumps early
        Risky,        // Delays jumps, late reactions
        Balanced      // Default (current behavior)
    }

}