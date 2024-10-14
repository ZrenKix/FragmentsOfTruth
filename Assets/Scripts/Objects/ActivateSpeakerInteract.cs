using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateSpeakerOnInteract : MonoBehaviour, IInteractable
{
    private AudioSource source;
    [SerializeField] AudioClip clip;
    [SerializeField] GameObject speaker;
    [SerializeField] float delayTime;



    // Start is called before the first frame update
    void Start()
    {
        if(!source)
        {
            source = speaker.GetComponent<AudioSource>();
        }
    }
    
    public string InteractionPrompt { get; }

    public bool Interact(Interactor interactor)
    {
        if(source.isPlaying)
        {
            return false;
        }



        PlayAudio();

        return true;
    }


    private void PlayAudio()
    {
        source.clip = clip;
        source.PlayDelayed(delayTime);
    }

}
