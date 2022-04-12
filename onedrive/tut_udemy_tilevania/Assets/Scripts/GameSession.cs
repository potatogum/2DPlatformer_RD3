using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSession : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int playerLives = 3;
    [SerializeField] private int score = 0;

    [Header("Restart Wait Timers")]
    [SerializeField] private float respawnWaitTime = 2f;
    [SerializeField] private float gameOverWaitTime = 4f;

    [Header("Canvas Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;

    void Awake()
    {
        GameSessionSingleton();
    }

    private void Start()
    {
        UpdateUIScoreAndLives();
    }

    private void GameSessionSingleton()
    {
        int numGameSessions = FindObjectsOfType<GameSession>().Length;
        
        if (numGameSessions > 1) { Destroy(gameObject); }
        else { DontDestroyOnLoad(gameObject); }
    }

    public void AddPointsToScore(int pointsToAdd)
    {
        score += pointsToAdd;
        UpdateUIScoreAndLives();
    }

    public void ProcessPlayerDeath()
    {
        if (playerLives > 1)
        {
            Invoke("TakeLife", respawnWaitTime);
        }
        else
        {
            Invoke("ResetGameSession", gameOverWaitTime);
        }
    }

    private void TakeLife()
    {
        playerLives --;
        UpdateUIScoreAndLives();
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private void ResetGameSession()
    {
        playerLives = 3;
        SceneManager.LoadScene(0);
        UpdateUIScoreAndLives();
        Destroy(gameObject);
    }

    private void UpdateUIScoreAndLives()
    {
        scoreText.text = score.ToString();
        livesText.text = playerLives.ToString();
    }
}
