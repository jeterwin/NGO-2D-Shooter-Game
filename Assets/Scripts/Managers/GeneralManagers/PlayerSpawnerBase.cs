using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnerBase : NetworkBehaviour
{
    public static PlayerSpawnerBase Instance;

    [SerializeField] private float respawnTime = 3f;

    [field: SerializeField] public GameObject playerPrefab { get; private set; }

    public float RespawnTime
    { get { return respawnTime; } }


    private void Awake()
    {
        Instance = this;
    }
    protected virtual void Start()
    {
        SpawnPlayerServerRpc();
    }

    public List<ulong> GetAllPlayerIDs()
    {
        List<ulong> playerIDs = new List<ulong>();

        foreach (var kvp in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            NetworkObject networkObject = kvp.Value;
            if (networkObject.IsPlayerObject)
            {
                playerIDs.Add(networkObject.OwnerClientId);
            }
        }

        return playerIDs;
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SpawnPlayerServerRpc()
    {
        List<ulong> playerIDs = GetAllPlayerIDs();
        foreach (var clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // We iterate through all ofthe playerIDs and we compare it to the clientID we want to instantiate
            // if the ID was already spawned in, break the loop and don't instantiate any new players with that ID
            if (playerIDs.Contains(clientID)) { continue; }

            GameObject playerTransform = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

            NetworkObject playerObject = playerTransform.GetComponent<NetworkObject>();

            Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint();
            playerTransform.transform.position = spawnPoint.position;
            playerObject.SpawnAsPlayerObject(clientID, true);
        }
    }
}
