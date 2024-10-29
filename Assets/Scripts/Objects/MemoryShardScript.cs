using System.Collections;
using UnityEngine;

public class MemoryShardScript : MonoBehaviour, IInteractable
{
    [Header("Audio")]
    [SerializeField] private AudioClip m_CueMinneAudioClip; // Additional audio clip to play
    [SerializeField] private AudioClip m_buildUpAudioClip;
    [SerializeField] private AudioClip m_memoryAudioClip;
    [SerializeField] private AudioClip m_afer_memoryAudioClip;
    [SerializeField] private AudioClip m_passiveAudioClip;
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private float m_resumeAudioDelay;
    private AudioManager m_audioManager;

    [SerializeField] private string m_prompt;

    private PlayerMovement m_playerMovement;

    [SerializeField] private string m_interactionPrompt;
    public string InteractionPrompt => m_interactionPrompt;

    private bool m_isMemorySequenceActive = false;

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

    public bool Interact(Interactor interactor)
    {
        Debug.Log("Memory interacted");

        if (m_audioSource.clip.name.Equals(m_memoryAudioClip.name) || m_audioSource.clip.name.Equals(m_buildUpAudioClip.name))
        {
            Debug.Log(gameObject.name + ": is already playing memory clip");
            return false;
        }

        // Pause player's movement and play the memory sequence
        m_playerMovement.PausePlayerMovement();
        m_audioManager.PauseAllAudioSourcesExcept(m_audioSource);
        m_isMemorySequenceActive = true;

        // Start the memory sequence coroutine
        StartCoroutine(PlayMemorySequence());

        // Notify MemoryShardManager that a shard has been found
        MemoryShardManager.Instance.ShardFound();

        Debug.Log("Playing memory");
        return true;
    }

    private IEnumerator PlayMemorySequence()
    {
        // If the object's name is "Memory", play the additional audio clip
        if (gameObject.name == "Memory" && m_CueMinneAudioClip != null)
        {
            m_audioSource.clip = m_CueMinneAudioClip;
            m_audioSource.Play();
            yield return new WaitForSeconds(m_CueMinneAudioClip.length);
        }

        // Play the build-up audio clip
        m_audioSource.clip = m_buildUpAudioClip;
        m_audioSource.Play();
        yield return new WaitForSeconds(m_buildUpAudioClip.length);

        // Play the memory audio clip
        m_audioSource.clip = m_memoryAudioClip;
        m_audioSource.Play();
        yield return new WaitForSeconds(m_memoryAudioClip.length);

        // Play the after-memory audio clip if assigned
        if (m_afer_memoryAudioClip != null)
        {
            m_audioSource.clip = m_afer_memoryAudioClip;
            m_audioSource.Play();
            yield return new WaitForSeconds(m_afer_memoryAudioClip.length);
        }

        // Wait for any additional delay
        yield return new WaitForSeconds(m_resumeAudioDelay);

        // Resume normal game state
        ResumeAfterMemory();
    }

    private void ResumeAfterMemory()
    {
        m_audioSource.clip = null;
        m_playerMovement.ResumePlayerMovement();
        m_audioManager.ResumeAllAudioSources();
        m_isMemorySequenceActive = false;
    }

    private void Update()
    {
        if (m_isMemorySequenceActive && Input.GetKeyDown(KeyCode.Escape))
        {
            StopAllCoroutines();
            ResumeAfterMemory();
        }
    }
}
