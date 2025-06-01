using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript Instance { get; private set; } // Singleton instance of GameManagerScript
    public PlayerDataSO playerData; // Reference to the PlayerData ScriptableObject

    public int currentLevel = 0; // Track the current level
    public bool isInLevel = false; // Track if the player is in a level
    public bool finishedWithLevel = false;

    public bool isGamePaused = false; // Track if the game is paused
    public bool isGameOver = false; // Track if the game is over

    public Coroutine timerCoroutine;
    public int timer;

    public int[] baseTimes = new int[3] { 60, 120, 180 }; // Base times for each level

    // UI elements for level select
    public GameObject baseContinueLevelButton;
    public GameObject baseStartLevelButton;

    public GameObject pausePanel;
    public GameObject resumeButton;
    public GameObject backToMainMenuButton;
    public GameObject settingsButton;
    public GameObject gameOverPanel;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            // Debug.Log("GameManagerScript instance created.");
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this instance across scenes
        }
        else
        {
            Debug.LogWarning("Another instance of GameManagerScript exists. Destroying this instance.");
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isGamePaused = !isGamePaused; // Toggle pause state
            if (isGamePaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }

        if (finishedWithLevel && !isGamePaused)
        {
            // Logic to handle when the player finishes a level
            Debug.Log("Player finished the level.");
            playerData.timeLeftAtCompletion[currentLevel] = playerData.timeElapsed[currentLevel]; // Save the time taken for the level
            playerData.levelCompleted[currentLevel] = true; // Mark the level as completed
            playerData.finalScoreAtCompletion[currentLevel] = playerData.currentScores[currentLevel]; // Save the score for the level
            playerData.timeElapsed[currentLevel] = 0; // Reset the time elapsed for the next playthrough
            playerData.currentScores[currentLevel] = 0; // Reset the current score for the next playthrough
            playerData.positionInLevel[currentLevel] = Vector2.zero; // Reset the player's position in the level
            Debug.Log($"Level {currentLevel} completed. Time: {playerData.timeLeftAtCompletion[currentLevel]} seconds, Score: {playerData.finalScoreAtCompletion[currentLevel]}");
            isInLevel = false; // Reset in-level state
            finishedWithLevel = false; // Reset finished state
            timer = 0; // Reset timer for next level
            //return to level selection here
        }
    }

    public IEnumerator TimerCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            timer--;
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f; // Stop the game time
        isGamePaused = true;
        Debug.Log("Game paused.");
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f; // Resume the game time
        isGamePaused = false;
        Debug.Log("Game resumed.");
    }

    private void OnApplicationQuit()
    {
        // Save player data when the application quits
        SaveSystem.currentSave.Save();
        
        // Debug.Log("GameManagerScript: Application quitting, saving player data.");
    }

    public void ContinueFromSaveOrNot(int levelChoice)
    {
        if (playerData.positionInLevel[levelChoice] != Vector2.zero)
        {
            baseContinueLevelButton.SetActive(true); // Show continue button if a save exists
            Button continueFromSaveButton = baseContinueLevelButton.GetComponent<Button>();
            continueFromSaveButton.onClick.AddListener(() => ContinueLevelFromSave(levelChoice));
        }
        baseStartLevelButton.SetActive(true); // Hide start level button
        Button newStartButton = baseStartLevelButton.GetComponent<Button>();
        newStartButton.onClick.AddListener(() => GoToLevelX(levelChoice));
    }

    public void GoToLevelX(int levelIndex)
    {
        string sceneName = "Level" + levelIndex;
        SceneManager.LoadScene("sceneName");
    }

    public void ContinueLevelFromSave(int levelIndex)
    {
        currentLevel = levelIndex;
        isInLevel = true;
        finishedWithLevel = false;
        timer = baseTimes[currentLevel] - playerData.timeElapsed[currentLevel];

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timerCoroutine = StartCoroutine(TimerCoroutine());

        Debug.Log($"Starting level {currentLevel} with timer set to {timer} seconds.");

        // Change scene based on level index
        string sceneName = "Level" + levelIndex; // Assumes scenes are named "Level1", "Level2", etc.
        SceneManager.LoadScene(sceneName);

        // Load enemies after the scene loads
        StartCoroutine(LoadEnemiesAfterSceneLoad(currentLevel));
    }
    
    private IEnumerator LoadEnemiesAfterSceneLoad(int levelIndex)
    {
        // Wait one frame to ensure the scene is loaded
        yield return null;
        // destroy all default enemies in the scene here
        foreach (var enemy in FindObjectsOfType<EnemyScript>())
            Destroy(enemy.gameObject);

        // Now load enemies from save
        SaveSystem.currentSave.LoadEnemiesForLevel(levelIndex);
    }
}