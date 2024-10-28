using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LogManager : MonoBehaviour
{
    public static LogManager Instance { get; private set; }

    private string logFilePath;
    private StreamWriter logWriter;
    private GameObject player;

    private Dictionary<string, int> customVariables = new Dictionary<string, int>();

    private void Awake()
    {
        // Implement the Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
            InitializeLogFile();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Try to find the player object with tag "Player"
        FindPlayer();
        // Write the setup configuration if the player is found
        if (player != null)
        {
            WriteSetupConfiguration();
        }
        else
        {
            Debug.LogWarning("Player object with tag 'Player' not found. Will attempt to find it later.");
        }
    }

    private void Update()
    {
        // If the player hasn't been found yet, keep trying
        if (player == null)
        {
            FindPlayer();
            if (player != null)
            {
                WriteSetupConfiguration();
            }
        }
    }

    private void OnDestroy()
    {
        // Close the log file when the instance is destroyed
        if (Instance == this)
        {
            CloseLogFile();
            Instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        // Ensure the log file is closed when the application quits
        CloseLogFile();
    }

    /// <summary>
    /// Initializes the log file and opens a StreamWriter.
    /// </summary>
    private void InitializeLogFile()
    {
        string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        // Define the "Game Logs" directory path
        string logsDirectory = Path.Combine(Application.dataPath, "../Game Logs");

        // Ensure the directory exists
        Directory.CreateDirectory(logsDirectory);

        // Define the full log file path
        logFilePath = Path.Combine(logsDirectory, $"log_{dateTime}.txt");

        try
        {
            logWriter = new StreamWriter(logFilePath, true);
            Debug.Log($"Log file initialized at path: {logFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize log file: {e.Message}");
        }
    }

    /// <summary>
    /// Closes the StreamWriter and releases resources.
    /// </summary>
    private void CloseLogFile()
    {
        if (logWriter != null)
        {
            try
            {
                LogSummary();
                logWriter.Flush();
                logWriter.Close();
                logWriter = null;
                Debug.Log("Log file closed.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to close log file: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Finds the player object with tag "Player" and assigns it to the player variable.
    /// </summary>
    private void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log("Player object found.");
        }
    }

    /// <summary>
    /// Writes the initial setup configuration to the log file.
    /// </summary>
    public void WriteSetupConfiguration()
    {
        if (logWriter == null)
        {
            Debug.LogError("LogWriter is not initialized.");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player object is not found. Cannot write setup configuration.");
            return;
        }

        try
        {
            logWriter.WriteLine("SETUP CONFIGURATION:");

            // Write player-specific setup values
            logWriter.WriteLine("Player:");
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                logWriter.WriteLine($"Name: {player.name}");
                logWriter.WriteLine($"Move Speed: {playerMovement.moveSpeed}");
                logWriter.WriteLine($"Rotation Speed: {playerMovement.rotationSpeed}");
            }

            // Write sonar-specific setup values
            Sonar sonar = player.GetComponent<Sonar>();
            if (sonar != null)
            {
                logWriter.WriteLine("\nSonar:");
                logWriter.WriteLine($"Max Ray Length: {sonar.m_rayMaxLength}");
                logWriter.WriteLine($"Max Ping Interval: {sonar.m_maxPingInterval}");
                logWriter.WriteLine($"Min Ping Interval: {sonar.m_minPingInterval}");
            }

            // Write interactor-specific setup values
            Interactor interactor = player.GetComponent<Interactor>();
            if (interactor != null)
            {
                logWriter.WriteLine("\nInteractor:");
                logWriter.WriteLine($"Interaction Point: {interactor._interactionPoint}");
                logWriter.WriteLine($"Interaction Point Radius: {interactor._interactionPointRadius}");
            }

            // Add a separator for game events
            logWriter.WriteLine("\n--- GAME EVENTS ---");
            logWriter.Flush();

            Debug.Log("Log file initialized and setup written.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write setup configuration: {e.Message}");
        }
    }

    /// <summary>
    /// Logs a game event with a timestamp.
    /// </summary>
    /// <param name="eventName">The name of the event to log.</param>
    public void LogEvent(string eventName)
    {
        if (logWriter == null)
        {
            Debug.LogError("LogWriter is not initialized.");
            return;
        }

        try
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            string logEntry = $"time: {time}, event: {eventName}";
            logWriter.WriteLine(logEntry);
            logWriter.Flush();

            Debug.Log($"Logged event: {logEntry}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to log event: {e.Message}");
        }
    }

    // New method to store a custom variable
    public void StoreValue(string variableName, int changeAmount)
    {
        if (customVariables.ContainsKey(variableName))
        {
            customVariables[variableName] += changeAmount; // Update the existing value with the change
        }
        else
        {
            customVariables[variableName] = changeAmount; // Add new variable with initial value
        }

        Debug.Log($"Stored value: {variableName} = {customVariables[variableName]}");
    }

    // New method to log summary of stored variables
    private void LogSummary()
    {
        if (logWriter == null) return;

        try
        {
            logWriter.WriteLine("\n--- SUMMARY ---");
            foreach (var kvp in customVariables)
            {
                logWriter.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
            logWriter.Flush();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to log summary: {e.Message}");
        }
    }
}
