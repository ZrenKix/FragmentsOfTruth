using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCollisionCheck : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource m_audioSource;   // Reference to the audio source
    [SerializeField] private AudioClip[] m_collisionAudioClip; // Audio clip to play on collision
    [SerializeField] private PlayerMovement m_playerMovement; // Reference to the player's movement script

    private bool m_isColliding = false; // To track if the player is stuck
    [SerializeField] private float collisionAngleThreshold = 30f; // Adjusted angle to determine if it's a wall collision
    [SerializeField] private float minCollisionSpeed = 0.1f; // Minimum speed for triggering collision sound

    private void Update()
    {
        // If the player is colliding with a wall and is moving, start looping the collision audio
        if (m_isColliding && m_playerMovement.IsMoving() && !m_audioSource.isPlaying)
        {
            //m_audioSource.clip = m_collisionAudioClip;
            m_audioSource.loop = true;  // Ensure the audio loops while stuck
            m_audioSource.Play();       // Start playing the collision sound
        }
        // Stop the audio if the player is no longer colliding or has stopped moving
        else if ((!m_isColliding || !m_playerMovement.IsMoving()) && m_audioSource.isPlaying)
        {
            m_audioSource.Stop();  // Stop the audio immediately when the collision ends or player stops moving
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Use both OnCollisionEnter and OnCollisionStay to ensure we're detecting collisions frequently
        HandleCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        // Continuously check for wall-like collisions while the player remains in contact
        HandleCollision(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        // When the player exits a collision, reset the flag
        m_isColliding = false;
    }

    // Helper method to check if the collision is with a wall based on the angle of the surface
    private void HandleCollision(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            float collisionAngle = Vector3.Angle(contact.normal, Vector3.up);

            // Check if the collision is with a vertical surface (wall) and if the player is moving at a minimum speed
            if (collisionAngle > collisionAngleThreshold && m_playerMovement.IsMoving())
            {
                m_isColliding = true;
                if(!m_audioSource.isPlaying) PlayWallHitAudio();
                return;
            }
        }
        m_isColliding = false;  // If no valid wall collision is detected, reset the flag
    }

    private void PlayWallHitAudio()
    {
        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_collisionAudioClip.Length);
        m_audioSource.clip = m_collisionAudioClip[n];
        m_audioSource.PlayOneShot(m_audioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        m_collisionAudioClip[n] = m_collisionAudioClip[0];
        m_collisionAudioClip[0] = m_audioSource.clip;
    }
}
