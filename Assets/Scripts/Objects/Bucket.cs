using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bucket : MonoBehaviour, IInteractable
{
    //Av Linn Li


    [SerializeField] private AudioClip pickupVoiceLine;
    [SerializeField] private AudioClip bucketAC;
    [SerializeField] private AudioSource bucketAS;
    public bool pickedUpBucket = false;
    public bool hasWater = false;

    [SerializeField] private string m_interactionPrompt;
    public string InteractionPrompt => m_interactionPrompt;

    void Start()
    {
        //bucketAC is the bucket sound, for locating the bucket
        bucketAS.clip = bucketAC;
        bucketAS.loop = true;
    }

    public bool Interact(Interactor interactor)
    {
        pickedUpBucket = true;

        //play the voiceline reacting to the bucket
        bucketAS.clip = pickupVoiceLine;
        bucketAS.loop = false;

        StartCoroutine(DestroyASAfterClip(bucketAS.clip.length));
        return true;
    }

    private IEnumerator DestroyASAfterClip(float clipLength)
    {
        //acts after the audio clip is finished
        yield return new WaitForSeconds(clipLength);
        Destroy(this.gameObject);
    }
}
