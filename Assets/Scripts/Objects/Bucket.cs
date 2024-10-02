using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bucket : MonoBehaviour, IInteractable
{
    //Av Linn Li


    [SerializeField] private AudioClip pickUpAC;
    [SerializeField] private AudioSource bucketAS;
    public bool pickedUpBucket = false;
    public bool hasWater = false;

    public string InteractionPrompt { get; }

    void Start()
    {
        bucketAS.clip = pickUpAC;
    }

    public bool Interact(Interactor interactor)
    {
        pickedUpBucket = true;
        bucketAS.Play();
        // Start a coroutine to wait for audio clip
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
