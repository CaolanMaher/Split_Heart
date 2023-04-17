using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class End_Room : MonoBehaviour
{

    Player player;

    GameObject dda;

    private void Start()
    {
        dda = GameObject.FindGameObjectWithTag("DDA");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {

            player = collision.GetComponent<Player>();

            PlayerPrefs.SetFloat("MaxHealth", player.GetMaxHealth());
            PlayerPrefs.SetFloat("CurrentHealth", player.GetCurrentHealth());
            PlayerPrefs.SetFloat("CurrentFuryCharge", player.GetCurrentFuryCharge());

            dda.GetComponent<Dynamic_Difficulty_Adjustment>().CalculatePlayerScore();

            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex + 1);
        }
    }
}
