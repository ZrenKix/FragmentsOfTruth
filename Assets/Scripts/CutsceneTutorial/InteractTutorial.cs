//Nora Wennerberg, nowe9092

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class InteractTutorial : MonoBehaviour {
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip interactClip;
    [SerializeField] private AudioMixer audioMixer;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag.Equals("Player")) {
            StartCoroutine(HandleInteraction());
        }
    }

    private IEnumerator HandleInteraction() {
        audioMixer.SetFloat("GameSoundsVolume", -80f); // Lowers all other sounds
        audioSource.PlayOneShot(interactClip);

        // Wait till the clip has finished playing
        yield return new WaitForSeconds(interactClip.length);

        audioMixer.SetFloat("GameSoundsVolume", 0f); // Unmutes the game sounds
        Destroy(gameObject);
    }
}
