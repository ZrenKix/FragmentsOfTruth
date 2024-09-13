using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryShardScript : MonoBehaviour, IInteractable
{
    [Header("Audio")]
    [SerializeField] private AudioClip m_audioClip;
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private float m_resumeAudioDelay;
    private AudioManager m_audioManager;

    [SerializeField] private string m_prompt;

    private PlayerMovement m_playerMovement;

    private void Start()
    {
        m_audioManager = FindObjectOfType<AudioManager>();
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            m_playerMovement = playerObject.GetComponent<PlayerMovement>();
        }
    }

    public string InteractionPrompt { get; }
    public bool Interact(Interactor interactor)
    {
        if (m_audioSource.isPlaying)
        {
            Debug.Log(gameObject.name + ": is already playing audio clip");
            return false;
        }

        m_audioSource.PlayOneShot(m_audioClip);
        m_playerMovement.PausePlayerMovement();
        m_audioManager.PauseAllAudioSourcesExcept(m_audioSource);

        StartCoroutine(ResumeAudioAfterMemory());

        Debug.Log("Playing memory");
        return true;
    }

    private IEnumerator ResumeAudioAfterMemory()
    {
        yield return new WaitForSeconds(m_audioClip.length + m_resumeAudioDelay);
        m_playerMovement.ResumePlayerMovement();
        m_audioManager.ResumeAllAudioSources();
    }
}