using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    // array holds varity of tiles
    public GameObject[] objects;

    private void Start()
    {
        // instantiate a random object from the array and spawn it
        int rand = Random.Range(0, objects.Length);

        // we set all tiles for this room to be children of the room
        GameObject instance = (GameObject)Instantiate(objects[rand], transform.position, Quaternion.identity);
        instance.transform.parent = transform;
    }
}
