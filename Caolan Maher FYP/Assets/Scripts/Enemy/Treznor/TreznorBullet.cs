using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreznorBullet : MonoBehaviour
{

    private int damage = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Tilemap") || collision.CompareTag("Boss_Doors"))
        {
            Destroy(gameObject.transform.parent.gameObject);
        }
        else if(collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().TakeDamage(damage);
            Destroy(gameObject.transform.parent.gameObject);
        }
    }
}
