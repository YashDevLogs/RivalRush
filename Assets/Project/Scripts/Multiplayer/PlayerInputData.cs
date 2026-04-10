using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using System;
using Unity.Netcode;

namespace Game.Input
{
    [Serializable]
    public struct PlayerInputData : INetworkSerializable
    {
        public float horizontal;
        public bool jump;
        public int tick;

        public PlayerInputData(float horizontal, bool jump, int tick)
        {
            this.horizontal = horizontal;
            this.jump = jump;
            this.tick = tick;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref horizontal);
            serializer.SerializeValue(ref jump);
            serializer.SerializeValue(ref tick);
        }
    }
}
