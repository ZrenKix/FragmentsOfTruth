using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WallCollisionCheck : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private AudioClip m_collisionAudioClip;
    [SerializeField] private AudioClip m_scrapeAudioClip;
    [SerializeField] private PlayerMovement m_playerMovement;

    private bool m_isColliding = false;
    private bool m_isScraping = false;
    [SerializeField] private float collisionAngleThreshold = 45f; // Increased for testing
    [SerializeField] private float groundAngleThreshold = 10f;    // Reduced for testing
    [SerializeField] private float minCollisionHeight = -1f;      // Reduced for testing

    private bool hasPlayedOwSound = false;

    private void Update()
    {
        // Play "ow" sound once when a direct collision occurs
        if (m_isColliding)
        {
            if (!hasPlayedOwSound)
            {
                m_audioSource.PlayOneShot(m_collisionAudioClip);
                hasPlayedOwSound = true;
            }

            // Stop the scrape sound if it was playing
            if (m_audioSource.isPlaying && m_audioSource.clip == m_scrapeAudioClip)
            {
                m_audioSource.Stop();
            }
        }
        // Play scrape sound continuously while scraping
        else if (m_isScraping && m_playerMovement.IsMoving())
        {
            // Reset the flag for "ow" sound since we're no longer in direct collision
            hasPlayedOwSound = false;

            if (!m_audioSource.isPlaying || m_audioSource.clip != m_scrapeAudioClip)
            {
                m_audioSource.clip = m_scrapeAudioClip;
                m_audioSource.loop = true;
                m_audioSource.Play();
            }
        }
        // Stop all sounds when not colliding
        else
        {
            hasPlayedOwSound = false;

            if (m_audioSource.isPlaying)
            {
                m_audioSource.Stop();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        m_isColliding = false;
        m_isScraping = false;
        hasPlayedOwSound = false;

        if (m_audioSource.isPlaying)
        {
            m_audioSource.Stop();
        }
    }

    private void HandleCollision(Collision collision)
    {
        Vector3 lookDirection = m_playerMovement.GetLookDirection();

        if (lookDirection == Vector3.zero)
        {
            m_isColliding = false;
            m_isScraping = false;
            return;
        }

        bool directCollisionDetected = false;
        bool scrapingDetected = false;

        foreach (ContactPoint contact in collision.contacts)
        {
            // ** For testing, temporarily bypass ground and height checks **
            // float groundCollisionAngle = Vector3.Angle(contact.normal, Vector3.up);
            // if (groundCollisionAngle <= groundAngleThreshold)
            // {
            //     continue;
            // }

            // float contactHeight = contact.point.y - transform.position.y;
            // if (contactHeight <= minCollisionHeight)
            // {
            //     continue;
            // }

            float collisionAngle = Vector3.Angle(lookDirection, -contact.normal);
            Debug.Log($"Collision Angle: {collisionAngle}");

            if (collisionAngle <= collisionAngleThreshold)
            {
                directCollisionDetected = true;
                Debug.Log("Direct collision detected.");
                break;
            }
            else
            {
                scrapingDetected = true;
                Debug.Log("Scraping detected.");
            }
        }

        if (directCollisionDetected)
        {
            m_isColliding = true;
            m_isScraping = false;
        }
        else if (scrapingDetected)
        {
            m_isColliding = false;
            m_isScraping = true;
        }
        else
        {
            m_isColliding = false;
            m_isScraping = false;
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Vector3 playerPosition = transform.position;

        Vector3 lookDirection;
        if (Application.isPlaying)
        {
            lookDirection = m_playerMovement.GetLookDirection();
        }
        else
        {
            lookDirection = transform.forward;
        }
        lookDirection.Normalize();

        Handles.color = new Color(1, 0, 0, 0.2f);

        float arcRadius = 2f;
        float collisionAngle = collisionAngleThreshold * 2;

        Handles.DrawSolidArc(playerPosition, Vector3.up, Quaternion.Euler(0, -collisionAngleThreshold, 0) * lookDirection, collisionAngle, arcRadius);
#endif
    }
}
