using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayManager : MonoBehaviour
{

    public GameObject pauseMenu;
    public GameObject deadMenu;

    bool gameIsPaused = false;

    bool playerAlive = true;

    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.SetActive(false);
        deadMenu.SetActive(false);

        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && !gameIsPaused && playerAlive)
        {
            PauseGame();
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && gameIsPaused && playerAlive)
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        // Pause the game
        gameIsPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        // Resume the game
        gameIsPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void PlayerDied()
    {
        deadMenu.SetActive(true);
        playerAlive = false;
        Time.timeScale = 0;
    }

    public void BackToSplashScreen()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Splash_Screen");
    }
}
