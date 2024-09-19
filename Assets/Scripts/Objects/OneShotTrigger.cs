using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneShotTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private AudioClip m_audioClip;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            m_audioSource.PlayOneShot(m_audioClip);
            Destroy(gameObject);
        }
    }
}