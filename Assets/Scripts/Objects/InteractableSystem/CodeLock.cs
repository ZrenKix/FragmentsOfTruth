using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NewBehaviourScript : MonoBehaviour, IInteractable {
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clickSounds;
    [SerializeField] private int[] correctCombination;

    //[SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip unlockSound;

    private int currentStep = 0; 
    private int currentSoundIndex = 0; //For keeping track of current click-sound
    [SerializeField] private PlayerMovement playerMovement;

    public string InteractionPrompt { get; }

    private void Start() {
        SetUpCorrectCombination(); 
    }
    
    public bool Interact(Interactor interactor) {
        playerMovement.enabled = false; //Disable the player's movement
        
        //Exlpain the controls for the player (A, D , Esc)

        ManagePlayerInput(); //Manage player input

        return true; //returns true if the interaction was successfull
    }

    private void ManagePlayerInput() {
        while (currentStep < correctCombination.Length) {
            if (Input.GetKeyDown(KeyCode.A)) { //If player is pressing A then go down 
                ChangeSound(-1);
            } else if (Input.GetKeyDown(KeyCode.D)){ //If player is pressing D then go up
                ChangeSound(1);
            }

            if (Input.GetKeyDown(KeyCode.E)){ //Press E to confirm the code-number
                ConfirmSelection();
            }

            if (Input.GetKeyDown(KeyCode.Escape)){ //If esc is pressed, end interaction 
                EndInteraction();
            }
        }
    }

    private void ChangeSound(int direction) {
        currentSoundIndex += direction;

        if (currentSoundIndex < 0) { //If the index is out of bounds, the circulate back to the top of the array
            currentSoundIndex = clickSounds.Length -1;
        }

        if (currentSoundIndex >= clickSounds.Length) {
            currentSoundIndex = 0;
        } 

        PlayNextSound();
        
    }

    private void PlayNextSound() {
        if (currentSoundIndex == correctCombination[currentStep]) { //If it was the correct number on the code
            audioSource.PlayOneShot(correctSound); // Spela det korrekta ljudet
        } else {
            audioSource.clip = clickSounds[currentSoundIndex]; // Otherwise play the sound from the array
            audioSource.Play();
        }
    }

    private void ConfirmSelection() {
        //Check if the chosen sound/code-number is correct
        if (currentSoundIndex == correctCombination[currentStep]) { //If the chosen sound is the same as the correct sound...
            currentStep++; // Go to the next step in the lock-combination
            audioSource.PlayOneShot(correctSound);

            if (currentStep >= correctCombination.Length) { //Is the lock open?
                UnlockSafe();
            }
        } else { //If it was wrong
            currentStep = 0;
            audioSource.Play();
        }
    }

    private void UnlockSafe() {
        audioSource.PlayOneShot(unlockSound);
        Debug.Log("Kassaksåpet är upplåst");
    }

    private void EndInteraction(){
        playerMovement.enabled = true; //re-enables the players movement
        Debug.Log("interaktionen har avslutats");
    }

    private void SetUpCorrectCombination() {
        correctCombination = new int[3]; // Assuming the combination has 3 steps
        for (int i = 0; i < correctCombination.Length; i++) {
            // Randomly select a position for the correct sound in the clickSounds array
            int randomIndex = UnityEngine.Random.Range(0, clickSounds.Length);
            correctCombination[i] = randomIndex; // Store it as 0-based index
        }

        // Add correctSound to the clickSounds if it's not already included
        if (!System.Array.Exists(clickSounds, clip => clip == correctSound)) {
            Array.Resize(ref clickSounds, clickSounds.Length + 1);
            clickSounds[clickSounds.Length - 1] = correctSound; // Add the correctSound to the array
        }
    }
}
