namespace Game.Core
{
    public enum GameMode
    {
        Multiplayer,
        SinglePlayer
    }

    /// <summary>
    /// Static accessor so any script can check the current mode
    /// without needing a direct reference to SessionManager.
    /// Set by SessionManager before a session or SP game starts.
    /// </summary>
    public static class GameModeState
    {
        public static GameMode Current { get; private set; } = GameMode.Multiplayer;

        public static bool IsSinglePlayer => Current == GameMode.SinglePlayer;
        public static bool IsMultiplayer  => Current == GameMode.Multiplayer;

        public static void Set(GameMode mode) => Current = mode;
    }
}