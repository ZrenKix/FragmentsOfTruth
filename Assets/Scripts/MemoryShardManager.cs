using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryShardManager : MonoBehaviour
{
    public static MemoryShardManager Instance;

    [SerializeField] private int totalShards = 3;  // Total number of shards to find
    private int shardsCollected = 0;  // Count of found shards

    [SerializeField] private DoorController doorController; // Reference to door controller to open the door

    [SerializeField] private GameObject audioSourceObject1; // GameObject containing the first audio source
    [SerializeField] private GameObject audioSourceObject2; // GameObject containing the second audio source

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

        // Deactivate audio source objects initially
        if (audioSourceObject1 != null) audioSourceObject1.SetActive(false);
        if (audioSourceObject2 != null) audioSourceObject2.SetActive(false);
    }

    // Call this method when a shard is found
    public void ShardFound()
    {
        shardsCollected++;
        Debug.Log("Memory shard found. Total collected: " + shardsCollected);

        // If all shards have been collected, open the door and activate audio
        if (shardsCollected >= totalShards)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        Debug.Log("All memory shards found! Opening door.");
        doorController.OpenDoor();  // Open the door when all shards are found
        Invoke("ActivateAudioSources", 32f);     // Activate audio sources when the door opens
    }

    private void ActivateAudioSources()
    {
        // Activate the audio source GameObjects, which will enable their AudioSources to play
        if (audioSourceObject1 != null)
        {
            audioSourceObject1.SetActive(true);
            audioSourceObject1.GetComponent<AudioSource>().Play();
        }

        if (audioSourceObject2 != null)
        {
            audioSourceObject2.SetActive(true);
            audioSourceObject2.GetComponent<AudioSource>().Play();
        }

        Debug.Log("Audio sources activated.");
    }
}
