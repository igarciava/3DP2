using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionSpawner : MonoBehaviour
{
    public CompanionScript CompanionPrefab;
    public Transform SpawnPosition;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Instantiate(CompanionPrefab, SpawnPosition);
        }
    }
}
