using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bucket : MonoBehaviour, IInteractable
{
    //Av Linn Li


    [SerializeField] private AudioClip pickUpAC;
    [SerializeField] private AudioClip bucketAC;
    [SerializeField] private AudioSource bucketAS;
    public bool pickedUpBucket = false;
    public bool hasWater = false;

    public string InteractionPrompt { get; }

    void Start()
    {
        //audioclip is the bucket sound, for locatin the bucket
        bucketAS.clip = bucketAC;
        bucketAS.loop = true;
    }

    public bool Interact(Interactor interactor)
    {
        pickedUpBucket = true;

        //change audioclip to pickup sound
        //bucketAS.clip = pickUpAC;
        //bucketAS.loop = false;

        // Start a coroutine to wait for audio clip
        //StartCoroutine(DestroyASAfterClip(bucketAS.clip.length));

        Destroy(this.gameObject);
        return true;
    }

    private IEnumerator DestroyASAfterClip(float clipLength)
    {
        //acts after the audio clip is finished
        yield return new WaitForSeconds(clipLength);
        Destroy(this.gameObject);
    }
}
