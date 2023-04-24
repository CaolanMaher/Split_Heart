using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Splash_Screen_Manager : MonoBehaviour
{

    [SerializeField] GameObject splashScreen;
    [SerializeField] GameObject optionsMenu;
    [SerializeField] GameObject newContinueMenu;
    [SerializeField] GameObject continueButton;

    private void Start()
    {
        splashScreen.SetActive(true);
        optionsMenu.SetActive(false);
        newContinueMenu.SetActive(false);
        continueButton.SetActive(false);
    }

    public void ShowSplashScreen()
    {
        splashScreen.SetActive(true);
        optionsMenu.SetActive(false);
        newContinueMenu.SetActive(false);
    }

    public void ShowOptionsScreen()
    {
        splashScreen.SetActive(false);
        optionsMenu.SetActive(true);
        newContinueMenu.SetActive(false);
    }

    public void ShowNewContinueScreen()
    {
        splashScreen.SetActive(false);
        optionsMenu.SetActive(false);
        newContinueMenu.SetActive(true);

        if (PlayerPrefs.HasKey("Level"))
        {
            continueButton.SetActive(true);
        }
        else
        {
            continueButton.SetActive(false);
        }
    }

    public void LoadNewGame()
    {
        // load level 1
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex + 1);
    }

    public void LoadContinueGame()
    {
        if (PlayerPrefs.HasKey("Level"))
        {
            SceneManager.LoadScene(PlayerPrefs.GetInt("Level"));
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
