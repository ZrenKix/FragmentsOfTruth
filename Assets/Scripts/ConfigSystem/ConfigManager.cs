using UnityEngine;
using System.IO;

public class ConfigManager
{
    private static ConfigManager _instance;
    public static ConfigManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new ConfigManager();
            return _instance;
        }
    }

    public GameConfig Config { get; private set; }

    private string configFilePath;

    private ConfigManager()
    {
        configFilePath = Path.Combine(Application.persistentDataPath, "config.json");
        LoadConfig();
    }

    public void LoadConfig()
    {
        if (File.Exists(configFilePath))
        {
            try
            {
                string json = File.ReadAllText(configFilePath);
                Config = JsonUtility.FromJson<GameConfig>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load config: " + e.Message);
                LoadDefaultConfig();
            }
        }
        else
        {
            LoadDefaultConfig();
            SaveConfig(); // Save the default config to file
        }
    }

    public void SaveConfig()
    {
        try
        {
            string json = JsonUtility.ToJson(Config, true);
            File.WriteAllText(configFilePath, json);
            Debug.Log("Config file saved at: " + configFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save config: " + e.Message);
        }
    }

    private void LoadDefaultConfig()
    {
        // Load default config from Resources
        TextAsset defaultConfigAsset = Resources.Load<TextAsset>("defaultConfig");
        if (defaultConfigAsset != null)
        {
            string json = defaultConfigAsset.text;
            Config = JsonUtility.FromJson<GameConfig>(json);
        }
        else
        {
            Debug.LogError("Default config file not found in Resources. Using hardcoded defaults.");
            Config = new GameConfig(); // Use default values
        }
    }
}
