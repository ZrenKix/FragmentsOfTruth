using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DoorController : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioClip creakingAC;
    [SerializeField] private AudioClip doorOpenAC;
    //[SerializeField] private AudioClip memoryWhisperAC;
    [SerializeField] private AudioClip memoryFlashbackAC;
    [SerializeField] private AudioClip memoryAC;
    [SerializeField] private AudioClip[] lockedBasementVoicelines;
    [SerializeField] private AudioSource doorAS;
    [SerializeField] private AudioSource playerAS;

    private MemoryShardManager memoryShardManager;
    private PlayerMovement playerMovement;
    public string InteractionPrompt { get; }

    void Start()
    {
        memoryShardManager = FindObjectOfType<MemoryShardManager>();
        playerMovement= FindObjectOfType<PlayerMovement>();
        doorAS.clip = creakingAC;  
        doorAS.loop = true;
        doorAS.Play();
    }
    public bool Interact(Interactor interactor)
    {
        if(memoryShardManager.allShardsFound() == false)
        {
            int randomIndex = Random.Range(0, lockedBasementVoicelines.Length);
            AudioClip randomClip = lockedBasementVoicelines[randomIndex];
            playerAS.PlayOneShot(randomClip);

            return true;
        } else if (memoryShardManager.allShardsFound() == true)
        {
            playerMovement.PausePlayerMovement();

            doorAS.clip = doorOpenAC;
            doorAS.loop = false;
            doorAS.Play();

            // Start a coroutine to wait for audio clip
            StartCoroutine(PlayMemoryFlashback(doorAS.clip.length));

            return true;
        } else
        {
            return false;
        }
        
    }

    //private IEnumerator PlayWhisper(float clipLength)
    //{
    //    //acts after the audio clip is finished
    //    yield return new WaitForSeconds(clipLength);

    //    //play whisper
    //    doorAS.clip = memoryWhisperAC;
    //    doorAS.Play();

    //    // Start a coroutine to wait for audio clip
    //    StartCoroutine(PlayMemoryStart(doorAS.clip.length));
    //}

    private IEnumerator PlayMemoryFlashback(float clipLength)
    {
        //acts after the audio clip is finished
        yield return new WaitForSeconds(clipLength);

        //play memory start
        doorAS.clip = memoryFlashbackAC;
        doorAS.Play();

        // Start a coroutine to wait for audio clip
        StartCoroutine(PlayMemory(doorAS.clip.length));
    }

    private IEnumerator PlayMemory(float clipLength)
    {
        //acts after the audio clip is finished
        yield return new WaitForSeconds(clipLength);

        //play memory
        doorAS.clip = memoryAC;
        doorAS.Play();

        // Start a coroutine to wait for audio clip
        StartCoroutine(ResumeMovement(doorAS.clip.length));
    }

    private IEnumerator ResumeMovement (float clipLength)
    {
        //acts after the audio clip is finished
        yield return new WaitForSeconds(clipLength);

        Debug.Log("Memory finished playing");

        playerMovement.ResumePlayerMovement();
        OpenDoor();
    }

    public void OpenDoor()
    {
        Debug.Log("Door is disappearing!");
        gameObject.SetActive(false);  // Disable the door to make it disappear
    }
}