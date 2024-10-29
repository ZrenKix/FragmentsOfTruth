using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalMemory : MonoBehaviour
{
    [SerializeField] private AudioClip knockingAC;
    [SerializeField] private AudioClip memoryFlashbackAC;
    [SerializeField] private AudioClip memoryAC;
    [SerializeField] private AudioSource finalMemoryAS;

    private PlayerMovement playerMovement;

    private bool isTriggered = false;

    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        finalMemoryAS.clip = knockingAC;
        finalMemoryAS.loop = true;
        finalMemoryAS.Play();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            playerMovement.PausePlayerMovement();

            //play memory start
            finalMemoryAS.clip = memoryFlashbackAC;
            finalMemoryAS.Play();
            finalMemoryAS.loop = false;

            // Start a coroutine to wait for audio clip
            StartCoroutine(PlayMemory(finalMemoryAS.clip.length));
        }
    }

    private IEnumerator PlayMemory(float clipLength)
    {
        //acts after the audio clip is finished
        yield return new WaitForSeconds(clipLength);

        //play memory
        finalMemoryAS.clip = memoryAC;
        finalMemoryAS.Play();

        // Start a coroutine to wait for audio clip
        StartCoroutine(ResumeMovement(finalMemoryAS.clip.length));
    }

    private IEnumerator ResumeMovement(float clipLength)
    {
        //acts after the audio clip is finished
        yield return new WaitForSeconds(clipLength);

        Debug.Log("Memory finished playing");

        playerMovement.ResumePlayerMovement();
        Destroy(this.gameObject);
    }
}
