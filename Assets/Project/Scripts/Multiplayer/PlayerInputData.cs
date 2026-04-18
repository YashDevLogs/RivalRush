using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using System;
using Unity.Netcode;

// -------------------------------------------------------
// PlayerInputData — UNUSED (client prediction removed)
//
// This struct was designed for tick-based client-side prediction:
// the client would stamp each input with a tick number, send it
// to the server, the server would validate it, and mismatches
// would be reconciled (rollback + replay).
//
// Prediction was removed because it added significant complexity
// for a game where low-latency LAN play is the primary use case.
// At <50ms RTT the server-authoritative model feels acceptable.
//
// TO RE-ENABLE PREDICTION:
// 1. Buffer inputs in PlayerController using this struct.
// 2. Send buffered inputs to server via ServerRpc.
// 3. Server validates and applies, sends correction ClientRpc on mismatch.
// 4. Client replays inputs from the mismatched tick forward.
// -------------------------------------------------------

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
