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
    public AudioClip preIntroClip;  // New pre-intro sound clip
    public AudioClip introClip;
    public AudioClip endGameClip;  // Endgame sound clip
    public GameObject uiPanel;
    public AudioMixer audioMixer;

    private int currentButtonIndex = 0;
    private bool isPaused = false;
    private bool isTriggered = false;

    private PlayerMovement playerMovement;

    void Start()
    {
        uiPanel.SetActive(false);
        playerMovement = FindObjectOfType<PlayerMovement>();
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

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
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
            PlayPreIntroAndIntro();
        }
    }

    private void PlayPreIntroAndIntro()
    {
        if (audioSource != null)
        {
            if (preIntroClip != null)
            {
                audioSource.clip = preIntroClip;
                audioSource.Play();
                Invoke(nameof(PlayIntroClip), preIntroClip.length);  // Play intro after pre-intro finishes
            }
            else
            {
                PlayIntroClip();  // Play intro immediately if no preIntroClip is set
            }
        }
    }

    private void PlayIntroClip()
    {
        if (introClip != null)
        {
            audioSource.clip = introClip;
            audioSource.Play();
            Invoke(nameof(PauseGame), introClip.length);
        }
        else
        {
            PauseGame();  // Pause immediately if no introClip is set
        }
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
        }
        else
        {
            PlayAudioClip(confirmClip2);
        }

        ResumeGame();

        if (currentButtonIndex == 1)
        {
            Invoke(nameof(PlayEndGameAndExit), confirmClip2.length + 0.2f);
        }
        if (currentButtonIndex == 0)
        {
            Invoke(nameof(PlayEndGameAndExit), confirmClip1.length + 0.2f);
        }
    }

    private void PlayEndGameAndExit()
    {
        if (audioSource != null && endGameClip != null)
        {
            audioSource.clip = endGameClip;
            audioSource.Play();

            Time.timeScale = 1f;

            Invoke(nameof(ExitGame), endGameClip.length);
        }
        else
        {
            ExitGame();
        }
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PlayAudioClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
            playerMovement.PausePlayerMovement();
        }
    }
}
