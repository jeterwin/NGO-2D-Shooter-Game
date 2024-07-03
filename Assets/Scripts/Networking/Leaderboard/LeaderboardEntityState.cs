using System;
using Unity.Collections;
using Unity.Netcode;

public struct LeaderboardEntityState : INetworkSerializable, IEquatable<LeaderboardEntityState>
{
    public ulong ClientId;

    public FixedString32Bytes PlayerName;

    public int PlayerKills;
    public int PlayerDeaths;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref PlayerKills);
        serializer.SerializeValue(ref PlayerDeaths);
    }
    public bool Equals(LeaderboardEntityState other)
    {
        return ClientId == other.ClientId && PlayerName.Equals(other.PlayerName) && PlayerKills == other.PlayerKills
            && PlayerDeaths == other.PlayerDeaths;
    }
}