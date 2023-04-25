using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPoundTrigger : MonoBehaviour
{
    FollowRoute followRoute;

    private void Start()
    {
        followRoute = transform.parent.GetComponent<FollowRoute>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Tilemap") && followRoute.justGroundPounded)
        {
            followRoute.GetAnimator().SetBool("isInAir", false);
        }
        else if (collision.CompareTag("Player") && followRoute.justGroundPounded)
        {
            collision.GetComponent<Player>().TakeDamage(30);
        }
    }
}
