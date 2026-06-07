using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Game.Core
{
    public static class GameEvents
    {
        public static bool EnableEventDebug { get; set; }

        private static Action onRaceStarted;
        private static Action onRaceFinished;
        private static Action onPlayerDied;
        private static Action onPlayerRespawned;
        private static Action<Vector3> onCheckpointReached;
        private static Action onLocalPlayerSpawned;
        private static Action<PowerUpId> onPowerUpPicked;
        private static Action<PowerUpId> onPowerUpActivated;
        private static Action<PowerUpId> onPowerUpExpired;
        private static Action<KillEventData> onPlayerKilled;
        private static Action<string> onAnyEventRaised;

        // RACE
        public static event Action OnRaceStarted
        {
            add => onRaceStarted = Subscribe(onRaceStarted, value, nameof(OnRaceStarted));
            remove => onRaceStarted = Unsubscribe(onRaceStarted, value, nameof(OnRaceStarted));
        }

        public static event Action OnRaceFinished
        {
            add => onRaceFinished = Subscribe(onRaceFinished, value, nameof(OnRaceFinished));
            remove => onRaceFinished = Unsubscribe(onRaceFinished, value, nameof(OnRaceFinished));
        }

        // PLAYER
        public static event Action OnPlayerDied
        {
            add => onPlayerDied = Subscribe(onPlayerDied, value, nameof(OnPlayerDied));
            remove => onPlayerDied = Unsubscribe(onPlayerDied, value, nameof(OnPlayerDied));
        }

        public static event Action OnPlayerRespawned
        {
            add => onPlayerRespawned = Subscribe(onPlayerRespawned, value, nameof(OnPlayerRespawned));
            remove => onPlayerRespawned = Unsubscribe(onPlayerRespawned, value, nameof(OnPlayerRespawned));
        }

        public static event Action<Vector3> OnCheckpointReached
        {
            add => onCheckpointReached = Subscribe(onCheckpointReached, value, nameof(OnCheckpointReached));
            remove => onCheckpointReached = Unsubscribe(onCheckpointReached, value, nameof(OnCheckpointReached));
        }

        public static event Action OnLocalPlayerSpawned
        {
            add => onLocalPlayerSpawned = Subscribe(onLocalPlayerSpawned, value, nameof(OnLocalPlayerSpawned));
            remove => onLocalPlayerSpawned = Unsubscribe(onLocalPlayerSpawned, value, nameof(OnLocalPlayerSpawned));
        }

        // POWER-UPS
        public static event Action<PowerUpId> OnPowerUpPicked
        {
            add => onPowerUpPicked = Subscribe(onPowerUpPicked, value, nameof(OnPowerUpPicked));
            remove => onPowerUpPicked = Unsubscribe(onPowerUpPicked, value, nameof(OnPowerUpPicked));
        }

        public static event Action<PowerUpId> OnPowerUpActivated
        {
            add => onPowerUpActivated = Subscribe(onPowerUpActivated, value, nameof(OnPowerUpActivated));
            remove => onPowerUpActivated = Unsubscribe(onPowerUpActivated, value, nameof(OnPowerUpActivated));
        }

        public static event Action<PowerUpId> OnPowerUpExpired
        {
            add => onPowerUpExpired = Subscribe(onPowerUpExpired, value, nameof(OnPowerUpExpired));
            remove => onPowerUpExpired = Unsubscribe(onPowerUpExpired, value, nameof(OnPowerUpExpired));
        }

        // KILL FEED
        public static event Action<KillEventData> OnPlayerKilled
        {
            add => onPlayerKilled = Subscribe(onPlayerKilled, value, nameof(OnPlayerKilled));
            remove => onPlayerKilled = Unsubscribe(onPlayerKilled, value, nameof(OnPlayerKilled));
        }

        // GLOBAL TRACKING HOOK
        public static event Action<string> OnAnyEventRaised
        {
            add => onAnyEventRaised = Subscribe(onAnyEventRaised, value, nameof(OnAnyEventRaised));
            remove => onAnyEventRaised = Unsubscribe(onAnyEventRaised, value, nameof(OnAnyEventRaised));
        }

        // RAISE METHODS
        public static void RaiseRaceStarted([CallerMemberName] string caller = null) =>
            InvokeEvent(nameof(OnRaceStarted), onRaceStarted, caller);

        public static void RaiseRaceFinished([CallerMemberName] string caller = null) =>
            InvokeEvent(nameof(OnRaceFinished), onRaceFinished, caller);

        public static void RaisePlayerDied([CallerMemberName] string caller = null) =>
            InvokeEvent(nameof(OnPlayerDied), onPlayerDied, caller);

        public static void RaisePlayerRespawned([CallerMemberName] string caller = null) =>
            InvokeEvent(nameof(OnPlayerRespawned), onPlayerRespawned, caller);

        public static void RaiseCheckpointReached(Vector3 position, [CallerMemberName] string caller = null) =>
            InvokeEvent(nameof(OnCheckpointReached), onCheckpointReached, position, caller);

        public static void RaiseLocalPlayerSpawned([CallerMemberName] string caller = null) =>
            InvokeEvent(nameof(OnLocalPlayerSpawned), onLocalPlayerSpawned, caller);

        public static void RaisePowerUpPicked(PowerUpId powerUpId, [CallerMemberName] string caller = null) =>
            InvokeEvent(nameof(OnPowerUpPicked), onPowerUpPicked, powerUpId, caller);

        public static void RaisePowerUpActivated(PowerUpId powerUpId, [CallerMemberName] string caller = null) =>
            InvokeEvent(nameof(OnPowerUpActivated), onPowerUpActivated, powerUpId, caller);

        public static void RaisePowerUpExpired(PowerUpId powerUpId, [CallerMemberName] string caller = null) =>
            InvokeEvent(nameof(OnPowerUpExpired), onPowerUpExpired, powerUpId, caller);

        public static void RaisePlayerKilled(KillEventData data, [CallerMemberName] string caller = null) =>
            InvokeEvent(nameof(OnPlayerKilled), onPlayerKilled, data, caller);

        public static Action Subscribe(Action current, Action listener, string eventName = "UnnamedEvent")
        {
            if (listener == null)
                return current;

            if (HasListener(current, listener))
            {
                LogDebug($"{eventName} already has listener {GetListenerName(listener)}. Duplicate subscription ignored.");
                return current;
            }

            LogDebug($"{eventName} subscribed: {GetListenerName(listener)}");
            return (Action)Delegate.Combine(current, listener);
        }

        public static Action Unsubscribe(Action current, Action listener, string eventName = "UnnamedEvent")
        {
            if (listener == null)
                return current;

            if (!HasListener(current, listener))
            {
                LogDebug($"{eventName} missing listener {GetListenerName(listener)} during unsubscribe.");
                return current;
            }

            LogDebug($"{eventName} unsubscribed: {GetListenerName(listener)}");
            return (Action)Delegate.Remove(current, listener);
        }

        public static Action<T> Subscribe<T>(Action<T> current, Action<T> listener, string eventName = "UnnamedEvent")
        {
            if (listener == null)
                return current;

            if (HasListener(current, listener))
            {
                LogDebug($"{eventName} already has listener {GetListenerName(listener)}. Duplicate subscription ignored.");
                return current;
            }

            LogDebug($"{eventName} subscribed: {GetListenerName(listener)}");
            return (Action<T>)Delegate.Combine(current, listener);
        }

        public static Action<T> Unsubscribe<T>(Action<T> current, Action<T> listener, string eventName = "UnnamedEvent")
        {
            if (listener == null)
                return current;

            if (!HasListener(current, listener))
            {
                LogDebug($"{eventName} missing listener {GetListenerName(listener)} during unsubscribe.");
                return current;
            }

            LogDebug($"{eventName} unsubscribed: {GetListenerName(listener)}");
            return (Action<T>)Delegate.Remove(current, listener);
        }

        private static void InvokeEvent(string eventName, Action handlers, string caller)
        {
            TrackEvent(eventName, null, caller);

            if (handlers == null)
                return;

            foreach (Action handler in handlers.GetInvocationList())
            {
                LogDebug($"{eventName} received by {GetListenerName(handler)}");
                try
                {
                    handler.Invoke();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        private static void InvokeEvent<T>(string eventName, Action<T> handlers, T value, string caller)
        {
            TrackEvent(eventName, FormatParameter(value), caller);

            if (handlers == null)
                return;

            foreach (Action<T> handler in handlers.GetInvocationList())
            {
                LogDebug($"{eventName} received by {GetListenerName(handler)}");
                try
                {
                    handler.Invoke(value);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        private static void TrackEvent(string eventName, string parameterText, string caller)
        {
            string parameterSegment = string.IsNullOrEmpty(parameterText) ? string.Empty : $" | Params: {parameterText}";
            string callerSegment = string.IsNullOrEmpty(caller) ? string.Empty : $" | Caller: {caller}";

            LogDebug($"{eventName} triggered{parameterSegment}{callerSegment}");

            if (onAnyEventRaised == null || eventName == nameof(OnAnyEventRaised))
                return;

            foreach (Action<string> handler in onAnyEventRaised.GetInvocationList())
            {
                try
                {
                    handler.Invoke(eventName);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        private static bool HasListener(Delegate current, Delegate listener)
        {
            if (current == null || listener == null)
                return false;

            foreach (Delegate existing in current.GetInvocationList())
            {
                if (existing == listener)
                    return true;
            }

            return false;
        }

        private static string GetListenerName(Delegate listener)
        {
            if (listener == null)
                return "null";

            string target = listener.Target != null ? listener.Target.GetType().Name : "static";
            return $"{target}.{listener.Method.Name}";
        }

        private static string FormatParameter<T>(T value)
        {
            if (value is KillEventData killEventData)
            {
                string killer = !string.IsNullOrWhiteSpace(killEventData.KillerName) ? killEventData.KillerName : "Unknown";
                string victim = !string.IsNullOrWhiteSpace(killEventData.VictimName) ? killEventData.VictimName : "Unknown";
                return $"Killer={killer}, Victim={victim}, PowerUp={killEventData.PowerUpId}";
            }

            return value != null ? value.ToString() : "null";
        }

        private static void LogDebug(string message)
        {
            if (!EnableEventDebug)
                return;

            Debug.Log($"[GameEvents] {message}");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticEvents()
        {
            onRaceStarted = null;
            onRaceFinished = null;
            onPlayerDied = null;
            onPlayerRespawned = null;
            onCheckpointReached = null;
            onLocalPlayerSpawned = null;
            onPowerUpPicked = null;
            onPowerUpActivated = null;
            onPowerUpExpired = null;
            onPlayerKilled = null;
            onAnyEventRaised = null;
            EnableEventDebug = false;
        }
    }
}
