using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneration1 : MonoBehaviour
{
    // array of where start of level can be
    public Transform[] startingPositions;

    // array of rooms that can be placed
    public GameObject[] rooms; // index 0 -> LR, index 1 -> LRB, index 2 -> LRT, index 3 -> LRBT

    // array of spawned rooms
    public List<GameObject> spawnedRooms;

    // pick which direction to go next
    private int direction;

    // amount to move on x axis to spawn new room
    public float moveAmountX;

    // amount to move on Y axis to spawn new room
    public float moveAmountY;

    // floats of the bounds of the level generation
    public float minX;
    public float maxX;
    public float minY;

    // bool to see if we should keep generating the level
    public bool generationIsStopped = false;

    // layermask for overlapsphere function
    public LayerMask roomMask;

    // count how many times we go down
    public int downConter;

    public float timeBetweenRoomSpawn;
    public float spawnRoomCooldown;

    // info to spawn player
    public GameObject player;
    public Transform playerSpawnPoint;

    private void Start()
    {
        //spawnedRooms = new List<GameObject>();

        // get a random starting point, set this objects position to it, and spawn our first room
        int randStartingPos = Random.Range(0, startingPositions.Length);
        transform.position = startingPositions[randStartingPos].position;
        GameObject newRoom = Instantiate(rooms[0], transform.position, Quaternion.identity);

        //spawnedRooms.Add(newRoom);

        //print(currentRoom);

        // set direction to random number between 1 and 5
        direction = Random.Range(1, 6);

        // spawn new room
        Move();
    }

    private void Update()
    {
        if (timeBetweenRoomSpawn <= 0 && !generationIsStopped)
        {
            Move();
            timeBetweenRoomSpawn = spawnRoomCooldown;
        }
        else
        {
            if (!generationIsStopped)
            {
                timeBetweenRoomSpawn -= Time.deltaTime;
            }
        }
    }

    // move to spawn a new room
    private void Move()
    {
        // there is less chance of moving down
        // keep this for now as it makes it more likely the player has to go through more of level
        if(direction == 1 || direction == 2)
        {
            //print(transform.position.x + " : " + maxX);
            if(transform.position.x < maxX)
            {
                // reset down counter
                downConter = 0;

                // we are within the maxX so we can move right
                Vector2 newPos = new Vector2(transform.position.x + moveAmountX, transform.position.y);
                transform.position = newPos;

                // all rooms have openings on right, so we pick on at random
                int rand = Random.Range(0, rooms.Length);
                GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);

                //spawnedRooms.Add(newRoom);

                //print(currentRoom);

                // set direction to random number between 1 and 6
                direction = Random.Range(1, 6);

                // we check if this will mean it will try and go left on the next run
                if(direction == 3)
                {
                    // if so, we change it so that it will go right instead
                    // this is so that if it goes right, it can't go left and overwrite itself
                    direction = 2;
                }
                else if(direction == 4)
                {
                    // or we can change it to go down to keep the randomness
                    direction = 5;
                }
            }
            else
            {
                //print("Setting Direction to down from right");

                // we reached the maxX so we must move down
                direction = 5;
            }

            //print("Current Direction: " + direction);
        }
        else if(direction == 3 || direction == 4)
        {
            //print(transform.position.x + " : " + minX);
            if (transform.position.x > minX)
            {
                // reset down counter
                downConter = 0;

                // we are within the minX so we can move left
                Vector2 newPos = new Vector2(transform.position.x - moveAmountX, transform.position.y);
                transform.position = newPos;

                // all rooms have openings on left, so we pick on at random
                int rand = Random.Range(0, rooms.Length);
                GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);

                //spawnedRooms.Add(newRoom);

                //print(currentRoom);

                // set direction to random number between 3 and 4
                // this is so that if it goes left, it can't go right and overwrite itself
                direction = Random.Range(3, 5);
            }
            else
            {
                //print("Setting Direction to down from Left");

                // we reached the minX so we must move down
                direction = 5;
            }

            //print("Current Direction: " + direction);
        }
        else if(direction == 5)
        {

            // increment down counter
            downConter++;

            //print(transform.position.y + " : " + minY);
            if (transform.position.y > minY)
            {

                // Get the room we just created before this
                Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomMask);

                //print(transform.position);

                //print(currentRoom.transform.childCount);

                // check if the room found has a bottom opening
                if (roomDetection.GetComponent<RoomType>().type != 1 && roomDetection.GetComponent<RoomType>().type != 3)
                {

                    // has our level gen has moved down more than 2 times in a row
                    if(downConter >= 2)
                    {
                        // if so, we want to spawn a room that has openings on all 4 ways
                        // previously, if we went down twice or more, there's a chance a room with no bottom opening could spawn
                        // resulting in a dead end

                        // we destroy the room that might have not had a bottom opening
                        //print("DESTROYING ROOM WITH NO BOTTOM OPENING AND COUNT >= 2");
                        roomDetection.GetComponent<RoomType>().RoomDestruction();
                        Instantiate(rooms[3], transform.position, Quaternion.identity);
                        //GameObject newRoom_local = Instantiate(rooms[3], transform.position, Quaternion.identity);

                        //spawnedRooms.Add(newRoom_local);
                    }
                    else
                    {
                        // if not, we destroy the last room and spwan a room with all 4 openings or with LRB
                        //print("DESTROYING ROOM WITH NO BOTTOM OPENING AND COUNT < 2");
                        roomDetection.GetComponent<RoomType>().RoomDestruction();

                        // we want to create a room that has a bottom opening
                        // we want an index of 1 or 3
                        int randomBottomRoom = Random.Range(1, 4);
                        // if we get 2
                        if (randomBottomRoom == 2)
                        {
                            // make it into 1
                            randomBottomRoom = 1;
                        }
                        Instantiate(rooms[randomBottomRoom], transform.position, Quaternion.identity);
                        //GameObject newRoom_local = Instantiate(rooms[randomBottomRoom], transform.position, Quaternion.identity);

                        //spawnedRooms.Add(newRoom_local);
                    }
                }

                // We are within the minY so we can move down
                Vector2 newPos = new Vector2(transform.position.x, transform.position.y - moveAmountY);
                transform.position = newPos;

                // rooms with index 2 and 3 have top openings
                int rand = Random.Range(2, 4);
                GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);

                //spawnedRooms.Add(newRoom);

                // set direction to random number between 1 and 6
                // we don't care what direction it moves in next
                direction = Random.Range(1, 6);
            }
            else
            {
                //print("Stop Generating Rooms");

                // We can stop the level gen here, or we can see if we can go left or right more
                // for now let's just stop level gen
                generationIsStopped = true;

                // clean up box colliders
                //CleanUp();

                // spawn our player
                SpawnPlayer();
            }
        }

        //print(currentRoom.transform.GetChild(0).gameObject);

        //print("Planning to make room at: " + transform.position.x + " " + transform.position.y);

        // check if we are still generating the level
        /*
        if (!generationIsStopped)
        {
            // if we can...

            // spawn new room
            StartCoroutine(DoMove());
            Move();
        }
        */
    }

    void CleanUp()
    {
        foreach (GameObject spawnedRoom in spawnedRooms)
        {
            if (spawnedRoom != null && spawnedRoom.transform.childCount > 0 && spawnedRoom.transform.GetChild(0).GetComponent<BoxCollider2D>() != null)
            {
                spawnedRoom.transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = false;
            }
        }
    }

    void SpawnPlayer()
    {
        player.transform.position = playerSpawnPoint.position;
        player.SetActive(true);
    }

    IEnumerator DoMove()
    {
        yield return new WaitForSeconds(0.2f);
        Move();
    }
}
