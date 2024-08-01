using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager5v5 : MonoBehaviour
{
    public static SpawnManager5v5 Instance;

    [SerializeField] private Transform[] redTeamTransform;
    [SerializeField] private Transform[] blueTeamTransform;
    private void Awake()
    {
        Instance = this;
    }

    public Transform GetSpawnPoint(bool isTeamRed)
    {
        return isTeamRed == true ? 
            redTeamTransform[Random.Range(0, redTeamTransform.Length)] : blueTeamTransform[Random.Range(0, blueTeamTransform.Length)];
    }
}
