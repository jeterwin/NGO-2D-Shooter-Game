using System;
using Unity.Collections;
using Unity.Netcode;

public struct LeaderboardEntityState : INetworkSerializable, IEquatable<LeaderboardEntityState>
{
    public ulong ClientId;

    public FixedString32Bytes PlayerName;

    public int PlayerKills;
    public int PlayerDeaths;
    public int PlayerCoins;
    public int PlayerAssists;
    public int PlayerTeam;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref PlayerKills);
        serializer.SerializeValue(ref PlayerDeaths);
        serializer.SerializeValue(ref PlayerCoins);
        serializer.SerializeValue(ref PlayerAssists);
        serializer.SerializeValue(ref PlayerTeam);
    }
    public bool Equals(LeaderboardEntityState other)
    {
        return ClientId == other.ClientId && PlayerName.Equals(other.PlayerName) && PlayerKills == other.PlayerKills
            && PlayerDeaths == other.PlayerDeaths && PlayerAssists == other.PlayerAssists && 
            PlayerCoins == other.PlayerCoins && PlayerTeam == other.PlayerTeam;
    }
}