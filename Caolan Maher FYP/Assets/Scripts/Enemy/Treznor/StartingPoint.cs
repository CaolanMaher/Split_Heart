using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingPoint : MonoBehaviour
{

    [SerializeField] int pointNumber;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Treznor"))
        {
            collision.GetComponent<FollowRoute>().startingPoint = pointNumber;
        }
    }
}
