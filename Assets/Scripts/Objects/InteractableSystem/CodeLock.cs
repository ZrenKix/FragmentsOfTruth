//Nora Wennerberg, nowe9092

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CodeLock : MonoBehaviour, IInteractable {
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clickSounds;
    [SerializeField] private int[] correctCombination;
    [SerializeField] private GameObject safeMemory;

    [SerializeField] private AudioClip instructions;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip rightSound;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip openVaultSound;

    private int currentStep = 0; 
    private int currentSoundIndex = 0; //For keeping track of current click-sound

    private bool isInteracting = false;
    private bool hasPlayedInstruction = false;
    
    public string InteractionPrompt { get; }

    private void Start() {
        SetUpCorrectCombination(); 
    }

    private void Update() {
        if (isInteracting) {
            ManagePlayerInput();
        }
    }
    
    public bool Interact(Interactor interactor) {
        playerMovement.PausePlayerMovement(); //Disable the player's movement
    
        //Exlpain the controls for the player (A, D , Esc)
        if (!hasPlayedInstruction){
            ExplainControls();
        }

        isInteracting = true;

        return true; //returns true if the interaction was successful
    }

    private void ExplainControls(){
        audioSource.PlayOneShot(instructions);
        hasPlayedInstruction = true;
    }

    private void ManagePlayerInput() {
        if (currentStep < correctCombination.Length) {
            if (Input.GetKeyDown(KeyCode.A)) { //If player is pressing A then go down 
                ChangeSound(-1);
            } else if (Input.GetKeyDown(KeyCode.D)){ //If player is pressing D then go up
                ChangeSound(1);
            }

            if (Input.GetKeyDown(KeyCode.E)){ //Press E to confirm the code-number
                ConfirmSelection();
                Debug.Log("Safe, E");
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
        Debug.Log("Playing sound at index: " + currentSoundIndex);

        if (currentSoundIndex == correctCombination[currentStep]) { //If it was the correct number on the code
            audioSource.PlayOneShot(correctSound); // Spela det korrekta ljudet
            Debug.Log("Playing correct sound: " + clickSounds[currentSoundIndex].name);
        } else {
           //audioSource.clip = clickSounds[currentSoundIndex]; // Otherwise play the sound from the array
            audioSource.PlayOneShot(clickSounds[currentSoundIndex]);
        }
    }

    private void ConfirmSelection() {
        //Check if the chosen sound/code-number is correct
        if (currentSoundIndex == correctCombination[currentStep]) { //If the chosen sound is the same as the correct sound...
            currentStep++; // Go to the next step in the lock-combination
            audioSource.PlayOneShot(rightSound);
            LogManager.Instance.LogEvent($"{gameObject.name} correct combination on safe");

            if (currentStep >= correctCombination.Length) { //Is the lock open?
                UnlockSafe();
            }
        } else { //If it was wrong
            currentStep = 0;
            audioSource.PlayOneShot(wrongSound);
            LogManager.Instance.LogEvent($"{gameObject.name} wrong combination on safe");
        }
    }

    private void UnlockSafe() {
        audioSource.PlayOneShot(unlockSound);
        audioSource.PlayOneShot(openVaultSound);
        EndInteraction();
        safeMemory.layer = 6; //Set the memorys layer from default to interactable
        gameObject.layer = 0; //Enables the interaction
        Debug.Log("Kassaksåpet är upplåst");
    }

    private void EndInteraction(){
        isInteracting = false;
        playerMovement.ResumePlayerMovement(); //re-enables the players movement
        Debug.Log("interaktionen har avslutats");
    }

    private void SetUpCorrectCombination() {
        correctCombination = new int[3]; // Assuming the combination has 3 steps

        // Update the combination with valid indices
        for (int i = 0; i < correctCombination.Length; i++) {
            // Randomly select a position for the correct sound in the clickSounds array
            int randomIndex = UnityEngine.Random.Range(0, clickSounds.Length);
            correctCombination[i] = randomIndex; // Store it as 0-based index
        }
    }
}
