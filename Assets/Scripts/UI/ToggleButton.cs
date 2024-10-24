using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ButtonToggle : MonoBehaviour
{
    public Button button1;  // Assign Button1 in Inspector
    public Button button2;  // Assign Button2 in Inspector
    public AudioSource audioSource;  // Assign an AudioSource in Inspector
    public AudioClip clip1;  // Assign AudioClip for Button1 in Inspector
    public AudioClip clip2;  // Assign AudioClip for Button2 in Inspector
    public AudioClip confirmClip1;  // Audio played when Button1 is selected
    public AudioClip confirmClip2;  // Audio played when Button2 is selected
    public GameObject uiPanel;  // Assign the UI panel containing the buttons
    public AudioMixer audioMixer;  // Assign the AudioMixer in Inspector

    private int currentButtonIndex = 0;  // 0 for Button1, 1 for Button2
    private bool isPaused = false;  // Tracks whether the game is paused

    void Start()
    {
        // Initially hide the UI and keep the game running
        uiPanel.SetActive(false);
    }

    void Update()
    {
        // If the UI is active, allow toggling between buttons
        if (isPaused)
        {
            // Check for 'A' key or Left Arrow key input
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ToggleButton(-1);
            }

            // Check for 'D' key or Right Arrow key input
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                ToggleButton(1);
            }

            // If the player presses Enter or Space to select the current button
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                ConfirmSelection();
            }
        }
    }

    // Detect when the player enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Make sure the collider is the player
        {
            PauseGame();  // Trigger the UI and pause the game
        }
    }

    // Method to pause the game, mute game sounds, and show the UI
    private void PauseGame()
    {
        isPaused = true;
        uiPanel.SetActive(true);  // Show the UI
        Time.timeScale = 0f;  // Freeze the game

        // Mute all other game sounds by setting the volume to -80 dB
        audioMixer.SetFloat("GameSoundsVolume", -80f);

        HighlightButton(currentButtonIndex);  // Highlight the current button and play audio
    }

    // Method to resume the game, unmute game sounds, and hide the UI
    public void ResumeGame()
    {
        isPaused = false;
        uiPanel.SetActive(false);  // Hide the UI
        Time.timeScale = 1f;  // Resume the game

        // Unmute all other game sounds by setting the volume to 0 dB (normal volume)
        audioMixer.SetFloat("GameSoundsVolume", 0f);
    }

    // Method to toggle between buttons
    private void ToggleButton(int direction)
    {
        // Change the button index
        currentButtonIndex += direction;

        // Wrap around if index goes out of range
        if (currentButtonIndex < 0) currentButtonIndex = 1;
        else if (currentButtonIndex > 1) currentButtonIndex = 0;

        // Highlight the new button and play its corresponding audio
        HighlightButton(currentButtonIndex);
    }

    // Method to highlight the selected button and play its audio
    private void HighlightButton(int buttonIndex)
    {
        if (buttonIndex == 0)
        {
            button1.Select();  // Highlight Button1
            PlayAudioClip(clip1);  // Play Button1's sound
        }
        else
        {
            button2.Select();  // Highlight Button2
            PlayAudioClip(clip2);  // Play Button2's sound
        }
    }

    // Method to confirm the player's button selection
    private void ConfirmSelection()
    {
        // Play the corresponding confirm sound based on the selected button
        if (currentButtonIndex == 0)
        {
            PlayAudioClip(confirmClip1);  // Play Button1's confirm sound
        }
        else
        {
            PlayAudioClip(confirmClip2);  // Play Button2's confirm sound
        }

        // Resume the game after selection
        ResumeGame();
    }

    // Method to play the assigned audio clip
    private void PlayAudioClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
