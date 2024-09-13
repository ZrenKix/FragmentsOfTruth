using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrationTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip m_audioClip;
    [SerializeField] private AudioSource m_audioSource;
    private void OnTriggerEnter(Collider other)
    {
        if(m_audioClip != null && other.gameObject.tag.Equals("Player")){
            m_audioSource.PlayOneShot(m_audioClip);
        }
    }
}
