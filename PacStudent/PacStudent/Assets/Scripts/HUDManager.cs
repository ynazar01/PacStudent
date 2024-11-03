using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public List<Image> heartImages;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI ghostTimerText;
    public TextMeshProUGUI gameTimerText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI gameOverText;

    private float gameTime;
    private bool isGameStarted = false;
    private bool isGameOver = false; // Flag to stop updating the timer after Game Over

    void Start()
    {
        gameTime = 0f;
        UpdateGameTimerUI(gameTime);
        StartCoroutine(StartCountdown());
        gameOverText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isGameStarted && !isGameOver)
        {
            gameTime += Time.deltaTime;
            UpdateGameTimerUI(gameTime);
        }
    }

    public void StopGameTimer()
    {
        isGameOver = true;
    }

    private IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSeconds(1f);

        countdownText.text = "2";
        yield return new WaitForSeconds(1f);

        countdownText.text = "1";
        yield return new WaitForSeconds(1f);

        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);

        countdownText.gameObject.SetActive(false);
        isGameStarted = true; // Start the game timer only after countdown finishes

        GameManager.Instance.StartGame();
    }

    public void UpdateLivesUI(int lives)
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            heartImages[i].enabled = i < lives;
        }
    }

    public void UpdateScoreUI(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
        else
        {
            Debug.LogWarning("Score Text is not assigned in the Inspector.");
        }
    }

    public void UpdateGhostTimerUI(float timeRemaining)
    {
        if (ghostTimerText != null)
        {
            ghostTimerText.text = "Ghost Scared: " + Mathf.Ceil(timeRemaining).ToString();
            ghostTimerText.gameObject.SetActive(timeRemaining > 0);
        }
    }

    public void UpdateGameTimerUI(float time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        int milliseconds = (int)((time - (int)time) * 100);
        gameTimerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", minutes, seconds, milliseconds);
    }

    public void DisplayGameOverText()
    {
        gameOverText.gameObject.SetActive(true);
    }
}
