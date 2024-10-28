using System.Collections;
using UnityEngine;

public class Locked : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioClip m_audioClip;
    [SerializeField] private AudioClip m_audioClip2;
    [SerializeField] private AudioSource m_audioSource;

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
        if (!m_audioSource.isPlaying)
        {
            m_audioSource.PlayOneShot(m_audioClip2);
        }

        if (m_freezePlayer)
        {
            m_playerMovement.PausePlayerMovement();
        }
        StartCoroutine(AfterAudio());
        gameObject.layer = LayerMask.NameToLayer("Default");

        return true;
    }

    private IEnumerator AfterAudio()
    {
        yield return new WaitForSeconds(m_audioClip.length);
        if (m_freezePlayer) m_playerMovement.ResumePlayerMovement();
        if (m_destoryAfterClip) Destroy(gameObject);
    }
}