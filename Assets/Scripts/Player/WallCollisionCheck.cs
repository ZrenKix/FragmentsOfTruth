using UnityEngine;

public class WallCollisionCheck : MonoBehaviour
{
    public float scrapingThreshold = 5f;  // Velocity threshold for scraping sound
    public float owThreshold = 10f;       // Velocity threshold for "ow" sound

    public AudioClip scrapingSound;       // Audio clip for scraping sound
    public AudioClip owSound;             // Audio clip for "ow" sound

    private AudioSource audioSource;      // AudioSource component
    private Rigidbody playerRigidbody;    // Rigidbody of the player

    private bool owSoundPlayed = false;   // Flag to track if "ow" sound has been played

    void Start()
    {
        // Get the AudioSource component, or add one if it doesn't exist
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Get the player's Rigidbody component from the parent
        playerRigidbody = GetComponentInParent<Rigidbody>();
        if (playerRigidbody == null)
        {
            Debug.LogError("Player Rigidbody not found in parent objects.");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plane"))
        {
            Debug.Log("Player has made contact with a plane.");

            // Check if the "ow" sound should be played
            float speed = playerRigidbody.velocity.magnitude;
            if (speed > owThreshold && !owSoundPlayed)
            {
                audioSource.PlayOneShot(owSound);
                owSoundPlayed = true;
                Debug.Log("Ow sound played.");
            }
            else if (speed > scrapingThreshold)
            {
                audioSource.PlayOneShot(scrapingSound);
                Debug.Log("Scraping sound played.");
            }

            // Determine if contact is head-on or scraping
            Vector3 collisionNormal = collision.contacts[0].normal;
            float angle = Vector3.Angle(playerRigidbody.velocity, -collisionNormal);
            if (angle < 45f)
            {
                Debug.Log("Head-on contact with the plane.");
            }
            else
            {
                Debug.Log("Scraping along the plane.");
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plane"))
        {
            owSoundPlayed = false; // Reset the "ow" sound flag
            Debug.Log("Player no longer in contact with the plane.");
        }
    }
}
