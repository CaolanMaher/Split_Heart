using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Dynamic_Difficulty_Adjustment : MonoBehaviour
{

    GameObject player;

    [SerializeField] private float playerScore = 500;

    [SerializeField] private int enemyCount;
    [SerializeField] private int enemiesKilled;
    [SerializeField] private int branchCount;
    [SerializeField] private int totalRoomCount;

    [SerializeField] private int totalDamageTaken;

    [SerializeField] private float timeToCompleteLevelEstimation;

    // How to estimate time to complete level:
    // > Each room should take approx 5 seconds to get through
    // > Each enemy should take approx 3 seconds to kill
    // > Initially, let's assume the player goes through every room and kills every enemy
    // > e.g then the estimation would be (roomCount * 5) + (enemyCount * 3)

    [SerializeField] private float timeInLevelSoFar = 0;

    private bool timerStarted = false;

    private void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if(currentScene.name == "Level_2")
        {
            PlayerPrefs.SetFloat("PlayerScore", 900);
            playerScore = PlayerPrefs.GetFloat("PlayerScore");

            player = GameObject.FindGameObjectWithTag("Player");

            // player is playing well
            if (playerScore >= 750)
            {
                player.GetComponent<Player>().takeDamageMultiplier = 1.15f;
            }
            else if(playerScore <= 250)
            {
                player.GetComponent<Player>().takeDamageMultiplier = 0.85f;
            }
            else if(playerScore > 250 && playerScore < 750)
            {
                player.GetComponent<Player>().takeDamageMultiplier = 1f;
            }
        }
        else
        {
            // a middle ground for a new player
            playerScore = 500;
            PlayerPrefs.SetFloat("PlayerScore", playerScore);
        }
    }

    private void Update()
    {
        if (timerStarted)
        {
            timeInLevelSoFar += Time.deltaTime;
        }
    }

    // Called by proc gen after its done
    public void LevelGenIsCompleted()
    {
        EstimateTimeToCompleteLevel();

        timerStarted = true;
    }

    private void EstimateTimeToCompleteLevel()
    {
        timeToCompleteLevelEstimation = (totalRoomCount * 5) + (enemyCount * 3);
    }

    public void CalculatePlayerScore()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        // get the remaining number of enemies
        int numberOfEnemiesLeft = GameObject.FindGameObjectsWithTag("Enemy").Length;

        int killedEnemies = enemyCount - numberOfEnemiesLeft;

        // for every enemy killed add 10 to the player's score
        playerScore += killedEnemies * 10;

        // for every damage point taken remove 1.5 from the player's score
        playerScore -= player.GetComponent<Player>().totalDamageTaken * 1.5f;

        // get the time difference in estimation and actual level finish
        float timeDifferenceToCompleteLevel = timeToCompleteLevelEstimation - timeInLevelSoFar;

        // for every second of difference, add or subtract 1 from player score
        if(timeDifferenceToCompleteLevel > 0)
        {
            playerScore += timeDifferenceToCompleteLevel;
        }
        else
        {
            playerScore -= timeDifferenceToCompleteLevel;
        }

        print(playerScore);

        // Submit all info to PlayerPrefs

        PlayerPrefs.SetFloat("PlayerScore", playerScore);
    }

    // Called by proc gen after its done
    public void SetEnemyCount(int count)
    {
        enemyCount = count;
    }

    // Called by proc gen after its done
    public void SetBranchCount(int count)
    {
        branchCount = count;
    }

    // Called by proc gen after its done
    public void SetTotalRoomCount(int count)
    {
        totalRoomCount = count;
    }

    public float GetPlayerScore()
    {
        return playerScore;
    }
}
