using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateSoundInteract : MonoBehaviour, IInteractable
{
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip clip;
    [SerializeField] GameObject keyItem;

    public string InteractionPrompt { get; }

    public bool Interact(Interactor interactor)
    {
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
