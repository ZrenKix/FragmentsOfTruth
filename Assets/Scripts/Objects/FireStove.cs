using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireStove : MonoBehaviour, IInteractable
{
    //Av Linn Li


    [SerializeField] bool isBurning = true;
    [SerializeField] private AudioClip fireExtinguishAC;
    [SerializeField] private AudioSource fireStoveAS;
    [SerializeField] private GameObject fireStoveMemory;
    private bool isExtinguishingFire = false; //to prevent audio source from double playing sound

    private Bucket bucketScript;
    private PlayerMovement playerMovementScript;

    public string InteractionPrompt { get; }

    // Start is called before the first frame update
    void Start()
    {
        bucketScript = FindObjectOfType<Bucket>();
        playerMovementScript = FindObjectOfType<PlayerMovement>();
        fireStoveAS.clip = fireExtinguishAC;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool Interact(Interactor interactor)
    {
        if(HasWaterBucket() == true && isExtinguishingFire ==false)
        {
            PutOutFire();
            return true;
        }
        else
        {
            //play detective voice line that says there's something in the firestove, but the fire is in the way
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
        isExtinguishingFire = true;
        fireStoveAS.Play();
        playerMovementScript.PausePlayerMovement();

        // Start a coroutine to wait for audio clip
        StartCoroutine(DestroyASAfterClip(fireStoveAS.clip.length));
    }

    private IEnumerator DestroyASAfterClip(float clipLength)
    {
        //acts after the audio clip is finished
        yield return new WaitForSeconds(clipLength);
        Destroy(fireStoveAS); //remove the audiosource so it wont play even with interaction
        fireStoveMemory.layer = 6; //set the memorys layer from default to interactable
        playerMovementScript.ResumePlayerMovement();
        isExtinguishingFire = false;
    }
}
