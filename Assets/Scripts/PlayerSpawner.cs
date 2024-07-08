using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public static PlayerSpawner Instance;

    [SerializeField] private float respawnTime = 3f;

    public float RespawnTime
    { get { return respawnTime; } }

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject deathParticlesPrefab;

    private GameObject player;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
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
    private void SpawnPlayerServerRpc()
    {
        List<ulong> playerIDs = GetAllPlayerIDs();
        foreach (var clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // We iterate through all ofthe playerIDs and we compare it to the clientID we want to instantiate
            // if the ID was already spawned in, break the loop and don't instantiate any new players with that ID
            bool isOk = true;
            foreach (ulong id in playerIDs)
            {
                if(id == clientID) 
                { 
                    isOk = false;
                    break;
                }
            }
            if(!isOk) { continue; }
            Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint();

            GameObject playerTransform = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity); 
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID, true);
        }
    }
}
