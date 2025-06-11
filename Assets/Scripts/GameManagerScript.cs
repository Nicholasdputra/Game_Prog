using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript instance; // Singleton instance

    public PlayerSaveDataSO playerSave;
    public WaveListScriptableObject waveList; // Reference to the wave list scriptable object
    public int currentWaveIndex = 0; // Current wave index

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
    public GameObject settingsButton;

    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverScoreText; // Text to display score on game over
    public TextMeshProUGUI gameOverTimerText; // Text to display timer on game over
    public TextMeshProUGUI gameOverWavesText; // Text to display waves completed on game over
    public TextMeshProUGUI gameOverEnemiesDefeatedText; // Text to display enemies defeated on game over

    public GameObject victoryPanel; // Panel to show when all waves are completed
    public TextMeshProUGUI victoryScoreText; // Text to display score on victory
    public TextMeshProUGUI victoryTimerText; // Text to display timer on victory
    public TextMeshProUGUI victoryEnemiesDefeatedText; // Text to display enemies defeated on victory
    public TextMeshProUGUI victoryWavesText; // Text to display waves completed on victory


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

        if (SceneManager.GetActiveScene().name == "Level")
        {
            StartLevel(); // Reset player data if in main menu
        }
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

        if (SceneManager.GetActiveScene().name == "Level")
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies.Length == 0)
            {
                int nextWaveIndex = playerSave.wavesCompleted + 1;
                if (nextWaveIndex < waveList.waves.Length)
                {
                    NextWave();
                }
                else
                {
                    // No more waves, show victory panel
                    ShowVictoryPanel();
                }
            }
        }
    }

    [ContextMenu("Start Level")]
    public void StartLevel()
    {
        if (SceneManager.GetActiveScene().name == "Level")
        {
            SpawnWave(waveList.waves[playerSave.wavesCompleted]);
            timer = playerSave.timeSurvived; // Load the saved time
            currentScore = playerSave.score; // Load the saved score
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

    public void ReturnToMainMenu()
    {
        isGameOver = false; // Reset game over state
        if (isGamePaused)
        {
            ResumeGame(); // Resume the game if paused
        }
        Time.timeScale = 1f; // Ensure time scale is reset
        SceneManager.LoadScene("MainMenu"); // Load the main menu scene
    }

    public void PauseGame()
    {
        Time.timeScale = 0f; // Stop the game time
        isGamePaused = true;
        pausePanel.SetActive(true); // Show the pause panel
        Debug.Log("Game paused.");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; // Resume the game time
        isGamePaused = false;
        pausePanel.SetActive(false); // Hide the pause panel
        Debug.Log("Game resumed.");
    }

    private void NewPlaythrough()
    {
        // Reset player data and UI elements
        playerSave.wavesCompleted = 0;
        playerSave.enemiesDefeated = 0;
        playerSave.timeSurvived = 0;
        playerSave.score = 0;
        currentWaveIndex = 0;
        currentScore = 0;

        // Reset the timer
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        timer = 0;

        // Reset UI elements
        scoreText.text = "Score: " + currentScore.ToString();
        timerText.text = "00:00";

        // Optionally, reset the game state or load a new scene
        SceneManager.LoadScene("Level"); // Load the level scene
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
        AudioManager.instance.PlaySoundEffect(AudioManager.instance.nextWaveSound); // Play next wave sound effect
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            DestroyImmediate(enemy); // Use DestroyImmediate in editor scripts, Destroy in runtime
        }
        playerSave.timeSurvived = timer; // Save the time survived in the player data
        playerSave.score = currentScore; // Example scoring logic
        playerSave.enemiesDefeated += waveList.waves[currentWaveIndex].enemies.Length; // Update enemies defeated count

        int nextWaveIndex = playerSave.wavesCompleted + 1;
        if (nextWaveIndex < waveList.waves.Length)
        {
            SpawnWave(waveList.waves[nextWaveIndex]);
            playerSave.wavesCompleted++;
        }
        else
        {
            Debug.Log("All waves completed!");
            // Optionally handle end of waves/game here
        }
        Debug.Log("Proceeding to the next wave.");
    }

    public void ShowGameOverPanel()
    {
        // Activate the game over panel
        gameOverPanel.SetActive(true);

        // Final Score
        gameOverScoreText.text = $"Final Score: {currentScore}";

        // Time Elapsed in MM:SS
        int minutes = timer / 60;
        int seconds = timer % 60;
        gameOverTimerText.text = $"Time Elapsed: {minutes:D2}:{seconds:D2}";

        // Waves Completed
        gameOverWavesText.text = $"Waves Completed: {playerSave.wavesCompleted}";

        // Calculate total enemies defeated
        int currWaveEnemies = waveList.waves[currentWaveIndex].enemies.Length;
        int enemiesLeft = GameObject.FindGameObjectsWithTag("Enemy").Length;
        int totalDefeated = playerSave.enemiesDefeated + (currWaveEnemies - enemiesLeft);

        gameOverEnemiesDefeatedText.text = $"Total Enemies Defeated: {totalDefeated}";

        playerSave.timeSurvived = 0; // Reset the timer for the next playthrough
        playerSave.score = 0; // Reset the score for the next playthrough
        playerSave.wavesCompleted = 0; // Reset waves completed for the next playthrough
        playerSave.enemiesDefeated = 0; // Reset enemies defeated for the next playthrough
        isGameOver = true; // Set game over state
    }

    public void ShowVictoryPanel()
    {
        victoryPanel.SetActive(true);

        playerSave.timeSurvived = timer; // Save the time survived in the player data
        playerSave.score = currentScore; // Example scoring logic
        playerSave.enemiesDefeated += waveList.waves[currentWaveIndex].enemies.Length; // Update enemies defeated count
        playerSave.wavesCompleted++;

        // Optionally update victory panel texts here
        Debug.Log("Victory! All waves completed.");
        victoryScoreText.text = $"Final Score: {currentScore}";
        int minutes = timer / 60;
        int seconds = timer % 60;
        victoryTimerText.text = $"Time Elapsed: {minutes:D2}:{seconds:D2}";
        victoryEnemiesDefeatedText.text = $"Total Enemies Defeated: {playerSave.enemiesDefeated}";
        victoryWavesText.text = $"Waves Completed: 5";

        playerSave.timeSurvived = 0; // Reset the timer for the next playthrough
        playerSave.score = 0; // Reset the score for the next playthrough
        playerSave.wavesCompleted = 0; // Reset waves completed for the next playthrough
        playerSave.enemiesDefeated = 0; // Reset enemies defeated for the next playthrough
    }
}