using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sink : MonoBehaviour, IInteractable
{
    //Av Linn Li


    private Bucket bucketScript;
    private PlayerMovement playerMovementScript;
    [SerializeField] private AudioClip sinkWaterAC;
    [SerializeField] private AudioSource sinkAS;

    public string InteractionPrompt { get; }

    // Start is called before the first frame update
    void Start()
    {
        bucketScript = FindObjectOfType<Bucket>();
        playerMovementScript = FindObjectOfType<PlayerMovement>();

        sinkAS.clip = sinkWaterAC;
    }

    //if player interacts with sink
    public bool Interact(Interactor interactor)
    {
        if (bucketScript.pickedUpBucket == true)
        {
            bucketScript.hasWater = true;
            sinkAS.Play();

            playerMovementScript.PausePlayerMovement();

            // Start a coroutine to wait for audio clip and resume movement
            StartCoroutine(ResumeMovementAfterClip(sinkAS.clip.length));

            return true;
        } else
        {
            return false;
        }
    }

    private IEnumerator ResumeMovementAfterClip(float clipLength)
    {
        //resumes player movement after the audioClip is finished
        yield return new WaitForSeconds(clipLength);
        playerMovementScript.ResumePlayerMovement();
    }
}
