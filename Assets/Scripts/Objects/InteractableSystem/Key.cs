using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Key : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject m_object;
    [SerializeField] private bool m_destoryAfterInteraction;
    [SerializeField] private AudioClip m_clip;
    [SerializeField] private AudioClip m_pickup;
    [SerializeField] private AudioSource source;

    private void Start()
    {
        if (!m_clip.IsUnityNull() && source != null) source.clip = m_clip;
    }
    public string InteractionPrompt { get; }

    public bool Interact(Interactor interactor) 
    {
        if (m_object == null) return false;
        m_object.layer = LayerMask.NameToLayer("Interactable");
        source.clip = m_pickup;
        source.loop = false;
        if (source != null) source.Play();

        if (m_destoryAfterInteraction) Destroy(this.gameObject);
        
        return true;
    }

}