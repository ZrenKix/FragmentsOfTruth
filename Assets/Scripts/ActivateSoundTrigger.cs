using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateSoundTrigger : MonoBehaviour
{
    [SerializeField] AudioSource source;
    [SerializeField] private AudioClip clip;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Range(0.5f, 3f)]
    [SerializeField] private float pitch = 1f;

    [SerializeField] private float fadeDuration = 1f;

    [Range(0.5f, 3f)]
    [SerializeField] private float minRandomPitch = 0.5f;

    [Range(0.5f, 3f)]
    [SerializeField] private float maxRandomPitch = 3f;

    private bool hasPlayed = false;
    private bool insideTrigger = false;

    // Static variable to track if a trigger sound is playing
    private static bool soundPlaying = false;

    void Start()
    {
        source.volume = volume;
        source.pitch = pitch;
    }

    private void Update()
    {
        if (source.isPlaying && Input.GetKey(KeyCode.Escape))
        {
            source.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasPlayed && !soundPlaying)
        {
            source.clip = clip;
            source.pitch = RandomizePitch();
            StartCoroutine(FadeInAndPlay());
            hasPlayed = true;
        }
    }

    private IEnumerator FadeInAndPlay()
    {
        soundPlaying = true; // Set the flag to prevent other triggers from activating

        source.volume = 0f;
        source.Play();

        float currentTime = 0f;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, volume, currentTime / fadeDuration);
            yield return null;
        }

        yield return new WaitWhile(() => source.isPlaying);

        soundPlaying = false; // Reset the flag after sound finishes playing
        GetComponent<Collider>().enabled = false;
    }

    private float RandomizePitch()
    {
        return Random.Range(minRandomPitch, maxRandomPitch);
    }
}
