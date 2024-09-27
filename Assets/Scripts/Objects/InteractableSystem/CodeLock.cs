using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour, IInteractable {
    [SerializeField] private AudioClip[] clickSounds;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int[] correctCombination;
    private int currentStep = 0; 
    private int currentSoundIndex = 0; //For keeping track of current click-sound
    [SerializeField] private PlayerMovement playerMovement;

    public string InteractionPrompt { get; }
    
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
        audioSource.clip = clickSounds[currentSoundIndex];
        audioSource.Play();
    }

    private void ConfirmSelection() {
        //Check if the chosen sound/code-number is correct
        if (currentSoundIndex == correctCombination[currentStep]) { //If the chosen sound is the same as the correct sound...
            currentStep++; // Go to the next step in the lock-combination

            if (currentStep >= correctCombination.Length) { //Is the lock open?
                UnlockSafe();
            }
        } else { //If it was wrong
            currentStep = 0;
        }
    }

    private void UnlockSafe() {
        Debug.Log("Kassaksåpet är upplåst");
    }

    private void EndInteraction(){
        playerMovement.enabled = true; //re-enables the players movement
        Debug.Log("interaktionen har avslutats");
    }
}
