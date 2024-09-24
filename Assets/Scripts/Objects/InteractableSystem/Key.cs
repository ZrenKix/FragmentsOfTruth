using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Key : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject m_object;
    [SerializeField] private bool m_destoryAfterInteraction;
    [SerializeField] private AudioClip m_clip;
    [SerializeField] private AudioSource source;
    public string InteractionPrompt { get; }


    public bool Interact(Interactor interactor)
    {
        source.clip = m_clip;
        if (m_object == null) return false;

        m_object.layer = LayerMask.NameToLayer("Interactable");
        source.Play();
        if (m_destoryAfterInteraction) Destroy(this.gameObject, m_clip.length);
        return true;
    }

  
}