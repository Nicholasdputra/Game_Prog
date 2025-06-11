using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManagerScript : MonoBehaviour
{
    
    public static GameManagerScript instance; // Singleton instance
    public PlayerSaveDataSO playerSave;

    public int currentScore; // Current score of the player

    public bool isGamePaused = false; // Track if the game is paused
    public bool isGameOver = false; // Track if the game is over

    public TextMeshProUGUI scoreText; // UI Text to display the score
    public int score;
    public TextMeshProUGUI timerText; // UI Text to display the timer
    public Coroutine timerCoroutine;
    public int timer;

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
        if (SceneManager.GetActiveScene().name == "Level" && timerCoroutine == null)
        {
            timerCoroutine = StartCoroutine(TimerCoroutine());
        }
        score = 0;
        if (instance == null)
        {
            instance = this; // Set the singleton instance
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
            return;
        }

        // Initialize UI elements
        // pausePanel.SetActive(false);
        // gameOverPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "Score: " + score.ToString();
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

        //if we're in a scene called Level, we start the timer coroutine
        if (SceneManager.GetActiveScene().name == "Level" && timerCoroutine == null)
        {
            timerCoroutine = StartCoroutine(TimerCoroutine());
        }
    }

    public IEnumerator TimerCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            timer++;
            //make the time text in XX:XX format
            int minutes = timer / 60;
            int seconds = timer % 60;
            timerText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds); // Update the timer text
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

    public void SpawnWave(WaveData wave)
    {
        foreach (var enemyInfo in wave.enemies)
        {
            Instantiate(enemyInfo.enemy, enemyInfo.position, enemyInfo.rotation);
        }
    }

    private void NextWave()
    {
        playerSave.wavesCompleted++;
        playerSave.timeSurvived = timer; // Save the time survived in the player data
        playerSave.score = currentScore; // Example scoring logic
        // Logic to proceed to the next wave
        Debug.Log("Proceeding to the next wave.");
    }
}