using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{

    [SerializeField] float health = 30;

    Dynamic_Difficulty_Adjustment dda;

    private void Start()
    {
        dda = GameObject.FindGameObjectWithTag("DDA").GetComponent<Dynamic_Difficulty_Adjustment>();

        if(dda.GetPlayerScore() >= 750)
        {
            health = health / 1.15f;
        }
        else if(dda.GetPlayerScore() <= 250)
        {
            health = health * 1.15f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().AddHealth(health);
            GetComponent<BoxCollider2D>().enabled = false;
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<AudioSource>().Play();
            Destroy(gameObject.transform.parent.gameObject, 0.5f);
        }
    }
}
