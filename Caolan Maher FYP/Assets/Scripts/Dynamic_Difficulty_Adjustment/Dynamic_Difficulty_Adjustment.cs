using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dynamic_Difficulty_Adjustment : MonoBehaviour
{

    [SerializeField] private float playerScore;

    [SerializeField] private int enemyCount;
    [SerializeField] private int branchCount;
    [SerializeField] private int totalRoomCount;

    [SerializeField] private int totalDamageTaken;

    [SerializeField] private int timeToCompleteLevelEstimation;

    // How to estimate time to complete level:
    // > Each room should take approx 5 seconds to get through
    // > Each enemy should take approx 3 seconds to kill
    // > Initially, let's assume the player goes through every room and kills every enemy
    // > e.g then the estimation would be (roomCount * 5) + (enemyCount * 3)

    [SerializeField] private float timeInLevelSoFar = 0;

    private bool timerStarted = false;

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
}
