using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource[] m_audioSources;
    void Start()
    {
        m_audioSources = FindObjectsOfType<AudioSource>();
    }

    internal void PauseAllAudioSourcesExcept(AudioSource[] audioSources)
    {

        foreach(AudioSource audioSource in m_audioSources)
        {
            if (audioSource == null)
            {
                return;
            }
            if (audioSources.Contains<AudioSource>(audioSource)) 
            {
                return;
            }
            audioSource.Pause();
        }
    }

    internal void PauseAllAudioSourcesExcept(AudioSource audioSource)
    {
        foreach (AudioSource _audioSource in m_audioSources)
        {
            if (_audioSource != null && !_audioSource.Equals(audioSource))
            {
                _audioSource.Pause(); ;
            }
        }
    }

    internal void ResumeAllAudioSources()
    {
        foreach (AudioSource audioSource in m_audioSources)
        {
            if (audioSource != null)
            {
                audioSource.UnPause();
            }
        }
    }
}
