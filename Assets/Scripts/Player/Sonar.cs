using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class Sonar : MonoBehaviour
{
    [SerializeField] private AudioClip m_audioClip;
    [SerializeField] private AudioSource m_audioSource;

    [SerializeField] private float m_rayMaxLength = 10f;

    private void Start()
    {
        m_audioSource.clip = m_audioClip;
    }

    private void Update()
    {
        Vector3 rayDirection = new Vector3(0.5f, 0.5f, 0);
        Ray ray = Camera.main.ViewportPointToRay(rayDirection);
        Debug.DrawRay(ray.origin, ray.direction * m_rayMaxLength, Color.red);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, m_rayMaxLength))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {

                float normalizedDistance = Mathf.InverseLerp(m_rayMaxLength, 0, hit.distance); // Normalizes the distance
                m_audioSource.pitch = Mathf.Lerp(0, 3, normalizedDistance);  // Maps the normalized value to pitch range 0-3

                m_audioSource.Play();

                Debug.DrawRay(ray.origin, ray.direction * m_rayMaxLength, Color.green);
            }
            else
            {
            }
        }
    }
}