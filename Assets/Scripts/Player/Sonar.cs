using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ObjectAudioPair
{
    public string objectName;    // The name of the object (can be auto-assigned if not provided)
    public AudioClip audioClip;  // The corresponding audio clip
}

public class Sonar : MonoBehaviour
{
    [SerializeField] private AudioClip m_audioClip;           // Default sonar audio clip
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private float m_rayMaxLength = 10f;
    [SerializeField] private float m_minPingInterval = 0.2f;
    [SerializeField] private float m_maxPingInterval = 2f;
    [SerializeField] private AnimationCurve pingIntervalCurve;

    private float m_nextPingTime = 0f;
    private Camera mainCamera;
    private AudioClip originalClip; // Store the original audio clip

    public enum State { On, Off }
    public State state;

    // List of object-audio pairs to be set in the Unity Editor
    [SerializeField] private List<ObjectAudioPair> objectAudioPairs = new List<ObjectAudioPair>();

    // Dictionary for quick lookup of audio clips based on object names
    private Dictionary<string, AudioClip> objectAudioClips = new Dictionary<string, AudioClip>();

    private void Start()
    {
        mainCamera = Camera.main;
        originalClip = m_audioClip;  // Store the initial sonar audio clip

        // Convert list to dictionary and auto-assign keys based on audio clip name if needed
        foreach (var pair in objectAudioPairs)
        {
            // If the objectName is empty, assign the audioClip's name as the key
            string objectName = string.IsNullOrEmpty(pair.objectName) && pair.audioClip != null
                ? pair.audioClip.name  // Auto-assign the audio clip's name as the key
                : pair.objectName;

            // If the objectName is still empty or if no clip exists, skip this entry
            if (string.IsNullOrEmpty(objectName) || pair.audioClip == null)
            {
                Debug.LogWarning("Missing object name or audio clip in pair.");
                continue;
            }

            // Add to dictionary (this will overwrite existing entries if the object name is already present)
            if (!objectAudioClips.ContainsKey(objectName))
            {
                objectAudioClips.Add(objectName, pair.audioClip);
            }
        }
    }

    private void Update()
    {
        // Toggle sonar state with Enter key
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (state == State.On)
            {
                state = State.Off;
                m_audioSource.clip = null;  // Mute the sonar by setting clip to null
            }
            else
            {
                state = State.On;
                m_audioSource.clip = originalClip;  // Restore the original sonar audio clip
                m_nextPingTime = Time.time;
            }
        }

        // Raycast should work all the time
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (!Physics.Raycast(ray, out RaycastHit hit, m_rayMaxLength)) return;

        // Calculate ping interval based on the distance to the object
        float normalizedDistance = Mathf.InverseLerp(m_rayMaxLength, 0, hit.distance);
        float pingInterval = Mathf.Lerp(
            m_maxPingInterval,
            m_minPingInterval,
            pingIntervalCurve.Evaluate(normalizedDistance)
        );

        // Check if the object hit by the ray exists in the dictionary
        string objectName = hit.collider.gameObject.name;
        if (objectAudioClips.ContainsKey(objectName))
        {
            // Increase the pitch if the object exists in the dictionary
            m_audioSource.pitch = 0.9f;
        }
        else
        {
            // Reset the pitch to normal if the object is not in the dictionary
            m_audioSource.pitch = 0.6f;
        }

        // Play ping sound if it's time and the sonar is active
        if (state == State.On && Time.time >= m_nextPingTime)
        {
            m_audioSource.PlayOneShot(m_audioClip);
            m_nextPingTime = Time.time + pingInterval;
        }

        Debug.DrawRay(ray.origin, ray.direction * m_rayMaxLength, Color.green);

        // When right Shift is pressed, play the audio clip associated with the object's name
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            // Use the dictionary to find the corresponding clip, or fall back to the default clip
            if (objectAudioClips.TryGetValue(objectName, out AudioClip clip))
            {
                m_audioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"No audio clip found for object: {objectName}");
            }
        }
    }
}