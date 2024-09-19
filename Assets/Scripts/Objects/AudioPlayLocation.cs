using System;
using System.Collections;
using UnityEngine;

public class AudioPlayLocation : MonoBehaviour
{
    [SerializeField] private RoomBuilder[] m_locations;   
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private float m_fadeDuration;
    private float m_endVolume;
    private GameObject player;                              

    private void Start()
    {
        m_endVolume = m_audioSource.volume;
        // Initialize the AudioSource if not assigned in the Inspector
        if (m_audioSource == null)
        {
            m_audioSource = GetComponent<AudioSource>();
            if (m_audioSource == null)
            {
                Debug.LogError("AudioSource component not found on the GameObject.");
            }
        }

        // Find the player GameObject by tag
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player with tag 'Player' not found in the scene.");
        }

        m_audioSource.Stop();
    }

    private void FixedUpdate()
    {
        // If player or AudioSource is not set, exit early
        if (player == null || m_audioSource == null)
            return;

        bool isPlayerInAnyLocation = false;

        // Iterate through all locations to check if the player is inside any room
        foreach (RoomBuilder location in m_locations)
        {
            if (location == null)
            {
                Debug.LogWarning("One of the locations in m_locations is not assigned.");
                continue;
            }

            // Use RoomBuilder's method to check if the player is inside the room
            if (location.IsObjectInsideRoom(player))
            {
                isPlayerInAnyLocation = true;
                break; // No need to check further if player is inside at least one room
            }
        }

        // Manage AudioSource based on player's presence
        if (isPlayerInAnyLocation)
        {
            if (!m_audioSource.isPlaying)
            {
                StartCoroutine(FadeInAudio());
            }
        }
        else
        {
            if (m_audioSource.isPlaying)
            {
                StartCoroutine(FadeOutAudio());
            }
        }
    }

    private IEnumerator FadeOutAudio()
    {
        float startVolume = m_audioSource.volume;
        for (float t = 0; t < m_fadeDuration; t+= Time.deltaTime)
        {
            m_audioSource.volume = Mathf.Lerp(startVolume, 0, t / m_fadeDuration);
            yield return null;
        }
        m_audioSource.volume = 0;
        m_audioSource.Stop();
    }

    private IEnumerator FadeInAudio()
    {
        m_audioSource.volume = 0;
        m_audioSource.Play();
        for (float t = 0; t <  m_fadeDuration; t += Time.deltaTime)
        {
            m_audioSource.volume = Mathf.Lerp(0, m_endVolume, t / m_fadeDuration);
            yield return null;
        }
        m_audioSource.volume = m_endVolume;
    }
}
