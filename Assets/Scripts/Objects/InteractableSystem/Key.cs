using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Key : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject m_object;
    [SerializeField] private bool m_destoryAfterInteraction;
    public string InteractionPrompt { get; }
    public bool Interact(Interactor interactor)
    {
        if (m_object == null) return false;

        m_object.layer = LayerMask.NameToLayer("Interactable");
        if (m_destoryAfterInteraction) Destroy(this.gameObject);
        return true;
    }
}