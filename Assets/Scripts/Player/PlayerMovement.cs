using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 90f; // Degrees per second
    private Rigidbody rb;
    private Vector3 movementInput;
    private float rotationInput;
    public bool pausedMovement = false;

    // Footstep sounds
    [SerializeField] private AudioSource m_AudioSource;
    [SerializeField] private AudioClip[] m_FootstepSounds;

    // Collision Audio
    public AudioClip collisionSound;          // Head collision sound
    private AudioSource collisionAudioSource; // AudioSource for collision sound
    private bool collisionSoundPlayed = false;

    // Scraping sound
    public AudioClip scrapingSound;           // Scraping sound
    private AudioSource scrapingAudioSource;  // AudioSource for scraping sound
    private bool isScraping = false;

    // Head collision
    public Transform headTransform;           // Assign the head's Transform
    public float headCollisionRadius = 0.5f;  // Radius for head collision detection

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Initialize collision sound
        collisionAudioSource = gameObject.AddComponent<AudioSource>();
        collisionAudioSource.clip = collisionSound;

        // Initialize scraping sound
        scrapingAudioSource = gameObject.AddComponent<AudioSource>();
        scrapingAudioSource.clip = scrapingSound;
        scrapingAudioSource.loop = true;  // Loop the scraping sound

        // Set Rigidbody constraints to prevent unwanted rotation
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }
    void Update()
    {
        float moveZ = 0;
        float rotateY = 0;
        if (!pausedMovement)
        {
            // Get input from player
            rotateY = Input.GetAxisRaw("Horizontal");
            moveZ = Input.GetAxisRaw("Vertical");
        }

        // Store movement and rotation input for FixedUpdate
        movementInput = new Vector3(0, 0, moveZ).normalized;
        rotationInput = rotateY;

        // Check head collision
        CheckHeadCollision();
    }

    void FixedUpdate()
    {
        // Instantaneous rotation
        if (rotationInput != 0)
        {
            float rotationAmount = rotationInput * rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0, rotationAmount, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        // Immediate movement
        if (movementInput.z != 0)
        {
            Vector3 movement = rb.transform.forward * movementInput.z * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);

            // Play footstep sounds
            if (!m_AudioSource.isPlaying)
            {
                PlayFootStepAudio();
            }
        }

        // Stop player immediately when no input
        if (movementInput == Vector3.zero)
        {
            rb.velocity = Vector3.zero;
        }
    }

    public bool IsMoving()
    {
        return movementInput.z != 0;
    }

    public Vector3 GetMovementDirection()
    {
        return movementInput;
    }

    public Vector3 GetLookDirection()
    {
        // Return the player's forward direction projected onto horizontal plane
        Vector3 lookDirection = transform.forward;
        lookDirection.y = 0;
        return lookDirection.normalized;
    }

    internal void PausePlayerMovement()
    {
        pausedMovement = true;
    }

    internal void ResumePlayerMovement()
    {
        pausedMovement = false;
    }

    private void PlayFootStepAudio()
    {
        // Select and play a random footstep sound from the array, excluding index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // Move chosen sound to index 0 so it won't be selected next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }

    // Head collision
    void CheckHeadCollision()
    {
        // Use Physics.OverlapSphere to detect colliders at the head's position
        Collider[] hitColliders = Physics.OverlapSphere(headTransform.position, headCollisionRadius);
        bool isColliding = false;
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.CompareTag("Plane"))
            {
                isColliding = true;
                break;
            }
        }
        if (isColliding && !collisionSoundPlayed)
        {
            collisionAudioSource.PlayOneShot(collisionSound);
            collisionSoundPlayed = true;
        }
        else if (!isColliding)
        {
            collisionSoundPlayed = false;
        }
    }

    // Body collision for scraping sound
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plane"))
        {
            // Check if the player is moving
            if (movementInput.z != 0)
            {
                // Start scraping sound
                if (!scrapingAudioSource.isPlaying)
                {
                    scrapingAudioSource.Play();
                    isScraping = true;
                }
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plane"))
        {
            // Check if the player is moving
            if (movementInput.z != 0)
            {
                if (!scrapingAudioSource.isPlaying)
                {
                    scrapingAudioSource.Play();
                    isScraping = true;
                }
            }
            else
            {
                // Stop scraping sound if the player has stopped moving
                if (scrapingAudioSource.isPlaying)
                {
                    scrapingAudioSource.Stop();
                    isScraping = false;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plane"))
        {
            // Stop scraping sound
            if (scrapingAudioSource.isPlaying)
            {
                scrapingAudioSource.Stop();
                isScraping = false;
            }
        }
    }

    // Optional: Visualize head collision radius in the editor
    private void OnDrawGizmosSelected()
    {
        if (headTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(headTransform.position, headCollisionRadius);
        }
    }
}
