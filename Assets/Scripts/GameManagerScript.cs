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
    public GameObject continueFromSaveButton;
    public GameObject baseStartLevelButton;
    
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
            playerData.timeForLevel[currentLevel] = timer; // Save the time taken for the level
            playerData.levelCompleted[currentLevel] = true; // Mark the level as completed
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
            continueFromSaveButton.SetActive(true); // Show continue button if a save exists
        }
        baseStartLevelButton.SetActive(true); // Hide start level button
        Button newStartButton = baseStartLevelButton.GetComponent<Button>();
        newStartButton.onClick.AddListener(() => StartLevel(levelChoice));
    }

    public void GoToLevel1()
    {
        SceneManager.LoadScene("Level1");
        StartLevel(0);
    }

    public void GoToLevel2()
    {
        SceneManager.LoadScene("Level2");
        StartLevel(1);
    }

    public void GoToLevel3()
    {
        SceneManager.LoadScene("Level3");
        StartLevel(2);
    }

    public void StartLevel(int levelIndex)
    {
        currentLevel = levelIndex; // Set the current level
        isInLevel = true; // Mark that the player is in a level
        finishedWithLevel = false; // Reset finished state
        timer = baseTimes[currentLevel] - playerData.timeForLevel[currentLevel];

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine); // Stop any existing timer coroutine
        }
        timerCoroutine = StartCoroutine(TimerCoroutine()); // Start the timer coroutine

        Debug.Log($"Starting level {currentLevel} with timer set to {timer} seconds.");
    }
}