using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using System;
using Unity.Collections;
using Unity.Netcode;

namespace Game.Systems
{
    public struct LobbyPlayerData : INetworkSerializable, IEquatable<LobbyPlayerData>
    {
        public ulong ClientId;
        public FixedString32Bytes PlayerName;
        public bool IsReady;
        public bool IsAI;

        public LobbyPlayerData(ulong clientId, FixedString32Bytes playerName, bool isReady, bool isAI)
        {
            ClientId = clientId;
            PlayerName = playerName;
            IsReady = isReady;
            IsAI = isAI;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref IsReady);
            serializer.SerializeValue(ref IsAI);
        }

        public bool Equals(LobbyPlayerData other)
        {
            return ClientId == other.ClientId &&
                   PlayerName.Equals(other.PlayerName) &&
                   IsReady == other.IsReady &&
                   IsAI == other.IsAI;
        }

        public override bool Equals(object obj)
        {
            return obj is LobbyPlayerData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClientId, PlayerName, IsReady, IsAI);
        }
    }

}