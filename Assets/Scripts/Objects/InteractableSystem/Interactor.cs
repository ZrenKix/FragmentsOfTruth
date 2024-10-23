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

    private void Update()
    {
        _numFound = Physics.OverlapSphereNonAlloc(
            _interactionPoint.position,
            _interactionPointRadius,
            _colliders,
            _interactableMask
        );

        if (_numFound > 0 && Input.GetKeyDown(KeyCode.E))
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
                    audioSource.PlayOneShot(audioClip);
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
