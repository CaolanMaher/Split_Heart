using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnlyWithHighScore : MonoBehaviour
{
    // array holds varity of tiles
    public GameObject[] objects;

    private void Start()
    {
        // instantiate a random object from the array and spawn it
        //int rand = Random.Range(0, objects.Length);

        if (PlayerPrefs.HasKey("PlayerScore"))
        {
            if (PlayerPrefs.GetFloat("PlayerScore") >= 750)
            {

                print("HIGH_SCORE");

                // we set all tiles for this room to be children of the room
                GameObject instance = (GameObject)Instantiate(objects[0], transform.position, Quaternion.identity);
                instance.transform.parent = transform;
            }
        }
    }
}
