using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class CollisionSoundPlayer : MonoBehaviour
{
    public AudioClip owSound;                  // Assign your 'ow' sound clip in the inspector
    public AudioClip scrapeSound;              // Assign your scrape sound clip in the inspector
    public float scrapeSoundInterval = 0.5f;   // Time interval between scrape sounds
    public float collisionVelocityThreshold = 1f; // Minimum velocity for 'ow' sound
    public float scrapeVelocityThreshold = 0.1f;  // Minimum velocity for scrape sound
    public float angleThreshold = 45f;         // Angle threshold to differentiate walls from ground (in degrees)

    private AudioSource audioSource;
    private float lastScrapeTime;

    private PlayerMovement playerMovement; // Reference to the PlayerMovement script

    // Variables to store collision data for visualization
    private Vector3 collisionPoint;
    private Vector3 collisionNormal;
    private bool hasCollision;

    // Reference to the main camera (optional)
    public bool useCameraRotation = false;    // Toggle to use the main camera's rotation
    private Transform referenceTransform;     // The transform used for rotation reference

    void Start()
    {
        // Get or add an AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        lastScrapeTime = -scrapeSoundInterval;

        // Get the PlayerMovement component
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement script not found on the player object.");
        }

        // Set the reference transform
        if (useCameraRotation)
        {
            if (Camera.main != null)
            {
                referenceTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogError("Main Camera not found. Please tag your camera as 'MainCamera'.");
                referenceTransform = transform;
            }
        }
        else
        {
            referenceTransform = transform;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Ignore collisions with the ground
        if (IsWallCollision(collision))
        {
            // Check collision intensity
            if (collision.relativeVelocity.magnitude >= collisionVelocityThreshold)
            {
                // Play 'ow' sound
                if (owSound != null)
                {
                    audioSource.PlayOneShot(owSound);
                }
            }
        }

        // Store collision data for visualization
        if (collision.contacts.Length > 0)
        {
            collisionPoint = collision.contacts[0].point;
            collisionNormal = collision.contacts[0].normal;
            hasCollision = true;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Only play scrape sound when the player is moving
        if (playerMovement != null && playerMovement.IsMoving())
        {
            // Ignore collisions with the ground
            if (IsWallCollision(collision))
            {
                // Check collision intensity
                if (collision.relativeVelocity.magnitude >= scrapeVelocityThreshold)
                {
                    // Play scrape sound at intervals
                    if (Time.time - lastScrapeTime >= scrapeSoundInterval)
                    {
                        if (scrapeSound != null)
                        {
                            audioSource.PlayOneShot(scrapeSound);
                            lastScrapeTime = Time.time;
                        }
                    }
                }
            }
        }

        // Store collision data for visualization
        if (collision.contacts.Length > 0)
        {
            collisionPoint = collision.contacts[0].point;
            collisionNormal = collision.contacts[0].normal;
            hasCollision = true;
        }
    }

    void Update()
    {
        // For editor mode visualization
        if (!Application.isPlaying)
        {
            SimulateCollisionForEditor();
        }
        else
        {
            // Reset collision data if no collision this frame
            hasCollision = false;
        }
    }

    void OnDrawGizmos()
    {
        if (referenceTransform == null)
        {
            referenceTransform = transform;
        }

        // Draw the angle threshold arc for visualization
        Gizmos.color = Color.yellow;

#if UNITY_EDITOR
        Handles.color = new Color(1f, 1f, 0f, 0.2f);

        // Define the rotation based on the reference transform
        Quaternion rotation = Quaternion.Euler(0, referenceTransform.eulerAngles.y, 0);

        // Draw a circular arc in the player's local horizontal plane
        Vector3 forward = rotation * Vector3.forward;
        Handles.DrawSolidArc(transform.position, Vector3.up, Quaternion.Euler(0, -angleThreshold, 0) * forward, angleThreshold * 2, 1f);
#endif

        if (hasCollision)
        {
            // Draw the collision normal in red
            Gizmos.color = Color.red;
            Gizmos.DrawLine(collisionPoint, collisionPoint + collisionNormal);

            // Transform collision normal into the reference frame
            Vector3 localCollisionNormal = Quaternion.Inverse(referenceTransform.rotation) * collisionNormal;

            // Project the collision normal onto the horizontal plane in the reference frame
            Vector3 collisionNormalHorizontal = Vector3.ProjectOnPlane(localCollisionNormal, Vector3.up);

            // Transform back to world space
            collisionNormalHorizontal = referenceTransform.rotation * collisionNormalHorizontal;

            // Draw the projected collision normal onto the horizontal plane in blue
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(collisionPoint, collisionPoint + collisionNormalHorizontal);

            // Draw the angle between the vectors
            float angle = Vector3.Angle(collisionNormal, collisionNormalHorizontal);

#if UNITY_EDITOR
            // Draw the angle text
            Handles.Label(collisionPoint + Vector3.up * 0.5f, "Angle: " + angle.ToString("F1") + "°");
#endif
        }
    }

    bool IsWallCollision(Collision collision)
    {
        // Calculate the average collision normal
        Vector3 averageNormal = Vector3.zero;
        foreach (ContactPoint contact in collision.contacts)
        {
            averageNormal += contact.normal;
        }
        averageNormal /= collision.contactCount;

        // Transform collision normal into the reference frame
        Vector3 localCollisionNormal = Quaternion.Inverse(referenceTransform.rotation) * averageNormal;

        // Project the collision normal onto the horizontal plane in the reference frame
        Vector3 collisionNormalHorizontal = Vector3.ProjectOnPlane(localCollisionNormal, Vector3.up);

        // Calculate the angle between the collision normal and its projection
        float angle = Vector3.Angle(localCollisionNormal, collisionNormalHorizontal);

        // Check if the collision is with a wall (angle less than threshold)
        return angle <= angleThreshold;
    }

    // Simulate collision in editor mode
    void SimulateCollisionForEditor()
    {
        if (referenceTransform == null)
        {
            referenceTransform = transform;
        }

        RaycastHit hit;
        // Cast a ray forward to simulate wall collision
        if (Physics.Raycast(transform.position, referenceTransform.forward, out hit, 1f))
        {
            collisionPoint = hit.point;
            collisionNormal = hit.normal;
            hasCollision = true;
        }
        else
        {
            hasCollision = false;
        }
    }
}
