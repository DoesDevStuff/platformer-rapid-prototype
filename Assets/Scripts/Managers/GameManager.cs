using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance;

    void Awake()
    {
        // Ensure only one instance of GameManager exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Check for "Escape" key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitFullScreen();
        }
    }

    public void ReloadSceneWithDelay()
    {
        StartCoroutine(ReloadSceneCoroutine());
    }

    IEnumerator ReloadSceneCoroutine()
    {
        yield return new WaitForSeconds(2.0f);

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    void ExitFullScreen()
    {
        // Check if the game is running in full screen
        if (Screen.fullScreen)
        {
            // Toggle full screen mode off
            Screen.fullScreen = false;
        }
    }
}