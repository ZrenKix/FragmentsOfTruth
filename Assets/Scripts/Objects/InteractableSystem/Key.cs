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
    [SerializeField] private string m_interactionPrompt;
    public string InteractionPrompt => m_interactionPrompt;

    private void Start()
    {
        if (!m_clip.IsUnityNull() && source != null) source.clip = m_clip;
    }

    public bool Interact(Interactor interactor) 
    {
        if (m_object == null) return false;
        m_object.layer = LayerMask.NameToLayer("Interactable");
        if (source != null) source.Play();

        if (m_destoryAfterInteraction) Destroy(this.gameObject);
        
        return true;
    }

}