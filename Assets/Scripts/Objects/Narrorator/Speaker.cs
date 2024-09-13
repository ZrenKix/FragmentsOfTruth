using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker : MonoBehaviour
{
    [SerializeField] private AudioClip[] m_audioClips;
    [SerializeField] private AudioSource m_audioSource;

    void PlayClipInList(AudioClip eventAudio)
    {
        if (eventAudio != null) { return; }

    }
}
