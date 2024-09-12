using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Key : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject _door;

    public string InteractionPrompt { get; }
    public bool Interact(Interactor interactor)
    {
        if (_door == null) return false;

        _door.GetComponent<Locked>().bInteractable = true;
        Destroy(this.gameObject);
        return true;
    }
}
