using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryShardScript : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioClip _audioClip;
    [SerializeField] private AudioSource _audioSource;

    [SerializeField] private string _prompt;

    private bool bPlaying = false;

    public string InteractionPrompt { get; }
    public bool Interact(Interactor interactor)
    {
        if (bPlaying)
        {
            Debug.Log(gameObject.name + ": is already playing audio clip");
            return false;
        }
        bPlaying = true;
        Debug.Log("Playing memory");
        return true;
    }
}
