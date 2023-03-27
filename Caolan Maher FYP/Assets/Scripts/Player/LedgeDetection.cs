using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeDetection : MonoBehaviour
{
    /*
    [SerializeField] float radius;

    [SerializeField] LayerMask groundMask;

    //[SerializeField] Player player;

    [SerializeField] bool canDetect;

    private void Update()
    {
        if (canDetect)
        {
            //playerMovement.ledgeDetected = Physics2D.OverlapCircle(transform.position, radius, groundMask);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Tilemap"))
        {
            canDetect = false;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Tilemap"))
        {
            canDetect = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Tilemap"))
        {
            canDetect = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    */
}
