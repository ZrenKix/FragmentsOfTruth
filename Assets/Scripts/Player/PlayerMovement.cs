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

    // Audio
    [SerializeField] private AudioSource m_AudioSource;
    [SerializeField] private AudioClip[] m_FootstepSounds;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float moveX = 0;
        float moveZ = 0;
        if (!pausedMovement)
        {
            // Get input from the player
            moveX = Input.GetAxis("Horizontal");
            moveZ = Input.GetAxis("Vertical");
        }

        // Update yaw based on mouse input
        float mouseX = Input.GetAxis("Mouse X");
        yaw += mouseX * lookSpeed;

        // Apply the camera rotation to the player
        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(0, yaw, 0);
        }

        // Calculate camera forward and right directions
        Vector3 cameraForward = playerCamera.forward;
        Vector3 cameraRight = playerCamera.right;
        cameraForward.y = 0; // Ensure the direction is horizontal
        cameraRight.y = 0;   // Ensure the direction is horizontal
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate movement direction relative to the camera
        movement = (cameraForward * moveZ + cameraRight * moveX).normalized;
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
        // Return the camera's forward direction projected onto the horizontal plane
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
        // Pick & play a random footstep sound from the array, excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // Move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }
}
