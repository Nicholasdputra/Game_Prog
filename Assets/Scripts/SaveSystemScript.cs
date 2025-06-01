using UnityEngine;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem currentSave;
    public PlayerDataSO currentPlayerData;
    public GameObject meleeEnemyPrefab; // Prefab for melee enemies
    public GameObject rangedEnemyPrefab; // Prefab for ranged enemies

    // Start is called before the first frame update
    void Awake()
    {
        if (currentSave == null)
        {
            // Debug.Log("SaveSystem instance created.");
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
        // Debug.Log(json);

        // Save the JSON string to a file
        using (System.IO.StreamWriter writer = new System.IO.StreamWriter(Application.persistentDataPath + "/save.json"))
        {
            writer.Write(json);
        }
    }

    public void SaveEnemiesForLevel(int levelIndex)
    {
        EnemyScript[] allEnemies = FindObjectsOfType<EnemyScript>();
        List<EnemyData> enemyDataList = new List<EnemyData>();
        foreach (var enemy in allEnemies)
        {
            EnemyData data = new EnemyData();
            // Detect type by script/component
            if (enemy.GetComponent<RangedEnemyScript>() != null)
                data.enemyType = "Ranged";
            else
                data.enemyType = "Melee";
            data.isAlive = enemy.health > 0;
            data.health = enemy.health;
            data.position = enemy.transform.position;
            enemyDataList.Add(data);
        }

        // Ensure array is initialized and has enough space for the level index
        if (currentPlayerData.enemiesInLevel == null || currentPlayerData.enemiesInLevel.Length <= levelIndex)
        {
            var temp = new List<EnemyData>[levelIndex + 1];
            if (currentPlayerData.enemiesInLevel != null)
                currentPlayerData.enemiesInLevel.CopyTo(temp, 0);
            currentPlayerData.enemiesInLevel = temp;
        }
        // Assign the enemy data list to the specific level index
        currentPlayerData.enemiesInLevel[levelIndex] = enemyDataList;
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

    public void LoadEnemiesForLevel(int levelIndex)
    {
        // Ensure the PlayerDataSO is initialized and has enemies for the level
        if (currentPlayerData.enemiesInLevel == null || currentPlayerData.enemiesInLevel.Length <= levelIndex)
            return;

        // Spawn enemies based on the saved data
        foreach (var enemyData in currentPlayerData.enemiesInLevel[levelIndex])
        {
            if (!enemyData.isAlive)
                continue; // Skip dead enemies

            GameObject prefab;
            if (enemyData.enemyType == "Ranged")
                prefab = rangedEnemyPrefab;
            else
                prefab = meleeEnemyPrefab;

            GameObject enemyObj = Instantiate(prefab, enemyData.position, Quaternion.identity);
            EnemyScript enemyScript = enemyObj.GetComponent<EnemyScript>();
            enemyScript.health = enemyData.health;
            // Optionally set other properties
        }
    }
}