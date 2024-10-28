using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ButtonToggle : MonoBehaviour
{
    public Button button1;
    public Button button2;
    public AudioSource audioSource;
    public AudioClip clip1;
    public AudioClip clip2;
    public AudioClip confirmClip1;
    public AudioClip confirmClip2;
    public AudioClip introClip;
    public AudioClip endGameClip;  // New endgame sound clip
    public GameObject uiPanel;
    public AudioMixer audioMixer;

    private int currentButtonIndex = 0;
    private bool isPaused = false;
    private bool isTriggered = false;

    void Start()
    {
        uiPanel.SetActive(false);
    }

    void Update()
    {
        if (isPaused)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ToggleButton(-1);
            }

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                ToggleButton(1);
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                ConfirmSelection();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            PlayIntroAndPauseGame();
        }
    }

    private void PlayIntroAndPauseGame()
    {
        if (audioSource != null && introClip != null)
        {
            audioSource.clip = introClip;
            audioSource.Play();
        }

        Invoke(nameof(PauseGame), introClip.length);
    }

    private void PauseGame()
    {
        isPaused = true;
        uiPanel.SetActive(true);
        Time.timeScale = 0f;

        audioMixer.SetFloat("GameSoundsVolume", -80f);

        HighlightButton(currentButtonIndex);
    }

    public void ResumeGame()
    {
        isPaused = false;
        uiPanel.SetActive(false);
        Time.timeScale = 1f;

        audioMixer.SetFloat("GameSoundsVolume", 0f);
    }

    private void ToggleButton(int direction)
    {
        currentButtonIndex += direction;

        if (currentButtonIndex < 0) currentButtonIndex = 1;
        else if (currentButtonIndex > 1) currentButtonIndex = 0;

        HighlightButton(currentButtonIndex);
    }

    private void HighlightButton(int buttonIndex)
    {
        if (buttonIndex == 0)
        {
            button1.Select();
            PlayAudioClip(clip1);
        }
        else
        {
            button2.Select();
            PlayAudioClip(clip2);
        }
    }

    private void ConfirmSelection()
    {
        if (currentButtonIndex == 0)
        {
            PlayAudioClip(confirmClip1);
            ResumeGame();
        }
        else
        {
            PlayAudioClip(confirmClip2);
            PlayEndGameAndExit();
        }
    }

    private void PlayEndGameAndExit()
    {
        if (audioSource != null && endGameClip != null)
        {
            audioSource.clip = endGameClip;
            audioSource.Play();
            Invoke(nameof(ExitGame), endGameClip.length + 0.2f);  // Wait for endgame clip to finish
        }
        else
        {
            ExitGame();  // Exit immediately if no endgame clip is set
        }
    }

    private void ExitGame()
    {

            Application.Quit();  // Quit the game in a built version
    }

    private void PlayAudioClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
