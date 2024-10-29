using System;
using System.Collections.Generic;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip audioClip;

    [SerializeField] public Transform _interactionPoint;
    [SerializeField] public float _interactionPointRadius = 0.5f;
    [SerializeField] private LayerMask _interactableMask;

    private readonly Collider[] _colliders = new Collider[3];
    [SerializeField] private int _numFound;

    private void Start()
    {
        // Initialize the layer mask to include "Interactable" and "Trunk"
        _interactableMask = LayerMask.GetMask("Interactable", "Trunk");
    }
    private void Update()
    {
        _numFound = Physics.OverlapSphereNonAlloc(
            _interactionPoint.position,
            _interactionPointRadius,
            _colliders,
            _interactableMask
        );

        if (Input.GetKeyDown(KeyCode.E))
        {
            LogManager.Instance.StoreValue("Pressed E", 1);

            if (_numFound > 0)
            {
                // Optionally, you can find the closest collider or interact with all of them
                Collider collider = _colliders[0]; // You can modify this to select a specific collider

                // Retrieve all IInteractable components on the collider's GameObject
                IInteractable[] interactables = collider.GetComponents<IInteractable>();
                foreach (IInteractable interactable in interactables)
                {
                    // Call Interact on each interactable component
                    if (interactable.Interact(this))
                    {
                        LogManager.Instance.LogEvent($"{gameObject.name} did interaction: {interactable.InteractionPrompt}, {interactable}");
                        LogManager.Instance.StoreValue("Total interacted objects", 1);
                        audioSource.PlayOneShot(audioClip);
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_interactionPoint.position, _interactionPointRadius);
    }
}
