using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bucket : MonoBehaviour, IInteractable
{
    //Av Linn Li


    public bool pickedUpBucket = false;
    public bool hasWater = false;

    public string InteractionPrompt { get; }

    public bool Interact(Interactor interactor)
    {
        pickedUpBucket = true;
        //Play pickup sound??
        Destroy(this.gameObject);
        return true;
    }
}
