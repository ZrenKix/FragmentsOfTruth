using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    public Transform playerCamera;
    private Rigidbody rb;
    private Vector3 movement;
    private float yaw = 0f;
    public bool pausedMovement = false;

    // Fotstegsljud
    [SerializeField] private AudioSource m_AudioSource;
    [SerializeField] private AudioClip[] m_FootstepSounds;

    // Kollision Audio
    public AudioClip collisionSound;          // Tilldela ditt huvudkollisionsljud
    private AudioSource collisionAudioSource; // AudioSource för kollisionsljud
    private bool collisionSoundPlayed = false;

    // Skrapljud
    public AudioClip scrapingSound;           // Tilldela ditt skrapljud
    private AudioSource scrapingAudioSource;  // AudioSource för skrapljud
    private bool isScraping = false;

    // Huvudkollision
    public Transform headTransform;           // Tilldela huvudets Transform
    public float headCollisionRadius = 0.5f;  // Radie för huvudkollisionsdetektion

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
        float moveX = 0;
        float moveZ = 0;
        if (!pausedMovement)
        {
            // Ta emot input från spelaren
            moveX = Input.GetAxis("Horizontal");
            moveZ = Input.GetAxis("Vertical");
        }

        // Uppdatera yaw baserat på musinput
        float mouseX = Input.GetAxis("Mouse X");
        yaw += mouseX * lookSpeed;

        // Applicera kamerarotation på spelaren
        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(0, yaw, 0);
        }

        // Beräkna kamerans framåt- och högervektorer
        Vector3 cameraForward = playerCamera.forward;
        Vector3 cameraRight = playerCamera.right;
        cameraForward.y = 0; // Säkerställ att riktningen är horisontell
        cameraRight.y = 0;   // Säkerställ att riktningen är horisontell
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Beräkna rörelseriktning relativt kameran
        movement = (cameraForward * moveZ + cameraRight * moveX).normalized;

        // Kontrollera huvudkollision
        CheckHeadCollision();
    }

    void FixedUpdate()
    {
        // Flytta spelaren
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));

        // Spela fotstegsljud
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
        // Returnera kamerans framåtriktning projicerad på horisontalplanet
        Vector3 lookDirection = playerCamera.forward;
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
        // Välj och spela ett slumpmässigt fotstegsljud från arrayen, exklusive index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // Flytta valt ljud till index 0 så att det inte väljs nästa gång
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }

    // Huvudkollision
    void CheckHeadCollision()
    {
        // Använd Physics.OverlapSphere för att upptäcka kolliderare vid huvudets position
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

    // Kroppskollision för skrapljud
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plane"))
        {
            // Kontrollera om spelaren rör på sig
            if (movement != Vector3.zero)
            {
                // Beräkna vinkeln mellan rörelsen och kollisionsnormalen
                Vector3 collisionNormal = collision.contacts[0].normal;
                float angle = Vector3.Angle(movement, -collisionNormal);

                if (angle > 10f && angle < 170f)
                {
                    // Inte en direkt frontalkollision, starta skrapljud
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
            // Kontrollera om spelaren rör på sig
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
                // Stoppa skrapljudet om spelaren har slutat röra sig
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
            // Stoppa skrapljudet
            if (scrapingAudioSource.isPlaying)
            {
                scrapingAudioSource.Stop();
                isScraping = false;
            }
        }
    }

    // Valfritt: Visualisera huvudkollisionsradien i editorn
    private void OnDrawGizmosSelected()
    {
        if (headTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(headTransform.position, headCollisionRadius);
        }
    }
}
