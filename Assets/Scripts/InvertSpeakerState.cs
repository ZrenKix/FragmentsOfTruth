using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertSpeakerState : MonoBehaviour
{
    [SerializeField] AudioSource[] audioSources;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            foreach (AudioSource source in audioSources)
            {
                if (source == null) continue;
                source.enabled = !source.enabled;
            }
        }
    }
}
