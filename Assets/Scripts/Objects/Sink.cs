using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sink : MonoBehaviour, IInteractable
{
    //Av Linn Li


    private Bucket bucketScript;
    private PlayerMovement playerMovementScript;
    [SerializeField] private AudioClip waterRunningAC;
    [SerializeField] private AudioClip waterDrippingAC;
    [SerializeField] private AudioClip sinkLockedVoiceLine;
    [SerializeField] private AudioSource sinkAS;

    public string InteractionPrompt { get; }

    // Start is called before the first frame update
    void Start()
    {
        bucketScript = FindObjectOfType<Bucket>();
        playerMovementScript = FindObjectOfType<PlayerMovement>();

        //audioclip = dripping water, looping
        sinkAS.clip = waterDrippingAC;
        sinkAS.loop = true;
    }

    //if player interacts with sink
    public bool Interact(Interactor interactor)
    {
        if (bucketScript.pickedUpBucket == true)
        {
            bucketScript.hasWater = true;

            //change audioclip to the one for filling the bucket with water (plays once)
            sinkAS.clip = waterRunningAC;
            sinkAS.loop = false;

            playerMovementScript.PausePlayerMovement();

            LogManager.Instance.LogEvent($"{gameObject.name} water collected");

            // Start a coroutine to wait for audio clip and resume movement
            StartCoroutine(ResumeMovementAfterClip(sinkAS.clip.length));

            return true;
        }
        else if ((bucketScript.pickedUpBucket == false))
        {
            //play voiceline for locked sink
            sinkAS.loop = false;
            sinkAS.clip = sinkLockedVoiceLine;
            sinkAS.Play();

            //change back to dripping audioclip after voiceline has played
            StartCoroutine(ChangeToDripping(sinkAS.clip.length));

            return true;
        }
        else 
        {
            return false;
        }
    }

    private IEnumerator ResumeMovementAfterClip(float clipLength)
    {
        //resumes player movement after the audioClip is finished
        yield return new WaitForSeconds(clipLength);
        playerMovementScript.ResumePlayerMovement();


        //change back audioclip to dripping water
        sinkAS.clip = waterDrippingAC;
        sinkAS.Play();
        sinkAS.loop = true;
    }

    private IEnumerator ChangeToDripping(float clipLength)
    {
        //resumes player movement after the audioClip is finished
        yield return new WaitForSeconds(clipLength);

        //change back audioclip to dripping water
        sinkAS.clip = waterDrippingAC;
        sinkAS.Play();
        sinkAS.loop = true;
    }
}
