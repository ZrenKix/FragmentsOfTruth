using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryShardScript : MonoBehaviour, IInteractable
{
    [Header("Audio")]
    [SerializeField] private AudioClip m_buildUpAudioClip;
    [SerializeField] private AudioClip m_memoryAudioClip;
    [SerializeField] private AudioClip m_afer_memoryAudioClip;
    [SerializeField] private AudioClip m_passiveAudioClip;
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
    
    private void Awake()
    {
        m_audioSource.clip = m_passiveAudioClip;
    }

    public string InteractionPrompt { get; }
    public bool Interact(Interactor interactor)
    {
        Debug.Log("Memory interacted");

        if (m_audioSource.clip.name.Equals(m_memoryAudioClip.name) || m_audioSource.clip.name.Equals(m_buildUpAudioClip.name))
        {
            Debug.Log(gameObject.name + ": is already playing memory clip");
            return false;
        }

        // Pause player movement and play memory audio
        m_playerMovement.PausePlayerMovement();
        m_audioManager.PauseAllAudioSourcesExcept(m_audioSource);

        m_audioSource.clip = m_buildUpAudioClip;
        StartCoroutine(AfterFlashBack());

        // Notify the MemoryShardManager that a shard has been found
        MemoryShardManager.Instance.ShardFound();

        Debug.Log("Playing memory");
        return true;
    }

    private IEnumerator AfterFlashBack()
    {
        yield return new WaitForSeconds(m_buildUpAudioClip.length);
        m_audioSource.clip = m_memoryAudioClip;
        StartCoroutine(ResumeAudioAfterMemory());
    }

    private IEnumerator ResumeAudioAfterMemory()
    {
        yield return new WaitForSeconds(m_memoryAudioClip.length + m_resumeAudioDelay);
        m_audioSource.clip = m_passiveAudioClip;
        m_playerMovement.ResumePlayerMovement();
        m_audioManager.ResumeAllAudioSources();
    }
}