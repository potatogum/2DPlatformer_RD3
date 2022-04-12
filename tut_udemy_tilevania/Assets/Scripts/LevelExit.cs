using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [SerializeField] private float LoadLevelDelay = 2f;
    [SerializeField] private float slowMotionScale = 0.2f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        StartCoroutine(LoadNextLevel());
    }

    private IEnumerator LoadNextLevel()
    {
        Time.timeScale = slowMotionScale;
        yield return new WaitForSecondsRealtime(LoadLevelDelay);
        Time.timeScale = 1;

        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
}
