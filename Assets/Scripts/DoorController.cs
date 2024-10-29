using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DoorController : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioClip[] lockedBasementVoicelines;
    [SerializeField] private AudioSource AS;

    private MemoryShardManager memoryShardManager;
    public string InteractionPrompt { get; }

    void Start()
    {
        memoryShardManager = FindObjectOfType<MemoryShardManager>();
    }
    public bool Interact(Interactor interactor)
    {
        if(memoryShardManager.allShardsFound() == false)
        {
            int randomIndex = Random.Range(0, lockedBasementVoicelines.Length);
            AudioClip randomClip = lockedBasementVoicelines[randomIndex];
            AS.PlayOneShot(randomClip);

            return true;
        } else
        {
            return false;
        }
        
    }

    public void OpenDoor()
    {
        Debug.Log("Door is disappearing!");
        gameObject.SetActive(false);  // Disable the door to make it disappear
    }
}