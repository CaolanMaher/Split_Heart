using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayManager : MonoBehaviour
{

    public GameObject pauseMenu;

    bool gameIsPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && !gameIsPaused)
        {
            PauseGame();
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && gameIsPaused)
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
}
