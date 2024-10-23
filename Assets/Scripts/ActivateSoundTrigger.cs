using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateSoundTrigger : MonoBehaviour
{
    [SerializeField] AudioSource source;
    [SerializeField] private AudioClip clip;

    // Slider for volume (0 to 1) and pitch (0.5 to 3) in the Inspector
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Range(0.5f, 3f)]
    [SerializeField] private float pitch = 1f;

    // Fade in/out duration controlled in the Inspector
    [SerializeField] private float fadeDuration = 1f;

    // Random pitch range controls in the Inspector
    [Range(0.5f, 3f)]
    [SerializeField] private float minRandomPitch = 0.5f;

    [Range(0.5f, 3f)]
    [SerializeField] private float maxRandomPitch = 3f;

    private bool hasPlayed = false;
    private bool insideTrigger = false;// To ensure the sound is played only once

    // Start is called before the first frame update
    void Start()
    {
        source.volume = volume;
        source.pitch = pitch;
    }

    private void Update()
    {
        if (source.isPlaying && Input.GetKey(KeyCode.E))
        {
            source.Stop();
        }

    }

    // Trigger detection when player enters the box
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasPlayed)
        {
           
            source.clip = clip;
            source.pitch = RandomizePitch(); // Randomize pitch
            StartCoroutine(FadeInAndPlay()); // Start fade-in and let the sound play fully
            hasPlayed = true; // Prevent the sound from being triggered again
        }
      
    }

    // Fade In Method and play until the end
    private IEnumerator FadeInAndPlay()
    {
        source.volume = 0f;
        source.Play();

        float currentTime = 0f;

        // Fade in effect
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, volume, currentTime / fadeDuration); // Smooth transition to the target volume
            yield return null;
        }

        // Wait until the clip finishes playing
        yield return new WaitWhile(() => source.isPlaying);

        // Deactivate the trigger after the sound is done playing
        GetComponent<Collider>().enabled = false;
    }

    // Randomize Pitch Method
    private float RandomizePitch()
    {
        return Random.Range(minRandomPitch, maxRandomPitch); // Random pitch between defined min and max
    }
}
