using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class Locked : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioClip m_audioClip;
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
        m_audioSource.clip = null;
        m_audioSource.PlayOneShot(m_audioClip, 1);
        // if (!m_audioSource.isPlaying)
        // {
        //     m_audioSource.PlayOneShot(m_audioClip2);
        // }

        if (m_freezePlayer)
        {
            m_playerMovement.PausePlayerMovement();
        }

        Debug.Log("audioS: " + m_audioSource.isPlaying);

        StartCoroutine(PlayClipsSequentially(m_audioClip.length));

        //StartCoroutine(AfterAudio());
        gameObject.layer = LayerMask.NameToLayer("Default");

        return true;
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