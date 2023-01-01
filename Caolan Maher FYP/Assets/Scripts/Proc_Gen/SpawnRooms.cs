using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Check if each pose game object detects a room nearby after critical path is complete
// If there is a room already there, don't do anthing
// If there is no room there, we generate a random room from our array

public class SpawnRooms : MonoBehaviour
{

    public LayerMask roomMask;
    public LevelGeneration1 levelGen;

    //public float timeBetweenRoomSpawn;
    //public float spawnRoomCooldown;

    private void Update()
    {

        Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomMask);

        // check if a room has been created after the critical path is done
        if(roomDetection == null && levelGen.generationIsStopped)
        {
            // if not, we want to spawn a random room
            int rand = Random.Range(0, levelGen.rooms.Length);
            Instantiate(levelGen.rooms[rand], transform.position, Quaternion.identity);
            //GameObject newRoom = Instantiate(levelGen.rooms[rand], transform.position, Quaternion.identity);
            //newRoom.GetComponent<BoxCollider2D>().enabled = false;

            // stop this from creating multiple rooms on itself
            Destroy(gameObject);
        }

    }

}
