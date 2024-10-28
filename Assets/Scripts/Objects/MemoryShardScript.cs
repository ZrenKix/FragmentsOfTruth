using System.Collections;
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

        // Pausa spelarens rörelse och spela upp minnesljudet
        m_playerMovement.PausePlayerMovement();
        m_audioManager.PauseAllAudioSourcesExcept(m_audioSource);
        m_isMemorySequenceActive = true;

        m_audioSource.clip = m_buildUpAudioClip;
        StartCoroutine(AfterFlashBack());

        // Meddela MemoryShardManager att en shard har hittats
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
