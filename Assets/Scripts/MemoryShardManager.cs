using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryShardManager : MonoBehaviour
{
    public static MemoryShardManager Instance;

    [SerializeField] private int totalShards = 3;  // Total number of shards to find
    private int shardsCollected = 0;  // Count of found shards

    [SerializeField] private DoorController doorController; // Reference to door controller to open the door

    private void Awake()
    {
        // Make sure there's only one instance of MemoryShardManager (Singleton)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this method when a shard is found
    public void ShardFound()
    {
        shardsCollected++;
        Debug.Log("Memory shard found. Total collected: " + shardsCollected);

        // If all shards have been collected, open the door
        if (shardsCollected >= totalShards)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        Debug.Log("All memory shards found! Opening door.");
        doorController.OpenDoor();  // Open the door when all shards are found
    }
}