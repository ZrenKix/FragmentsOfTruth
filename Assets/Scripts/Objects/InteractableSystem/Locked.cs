using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class Locked : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioClip m_audioClip;
    [SerializeField] private AudioClip m_lockedAudioClip;
    [SerializeField] private AudioClip m_trunkVoiceKeyAudioClip;
    //[SerializeField] private AudioClip m_audioClip2;
    //[SerializeField] private AudioClip memoryClip;
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private AudioSource audioS;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private List<AudioClip> audioClips; // List with all clips that should play
    private int currentClipIndex = 0;

    [SerializeField] private bool m_freezePlayer = true;
    [SerializeField] private bool m_destoryAfterClip = false;

    private PlayerMovement m_playerMovement;

    [SerializeField] private string m_interactionPrompt;
    public string InteractionPrompt => m_interactionPrompt;

    private void Start()
    {
        if (m_freezePlayer)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                m_playerMovement = playerObject.GetComponent<PlayerMovement>();
            }
        }
    }
    public bool Interact(Interactor interactor)
    {
        int trunkLayerIndex = LayerMask.NameToLayer("Trunk");
        Debug.Log($"Interact called on {gameObject.name}, gameObject.layer: {gameObject.layer}, Trunk Layer Index: {trunkLayerIndex}");

        // Check if the object's layer is "Trunk" by comparing layer indices
        if (gameObject.layer == LayerMask.NameToLayer("Trunk"))
        {
            // Play the locked sound
            m_audioSource.clip = null;
            m_audioSource.PlayOneShot(m_lockedAudioClip, 1);

            return false; // Interaction did not proceed
        }
        else
        {
            // Proceed with normal interaction
            if (m_freezePlayer)
            {
                m_playerMovement.PausePlayerMovement();
            }
            StartCoroutine(PlayClipsSequentially(m_audioClip.length));

            // Change the object's layer to "Default" to prevent re-interaction
            gameObject.layer = LayerMask.NameToLayer("Default");

            // Start the interaction sequence coroutine
            StartCoroutine(PlayInteractionSequence());

            return true; // Interaction succeeded
        }
    }

    IEnumerator PlayInteractionSequence()
    {
        // If m_trunkVoiceKeyAudioClip is not null, play it and wait
        if (m_trunkVoiceKeyAudioClip != null)
        {
            m_audioSource.clip = null;
            m_audioSource.PlayOneShot(m_trunkVoiceKeyAudioClip, 1);
            yield return new WaitForSeconds(m_trunkVoiceKeyAudioClip.length);
        }

        // Play m_audioClip (chest opening sound) and wait
        m_audioSource.clip = null;
        m_audioSource.PlayOneShot(m_audioClip, 1);
        yield return new WaitForSeconds(m_audioClip.length);

    }
    // private IEnumerator AfterAudio()
    // {
    //     yield return new WaitForSeconds(memoryClip.length);
    //     if (m_freezePlayer) m_playerMovement.ResumePlayerMovement();
    //     if (m_destoryAfterClip) Destroy(gameObject);
    // }

    IEnumerator PlayClipsSequentially(float delay) {
        yield return new WaitForSeconds(delay);

        while (currentClipIndex < audioClips.Count) {
            //playerMovement.PausePlayerMovement(); //Disable the player's movement
            audioMixer.SetFloat("GameSoundsVolume", -80f); //Lowers all other sounds

            audioS.clip = audioClips[currentClipIndex];
            audioS.Play();

            // Waits till the clip has played to the end
            yield return new WaitForSeconds(audioClips[currentClipIndex].length);

            // Goes to next clip
            currentClipIndex++;
        }
       
        audioMixer.SetFloat("GameSoundsVolume", 0f); //Unmutes the game sounds
        if (m_freezePlayer) m_playerMovement.ResumePlayerMovement();
        if (m_destoryAfterClip) Destroy(gameObject);
    }

    // private IEnumerator WaitAndPlayMemory(float delay) // Nora Wennerberg wrote this method
    // {
    //     yield return new WaitForSeconds(delay);

    //     audioMixer.SetFloat("GameSoundsVolume", -80f); // Lowers all other sounds
    //     audioS.PlayOneShot(memoryClip);

    //     // Wait till the clip has finished playing
    //     yield return new WaitForSeconds(memoryClip.length);

    //     audioMixer.SetFloat("GameSoundsVolume", 0f); // Unmutes the game sounds

    //     if (m_freezePlayer) m_playerMovement.ResumePlayerMovement();
    //     if (m_destoryAfterClip) Destroy(gameObject);
    // }
}

