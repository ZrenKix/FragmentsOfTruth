//Nora Wennerberg, nowe9092

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Tutorial : MonoBehaviour {
    [SerializeField] private AudioSource audioSource; 
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private List<AudioClip> audioClips; // List with all clips that should play
    [SerializeField] private AudioClip interactClip;
    [SerializeField] private AudioMixer audioMixer;

    private int currentClipIndex = 0;

    void Start() {
        if (audioClips.Count > 0 && audioSource != null) {
            // Starts playing the clips in order
            StartCoroutine(PlayClipsSequentially());
        }
        else {
            Debug.LogWarning("AudioSource eller ljudklipp saknas!");
        }
    }

    IEnumerator PlayClipsSequentially() {
        while (currentClipIndex < audioClips.Count) {
            playerMovement.PausePlayerMovement(); //Disable the player's movement
            audioMixer.SetFloat("GameSoundsVolume", -80f); //Lowers all other sounds

            audioSource.clip = audioClips[currentClipIndex];
            audioSource.Play();

            // Waits till the clip has played to the end
            yield return new WaitForSeconds(audioClips[currentClipIndex].length);

            // Goes to next clip
            currentClipIndex++;
        }
       
        audioMixer.SetFloat("GameSoundsVolume", 0f); //Unmutes the game sounds
        playerMovement.ResumePlayerMovement(); //re-enables the players movement
    }
}
