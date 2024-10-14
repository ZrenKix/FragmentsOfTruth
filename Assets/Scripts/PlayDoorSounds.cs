using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayDoorSounds : MonoBehaviour
{
    [SerializeField] AudioSource m_AudioSource;
    [SerializeField] AudioClip[] audioClips;
    private bool isPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlayDoorAudioWithDelay());
    }

    // Coroutine to play audio clips with delay
    IEnumerator PlayDoorAudioWithDelay()
    {
        while (true)
        {
            if (!m_AudioSource.isPlaying)
            {
                PlayDoorAudio();
            }
            // Delay between clips (adjust as needed)
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
    }

    private void PlayDoorAudio()
    {
        // Pick a random sound from the array, excluding index 0
        int n = Random.Range(1, audioClips.Length);
        m_AudioSource.clip = audioClips[n];
        m_AudioSource.pitch = Random.Range(0.8f, 1.2f);
        m_AudioSource.PlayOneShot(m_AudioSource.clip);

        // Move picked sound to index 0 so it's not picked next time
        audioClips[n] = audioClips[0];
        audioClips[0] = m_AudioSource.clip;
    }
}
