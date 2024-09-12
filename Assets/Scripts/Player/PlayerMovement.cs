using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // 
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    public Transform playerCamera;
    private Rigidbody rb;
    private Vector3 movement;
    private float yaw = 0f;

    // Audio
    [SerializeField] private AudioSource m_AudioSource;
    [SerializeField] private AudioClip[] m_FootstepSounds;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Get input from the player
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Update yaw based on mouse input
        float mouseX = Input.GetAxis("Mouse X");
        yaw += mouseX * lookSpeed;

        // Calculate camera forward and right directions
        Vector3 cameraForward = playerCamera.forward;
        Vector3 cameraRight = playerCamera.right;
        cameraForward.y = 0; // Ensure the direction is horizontal
        cameraRight.y = 0;   // Ensure the direction is horizontal
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate movement direction relative to the camera
        movement = (cameraForward * moveZ + cameraRight * moveX).normalized;

        // Apply the camera rotation to the player
        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(0, yaw, 0);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        if (movement != Vector3.zero && !m_AudioSource.isPlaying)
        {
            PlayFootStepAudio();
        }
    }

    private void PlayFootStepAudio()
    {
        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }

}
