using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locked : MonoBehaviour, IInteractable
{
    public bool bInteractable = false;

    [SerializeField] private AudioClip _audioClip;
    [SerializeField] private AudioSource _audioSource;

    public string InteractionPrompt { get; }
    public bool Interact(Interactor interactor)
    {
        Debug.Log("Opening Door");
        _audioSource.PlayOneShot(_audioClip);
        Destroy(this.gameObject);
        return bInteractable;
    }
}
