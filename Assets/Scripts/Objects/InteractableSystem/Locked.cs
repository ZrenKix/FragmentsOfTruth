using System.Collections;
using UnityEngine;

public class Locked : MonoBehaviour, IInteractable
{
    public bool bInteractable = false;

    [SerializeField] private AudioClip _audioClip;
    [SerializeField] private AudioSource _audioSource;

    public string InteractionPrompt { get; }
    public bool Interact(Interactor interactor)
    {
        if (!bInteractable) return false;

        Debug.Log("Opening Door");
        _audioSource.PlayOneShot(_audioClip);

        // Start a coroutine to destroy the object after the audio clip finishes
        StartCoroutine(DestroyAfterAudio());

        return bInteractable;
    }

    private IEnumerator DestroyAfterAudio()
    {
        // Wait for the audio clip duration
        yield return new WaitForSeconds(_audioClip.length);

        // Destroy the object
        Destroy(this.gameObject);
    }
}