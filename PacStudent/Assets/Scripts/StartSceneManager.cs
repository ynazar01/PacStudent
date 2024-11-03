using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartSceneManager : MonoBehaviour
{
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI bestTimeText;
    public AudioSource introMusic; // New: Drag your intro music AudioSource here

    private void Start()
    {
        // Play intro music
        if (introMusic != null && !introMusic.isPlaying)
        {
            introMusic.loop = true;
            introMusic.Play();
        }

        // Retrieve the high score and best time from PlayerPrefs
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime", 0f);

        // Format the best time as minutes:seconds:milliseconds
        int minutes = (int)(bestTime / 60);
        int seconds = (int)(bestTime % 60);
        int milliseconds = (int)((bestTime * 100) % 100);

        // Display the high score and best time separately
        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + highScore;
        }

        if (bestTimeText != null)
        {
            bestTimeText.text = "Time: " + string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        }

        Debug.Log("Loaded high score: " + highScore + " with time: " + bestTime);
    }

    public void LoadLevel1()
    {
        if (introMusic != null && introMusic.isPlaying)
        {
            introMusic.Stop();
        }
        SceneManager.LoadScene("PacStudent"); 
    }

    public void LoadLevel2()
    {
        if (introMusic != null && introMusic.isPlaying)
        {
            introMusic.Stop();
        }
        SceneManager.LoadScene("Level 2"); 
    }

    public void LoadStartScene()
    {
        if (introMusic != null && introMusic.isPlaying)
        {
            introMusic.Stop();
        }
        SceneManager.LoadScene("StartScene");
    }
}
