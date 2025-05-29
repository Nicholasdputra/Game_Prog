using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem currentSave;
    public PlayerDataSO currentPlayerData;

    // Start is called before the first frame update
    void Awake()
    {
        if (currentSave == null)
        {
            Debug.Log("SaveSystem instance created.");
            currentSave = this;
            DontDestroyOnLoad(gameObject);
            InitializePlayerData();
        }
        else
        {
            Debug.LogWarning("Another instance of SaveSystem exists. Destroying this instance.");
            Destroy(gameObject);
        }
    }

    private void InitializePlayerData()
    {
        // Initialize the PlayerDataSO if it doesn't exist
        if (currentPlayerData == null)
        {
            // Create a new instance of PlayerDataSO
            currentPlayerData = ScriptableObject.CreateInstance<PlayerDataSO>();
        }
    }

    public void Save()
    {
        // Ensure the PlayerDataSO is initialized
        string json = JsonUtility.ToJson(currentPlayerData);
        Debug.Log(json);

        // Save the JSON string to a file
        using (System.IO.StreamWriter writer = new System.IO.StreamWriter(Application.persistentDataPath + "/save.json"))
        {
            writer.Write(json);
        }
    }

    void Load()
    {
        // Load the JSON string from a file
        if (System.IO.File.Exists(Application.persistentDataPath + "/save.json"))
        {
            // Read the JSON string from the file
            Debug.Log("Loading save file from: " + Application.persistentDataPath + "/save.json");
            using (System.IO.StreamReader reader = new System.IO.StreamReader(Application.persistentDataPath + "/save.json"))
            {
                // Read the entire file content
                string json = reader.ReadToEnd();
                currentPlayerData = JsonUtility.FromJson<PlayerDataSO>(json);
            }
        }
        else
        {
            // Handle the case where the save file does not exist
            Debug.LogError("Save file not found!");
        }
    }
}