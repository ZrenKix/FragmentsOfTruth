using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    private Rigidbody rb;
    private Vector3 movement;
    public bool pausedMovement = false;

    // Fotstegsljud
    [SerializeField] private AudioSource m_AudioSource;
    [SerializeField] private AudioClip[] m_FootstepSounds;

    // Kollision Audio
    public AudioClip collisionSound;          // Tilldela ditt huvudkollisionsljud
    private AudioSource collisionAudioSource; // AudioSource f�r kollisionsljud
    private bool collisionSoundPlayed = false;

    // Skrapljud
    public AudioClip scrapingSound;           // Tilldela ditt skrapljud
    private AudioSource scrapingAudioSource;  // AudioSource f�r skrapljud
    private bool isScraping = false;

    // Huvudkollision
    public Transform headTransform;           // Tilldela huvudets Transform
    public float headCollisionRadius = 0.5f;  // Radie f�r huvudkollisionsdetektion

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Initiera kollisionsljud
        collisionAudioSource = gameObject.AddComponent<AudioSource>();
        collisionAudioSource.clip = collisionSound;

        // Initiera skrapljud
        scrapingAudioSource = gameObject.AddComponent<AudioSource>();
        scrapingAudioSource.clip = scrapingSound;
        scrapingAudioSource.loop = true;  // Loopar skrapljudet
    }

    void Update()
    {
        float moveZ = 0;
        float rotateY = 0;
        if (!pausedMovement)
        {
            // Get input from player
            rotateY = Input.GetAxis("Horizontal");
            moveZ = Input.GetAxis("Vertical");
        }

        // Apply rotation to the player
        float rotationAmount = rotateY * lookSpeed;
        transform.Rotate(0, rotationAmount, 0);

        // Calculate movement direction
        movement = transform.forward * moveZ;

        // Kontrollera huvudkollision
        CheckHeadCollision();
    }

    void FixedUpdate()
    {
        // Move the player
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));

        // Play footstep sounds
        if (movement != Vector3.zero && !m_AudioSource.isPlaying)
        {
            PlayFootStepAudio();
        }
    }

    public bool IsMoving()
    {
        return movement != Vector3.zero;
    }

    public Vector3 GetMovementDirection()
    {
        return movement;
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
        // V�lj och spela ett slumpm�ssigt fotstegsljud fr�n arrayen, exklusive index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // Flytta valt ljud till index 0 s� att det inte v�ljs n�sta g�ng
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }

    // Huvudkollision
    void CheckHeadCollision()
    {
        // Anv�nd Physics.OverlapSphere f�r att uppt�cka kolliderare vid huvudets position
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

    // Kroppskollision f�r skrapljud
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plane"))
        {
            // Check if the player is moving
            if (movement != Vector3.zero)
            {
                // Calculate the angle between movement and collision normal
                Vector3 collisionNormal = collision.contacts[0].normal;
                float angle = Vector3.Angle(movement, -collisionNormal);

                if (angle > 10f && angle < 170f)
                {
                    // Not a direct frontal collision, start scraping sound
                    if (!scrapingAudioSource.isPlaying)
                    {
                        scrapingAudioSource.Play();
                        isScraping = true;
                    }
                }
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plane"))
        {
            // Check if the player is moving
            if (movement != Vector3.zero)
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
