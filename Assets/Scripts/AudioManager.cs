using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; // Singleton instance
    private void Awake()
    {
        if (instance == null)
        {
            instance = this; // Set the singleton instance
            DontDestroyOnLoad(gameObject); // Keep this object across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    [SerializeField] AudioSource backgroundMusic; // Reference to the background music AudioSource
    [SerializeField] AudioSource soundEffects; // Reference to the sound effects AudioSource


    [Header("Music")]
    public AudioClip mainMenuMusic; // Music for the main menu
    public AudioClip levelMusic; // Music for the level
    public AudioClip gameOverMusic; // Music for the game over screen
    public AudioClip victoryMusic; // Music for the victory screen

    [Header("Sound Effects")]
    public AudioClip dashSound; // Sound effect for jumping
    public AudioClip fireballSound; // Sound effect for firing a fireball
    public AudioClip fireFormSound; // Sound effect for switching to fire form
    public AudioClip slashSound; // Sound effect for jumping in knight form
    public AudioClip parrySound; // Sound effect for parrying
    public AudioClip knightFormSound; // Sound effect for switching to knight form
    public AudioClip hitSound; // Sound effect for jumping
    public AudioClip nextWaveSound; // Sound effect for next wave

    // Start is called before the first frame update
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
            PlayBackgroundMusic(mainMenuMusic);
        else if (scene.name == "Level")
            PlayBackgroundMusic(levelMusic);           
    }

    // Method to play background music
    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (backgroundMusic.clip != clip)
        {
            backgroundMusic.clip = clip;
            backgroundMusic.Play();
        }
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        if (clip != null)
        {
            soundEffects.PlayOneShot(clip);
        }
    }
}
