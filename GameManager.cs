using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int score = 0;
    private float gameTime;
    public AudioSource backgroundMusic; // Drag your background music AudioSource here
    public BananaController bananaController; // Reference to BananaController

    private int totalPellets;
    private int collectedPellets;
    public int playerLives = 3; // Track player lives

    private bool isGameOver = false; // Flag to check if the game is over
    private int highScore = 0; // High score variable

    private void Awake()
    {
        // Singleton pattern to ensure only one GameManager instance
        if (Instance == null)
        {
            Instance = this;
            LoadHighScore(); // Load the high score when the game starts
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Ensure game does not start immediately; wait for countdown
        DisablePlayerControl();
        DisableGhostMovement();
        gameTime = 0f; // Initialize game time at start

        // Count all pellets in the level
        totalPellets = GameObject.FindGameObjectsWithTag("Pellet").Length +
                       GameObject.FindGameObjectsWithTag("PowerPellet").Length;
        collectedPellets = 0;
    }

    private void Update()
    {
        if (!isGameOver) // Increment game time while the game is active
        {
            gameTime += Time.deltaTime;
        }
    }

    public void StartGame()
    {
        // Called by HUDManager after the countdown finishes
        EnablePlayerControl();
        EnableGhostMovement();
        StartBackgroundMusic();

        // Start banana spawning after the countdown
        if (bananaController != null)
        {
            bananaController.StartSpawning();
        }

        // Play ghost normal music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGhostMusic("normal");
        }
    }

    public void AddScore(int value)
    {
        score += value;
        Debug.Log("Score Updated: " + score);

        // Update HUD score UI if needed
        HUDManager hud = Object.FindFirstObjectByType<HUDManager>();
        if (hud != null)
        {
            hud.UpdateScoreUI(score);
        }
    }

    public int GetScore() // New method to get the current score
    {
        return score;
    }

    public void PelletCollected()
    {
        collectedPellets++;
        if (collectedPellets >= totalPellets)
        {
            GameOver();
        }
    }

    public void PacStudentDied()
    {
        playerLives--;
        if (playerLives <= 0)
        {
            GameOver();
        }
        else
        {
            // Respawn or handle PacStudent life loss scenario here if needed
        }
    }

    public void PowerPelletEaten()
    {
        Debug.Log("Power Pellet Eaten! Ghosts are scared!");
        // Logic for setting ghosts to scared state
        // Call methods in GhostController if needed to trigger the scared state
    }

    private void EnablePlayerControl()
    {
        PacStudentController pacStudent = Object.FindFirstObjectByType<PacStudentController>();
        if (pacStudent != null)
        {
            pacStudent.SetMovementEnabled(true);
        }
    }

    private void DisablePlayerControl()
    {
        PacStudentController pacStudent = Object.FindFirstObjectByType<PacStudentController>();
        if (pacStudent != null)
        {
            pacStudent.SetMovementEnabled(false);
        }
    }

    private void EnableGhostMovement()
    {
        GhostController[] ghosts = Object.FindObjectsByType<GhostController>(FindObjectsSortMode.None);
        foreach (GhostController ghost in ghosts)
        {
            ghost.SetMovementEnabled(true);
        }
    }

    private void DisableGhostMovement()
    {
        GhostController[] ghosts = Object.FindObjectsByType<GhostController>(FindObjectsSortMode.None);
        foreach (GhostController ghost in ghosts)
        {
            ghost.SetMovementEnabled(false);
        }
    }

    private void StartBackgroundMusic()
    {
        if (backgroundMusic != null && !backgroundMusic.isPlaying)
        {
            backgroundMusic.loop = true;
            backgroundMusic.Play();
        }
    }

    public void StopGhostMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopGhostMusic(); // Stop ghost music
        }
    }

    public void GameOver()
    {
        Debug.Log("Game Over");

        DisablePlayerControl();
        DisableGhostMovement();

        StopGhostMusic(); // Stop the ghost music

        if (backgroundMusic.isPlaying)
        {
            backgroundMusic.Stop();
        }

        // Check and update high score
        if (score > highScore)
        {
            highScore = score; // Update high score
            SaveHighScore(); // Save the new high score
        }

        HUDManager hud = Object.FindFirstObjectByType<HUDManager>();
        if (hud != null)
        {
            hud.StopGameTimer();  // Stop timer updates in HUD
            hud.UpdateScoreUI(score);
            hud.UpdateGameTimerUI(gameTime);
            hud.DisplayGameOverText();
        }

        Invoke("ReturnToStartScene", 3f);
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();

        Debug.Log($"High score saved: {highScore}");
    }

    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        Debug.Log($"Loaded high score: {highScore}");
    }

    private void ReturnToStartScene()
    {
        StopGhostMusic(); // Stop the ghost music before loading the Start scene
        SceneManager.LoadScene("StartScene");
    }
}
