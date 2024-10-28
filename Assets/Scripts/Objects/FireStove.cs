using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireStove : MonoBehaviour, IInteractable
{
    //Av Linn Li


    [SerializeField] bool isBurning = true;
    [SerializeField] private AudioClip fireExtinguishAC;
    [SerializeField] private AudioClip fireAC;
    [SerializeField] private AudioClip firestoveLockedVoiceLine;
    [SerializeField] private AudioClip firestoveOpenVoiceLine;
    [SerializeField] private AudioSource fireStoveAS;
    [SerializeField] private GameObject fireStoveMemory;
    private bool hasExtinguishedFire = false; //to prevent audio source from double playing sound

    private Bucket bucketScript;
    private PlayerMovement playerMovementScript;

    public string InteractionPrompt { get; }

    // Start is called before the first frame update
    void Start()
    {
        bucketScript = FindObjectOfType<Bucket>();
        playerMovementScript = FindObjectOfType<PlayerMovement>();
        fireStoveAS.clip = fireAC;
        fireStoveAS.loop = true;
        fireStoveAS.Play();
    }

    public bool Interact(Interactor interactor)
    {
        if(HasWaterBucket() == true && hasExtinguishedFire ==false)
        {
            PutOutFire();
            return true;
        }
        else
        {
            //play detective voice line that says there's something in the firestove, but the fire is in the way
            //fireStoveAS.clip = firestoveLockedVoiceLine;
            //fireStoveAS.loop = false;
            //fireStoveAS.Play();
            
            return false;
        }
        
    }

    private bool HasWaterBucket()
    {
        return bucketScript.hasWater;
    }

    private void PutOutFire()
    {
        isBurning = false;
        hasExtinguishedFire = true;
        fireStoveAS.clip = fireExtinguishAC;
        fireStoveAS.loop = false;
        fireStoveAS.Play();
        playerMovementScript.PausePlayerMovement();

        LogManager.Instance.LogEvent($"{gameObject.name} firestove extinguished");

        // Start a coroutine to wait for audio clip
        StartCoroutine(PlayVoiceLineAfterClip(fireStoveAS.clip.length));
    }

    private IEnumerator PlayVoiceLineAfterClip(float clipLength)
    {
        //acts after the audio clip is finished
        yield return new WaitForSeconds(clipLength);

        //play reaction voiceline to extinguished stove
        //fireStoveAS.clip = firestoveOpenVoiceLine;
        //fireStoveAS.Play();

        // Start a coroutine to wait for audio clip
        StartCoroutine(DestroyASAfterClip(fireStoveAS.clip.length));
    }

    private IEnumerator DestroyASAfterClip(float clipLength)
    {
        //acts after the audio clip is finished
        yield return new WaitForSeconds(clipLength);
        
        Destroy(this.gameObject); //remove so it wont play even with interaction
        //Destroy(fireStoveAS); //remove the audiosource so it wont play even with interaction
        fireStoveMemory.layer = 6; //set the memorys layer from default to interactable
        playerMovementScript.ResumePlayerMovement();
    }
}
