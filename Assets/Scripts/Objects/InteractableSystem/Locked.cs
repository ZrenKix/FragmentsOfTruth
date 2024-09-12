using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locked : MonoBehaviour, IInteractable
{
    public bool bInteractable = false;

    public string InteractionPrompt { get; }
    public bool Interact(Interactor interactor)
    {
        Debug.Log("Opening Door");
        Destroy(this.gameObject);
        return bInteractable;
    }
}
