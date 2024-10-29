using UnityEngine;

public class AudioFilterController : MonoBehaviour
{
    public Transform playerTransform; // The player's (or camera's) transform
    public float cutoffFrequencyBehind = 500f; // Cutoff frequency when the sound source is behind the player
    public float normalCutoffFrequency = 22000f; // Default cutoff frequency when in front of the player

    private AudioSource[] allAudioSources;
    private AudioLowPassFilter[] allLowPassFilters;

    void Start()
    {
        // Find all AudioSource components in the scene
        allAudioSources = FindObjectsOfType<AudioSource>();

        // Add an AudioLowPassFilter to each AudioSource if it doesn't already have one
        allLowPassFilters = new AudioLowPassFilter[allAudioSources.Length];
        for (int i = 0; i < allAudioSources.Length; i++)
        {
            AudioSource audioSource = allAudioSources[i];
            AudioLowPassFilter lowPassFilter = audioSource.GetComponent<AudioLowPassFilter>();

            if (lowPassFilter == null)
            {
                lowPassFilter = audioSource.gameObject.AddComponent<AudioLowPassFilter>();
                //Debug.Log($"Added AudioLowPassFilter to AudioSource: {audioSource.gameObject.name}");
            }

            allLowPassFilters[i] = lowPassFilter;
        }
    }

    void Update()
    {
        // Iterate through all AudioSources in the scene
        for (int i = 0; i < allAudioSources.Length; i++)
        {
            AudioSource audioSource = allAudioSources[i];
            AudioLowPassFilter lowPassFilter = allLowPassFilters[i];

            if (audioSource == null) return;

            // Calculate the vector from the player to the sound source
            Vector3 directionToSource = audioSource.transform.position - playerTransform.position;

            // Get the forward direction of the player/camera
            Vector3 forwardDirection = playerTransform.forward;

            // Check if the sound source is behind the player
            if (Vector3.Dot(forwardDirection, directionToSource) < 0)
            {
                // Sound source is behind the player, apply low-pass filter
                lowPassFilter.cutoffFrequency = cutoffFrequencyBehind;
                //Debug.Log($"AudioSource '{audioSource.gameObject.name}' is behind the player. Low-pass filter applied.");
            }
            else
            {
                // Sound source is in front of the player, set normal cutoff frequency
                lowPassFilter.cutoffFrequency = normalCutoffFrequency;
                //Debug.Log($"AudioSource '{audioSource.gameObject.name}' is in front of the player. Normal audio applied.");
            }
        }
    }
}
