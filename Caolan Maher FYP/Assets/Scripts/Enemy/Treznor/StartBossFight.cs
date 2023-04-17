using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartBossFight : MonoBehaviour
{

    bool bossFightStarted = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !bossFightStarted)
        {
            bossFightStarted = true;

            GameObject.FindGameObjectWithTag("Treznor").GetComponent<TreznorBossFight>().StartBossFight();

            GameObject.FindGameObjectWithTag("Background_Music").GetComponent<BackgroundMusicFade>().StartBossFight();
        }
    }
}
