using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class LevelGeneration2 : MonoBehaviour
{
    // array of where start of level can be
    public Transform[] startingPositions;

    // array of rooms that can be placed
    public GameObject[] rooms; // index 0 -> LR, index 1 -> LRB, index 2 -> LRT, index 3 -> LRBT

    // array of spawned rooms
    public List<GameObject> criticalPathRooms;

    // array of rooms to branch from
    public List<GameObject> roomsToStartBranchFrom;

    // dictionary of index of room to branch from on the critical path and the current room that the branch should continue from
    public Dictionary<int, GameObject> roomsToContinueBranchFrom;

    // dictionary of room to branch from on the critical path and the current count of the branch size
    public Dictionary<int, int> branchRoomCount;

    public enum RoomContext { START, END, UP_TO_RIGHT_CORNER, DOWN_TO_RIGHT_CORNER, RIGHT_TO_UP_CORNER, RIGHT_TO_DOWN_CORNER, DOWN_STRAIGHT, UP_STRAIGHT, RIGHT_STRAIGHT }

    // dictionary for room and context of room e.g corner, straigh path etc
    //public Dictionary<GameObject, RoomContext> criticalPathDictionary;

    // pick which direction to go next
    // 1 & 2 = right
    // 3 = down
    // 4 = up
    private int direction;
    //private int previousDirection;

    // amount to move on x axis to spawn new room
    public float moveAmountX;

    // amount to move on Y axis to spawn new room
    public float moveAmountY;

    // floats of the bounds of the level generation
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    // bool to see if we should keep generating the level
    public bool generationIsStopped = false;

    // bool to see if critical path is made
    public bool criticalPathFinished = false;

    // bool to see if branching has started
    public bool branchingFinished = false;

    // layermask for overlapsphere function
    public LayerMask roomMask;

    // count how many times we go in a diretion
    public int downCounter;
    public int upCounter;
    public int rightCounter;

    // count how many rooms are created in a branch
    //public int branchRoomCounter;

    public float timeBetweenRoomSpawn;
    public float spawnRoomCooldown;

    // info to spawn player
    public GameObject player;
    public Transform playerSpawnPoint;

    private void Start()
    {
        criticalPathRooms = new List<GameObject>();

        roomsToStartBranchFrom = new List<GameObject>();

        //roomsToContinueBranchFrom = new List<GameObject>();
        roomsToContinueBranchFrom = new Dictionary<int, GameObject>();

        branchRoomCount = new Dictionary<int, int>();

        //criticalPathDictionary = new Dictionary<GameObject, RoomContext>();

        // get a random starting point, set this objects position to it, and spawn our first room
        int randStartingPos = Random.Range(0, startingPositions.Length);
        transform.position = startingPositions[randStartingPos].position;
        GameObject newRoom = Instantiate(rooms[0], transform.position, Quaternion.identity);

        criticalPathRooms.Add(newRoom);
        //criticalPathDictionary[newRoom] = RoomContext.START;

        // set direction to random number between 1 and 2
        // start with going right
        direction = Random.Range(1, 3);

        // spawn new room
        //Move();
    }

    private void Update()
    {
        /*
        if (timeBetweenRoomSpawn <= 0 && !generationIsStopped)
        {
            //previousDirection = direction;
            Move();
            timeBetweenRoomSpawn = spawnRoomCooldown;
        }
        */
        if (timeBetweenRoomSpawn <= 0 && !criticalPathFinished)
        {
            //previousDirection = direction;
            Move();
            timeBetweenRoomSpawn = spawnRoomCooldown;
        }
        else if(timeBetweenRoomSpawn <= 0 && criticalPathFinished && !branchingFinished)
        {
            //int numberOfBranches = Random.Range(5, criticalPathRooms.Count / 4);
            //CreateBranches(numberOfBranches);

            print("CALLING MOVE BRANCH");

            MoveBranch();
            timeBetweenRoomSpawn = spawnRoomCooldown;
        }
        else if(timeBetweenRoomSpawn <= 0 && generationIsStopped)
        {
            //SpawnPlayer();
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

                // we are within the maxX so we can move right
                Vector2 newPos = new Vector2(transform.position.x + moveAmountX, transform.position.y);
                transform.position = newPos;

                // all rooms have openings on right, so we pick on at random
                int rand = Random.Range(0, rooms.Length);
                GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);

                criticalPathRooms.Add(newRoom);

                // check if we are making a corner or continuing right
                /*
                if (downCounter > 0)
                {
                    criticalPathDictionary[newRoom] = RoomContext.DOWN_TO_RIGHT_CORNER;
                }
                else if(upCounter > 0)
                {
                    criticalPathDictionary[newRoom] = RoomContext.UP_TO_RIGHT_CORNER;
                }
                else
                {
                    criticalPathDictionary[newRoom] = RoomContext.RIGHT_STRAIGHT;
                }
                */

                // reset down and up counter
                downCounter = 0;
                upCounter = 0;

                // set direction to random number between 1 and 4
                direction = Random.Range(1, 5);
            }
            else
            {
                //generationIsStopped = true;
                criticalPathFinished = true;

                int numberOfBranches = Random.Range(5, criticalPathRooms.Count / 4);
                print("STARTING BRANCHING");
                CreateBranches(2);

                // spawn our player
                //SpawnPlayer();
            }
        }
        else if(direction == 3)
        {
            // increment down counter
            downCounter++;

            //print(transform.position.y + " : " + minY);
            if (transform.position.y > minY)
            {

                // Get the room we just created before this
                Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomMask);

                // check if the room found has a bottom opening
                if (roomDetection.GetComponent<RoomType>().type != 1 && roomDetection.GetComponent<RoomType>().type != 3)
                {

                    // has our level gen has moved down more than 2 times in a row
                    if (downCounter >= 2)
                    {
                        // if so, we want to spawn a room that has openings on all 4 ways
                        // previously, if we went down twice or more, there's a chance a room with no bottom opening could spawn
                        // resulting in a dead end

                        // we destroy the room that might have not had a bottom opening
                        criticalPathRooms.Remove(roomDetection.gameObject.transform.parent.gameObject);
                        //criticalPathDictionary.Remove(roomDetection.gameObject.transform.parent.gameObject);
                        roomDetection.GetComponent<RoomType>().RoomDestruction();

                        GameObject newRoom_local = Instantiate(rooms[3], transform.position, Quaternion.identity);
                        //GameObject newRoom_local = Instantiate(rooms[3], transform.position, Quaternion.identity);

                        criticalPathRooms.Add(newRoom_local);

                        // check if we are making a corner or continuing down
                        /*
                        if (downCounter > 1)
                        {
                            criticalPathDictionary[newRoom_local] = RoomContext.DOWN_STRAIGHT;
                        }
                        else
                        {
                            criticalPathDictionary[newRoom_local] = RoomContext.DOWN_TO_RIGHT_CORNER;
                        }
                        */
                    }
                    else
                    {
                        // if not, we destroy the last room and spwan a room with all 4 openings or with LRB
                        criticalPathRooms.Remove(roomDetection.gameObject.transform.parent.gameObject);
                        //criticalPathDictionary.Remove(roomDetection.gameObject.transform.parent.gameObject);
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
                        GameObject newRoom_local = Instantiate(rooms[randomBottomRoom], transform.position, Quaternion.identity);

                        criticalPathRooms.Add(newRoom_local);

                        // check if we are making a corner or continuing down
                        /*
                        if (downCounter > 1)
                        {
                            criticalPathDictionary[newRoom_local] = RoomContext.DOWN_STRAIGHT;
                        }
                        else
                        {
                            criticalPathDictionary[newRoom_local] = RoomContext.DOWN_TO_RIGHT_CORNER;
                        }
                        */
                    }
                }

                // We are within the minY so we can move down
                Vector2 newPos = new Vector2(transform.position.x, transform.position.y - moveAmountY);
                transform.position = newPos;

                // rooms with index 2 and 3 have top openings
                int rand = Random.Range(2, 4);
                GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);

                criticalPathRooms.Add(newRoom);

                // check if we are making a corner or continuing down
                /*
                if (downCounter > 1)
                {
                    criticalPathDictionary[newRoom] = RoomContext.DOWN_STRAIGHT;
                }
                else
                {
                    criticalPathDictionary[newRoom] = RoomContext.DOWN_TO_RIGHT_CORNER;
                }
                */

                // set direction to random number between 1 and 4
                // we don't care what direction it moves in next
                direction = Random.Range(1, 5);

                // we can't go up (no overwriting), so go right instead
                if (direction == 4)
                {
                    direction = Random.Range(1, 3);
                }
            }
            else
            {
                // set direction to random number between 1 and 2 to go right
                direction = Random.Range(1, 3);
            }
        }
        else if(direction == 4)
        {

            upCounter++;

            // can we go up?
            if (transform.position.y < maxY)
            {

                // Get the room we just created before this
                Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomMask);

                // check if the room found has a top opening
                if (roomDetection.GetComponent<RoomType>().type != 2 && roomDetection.GetComponent<RoomType>().type != 3)
                {
                    // has our level gen has moved up more than 2 times in a row
                    if (upCounter >= 2)
                    {
                        // if so, we want to spawn a room that has openings on all 4 ways
                        // previously, if we went down twice or more, there's a chance a room with no bottom opening could spawn
                        // resulting in a dead end

                        // we destroy the room that might have not had a bottom opening
                        criticalPathRooms.Remove(roomDetection.gameObject.transform.parent.gameObject);
                        //criticalPathDictionary.Remove(roomDetection.gameObject.transform.parent.gameObject);
                        roomDetection.GetComponent<RoomType>().RoomDestruction();

                        GameObject newRoom_local = Instantiate(rooms[3], transform.position, Quaternion.identity);
                        criticalPathRooms.Add(newRoom_local);

                        // check if we are making a corner or continuing up
                        /*
                        if (upCounter > 1)
                        {
                            criticalPathDictionary[newRoom_local] = RoomContext.UP_STRAIGHT;
                        }
                        else
                        {
                            criticalPathDictionary[newRoom_local] = RoomContext.UP_TO_RIGHT_CORNER;
                        }
                        */
                    }
                    else
                    {
                        // if not, we destroy the last room and spwan a room with all 4 openings or with LRT
                        criticalPathRooms.Remove(roomDetection.gameObject.transform.parent.gameObject);
                        //criticalPathDictionary.Remove(roomDetection.gameObject.transform.parent.gameObject);
                        roomDetection.GetComponent<RoomType>().RoomDestruction();

                        // we want to create a room that has a bottom opening
                        // we want an index of 2 or 3
                        int randomBottomRoom = Random.Range(2, 4);

                        GameObject newRoom_local = Instantiate(rooms[randomBottomRoom], transform.position, Quaternion.identity);

                        criticalPathRooms.Add(newRoom_local);

                        // check if we are making a corner or continuing up
                        /*
                        if (upCounter > 1)
                        {
                            criticalPathDictionary[newRoom_local] = RoomContext.UP_STRAIGHT;
                        }
                        else
                        {
                            criticalPathDictionary[newRoom_local] = RoomContext.UP_TO_RIGHT_CORNER;
                        }
                        */
                    }
                }

                // We are within the minY so we can move down
                Vector2 newPos = new Vector2(transform.position.x, transform.position.y + moveAmountY);
                transform.position = newPos;

                // rooms with index 1 and 3 have bottom openings
                int rand = Random.Range(1, 4);

                if(rand == 2)
                {
                    rand = 1;
                }

                GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);

                criticalPathRooms.Add(newRoom);

                // check if we are making a corner or continuing up
                /*
                if (upCounter > 1)
                {
                    criticalPathDictionary[newRoom] = RoomContext.UP_STRAIGHT;
                }
                else
                {
                    criticalPathDictionary[newRoom] = RoomContext.UP_TO_RIGHT_CORNER;
                }
                */

                // set direction to random number between 1 and 4
                // we don't care what direction it moves in next
                direction = Random.Range(1, 5);

                if (direction == 3)
                {
                    direction = 1;
                }
            }
            // we can't, so let's go right
            else
            {
                direction = Random.Range(1, 3);
            }
        }

        /*
        else if (direction == 6)
        {
            // can we go up?
            if (transform.position.y < maxY)
            {

                // We are within the minY so we can move down
                Vector2 newPos = new Vector2(transform.position.x, transform.position.y + moveAmountY);
                transform.position = newPos;

                // rooms with index 2 and 3 have top openings
                int rand = Random.Range(2, 4);
                GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);

                //spawnedRooms.Add(newRoom);

                // set direction to random number between 1 and 6
                // we don't care what direction it moves in next
                //direction = Random.Range(1, 6);
                direction = Random.Range(1, 7);

                if (direction == 5)
                {
                    direction = 1;
                }

                print("SPAWNED UP: " + direction);
            }
            // we can't, so let's go right
            else
            {
                direction = Random.Range(1, 3);
            }

            //}
            //else
            //{
            //print("Stop Generating Rooms");

            // We can stop the level gen here, or we can see if we can go left or right more
            // for now let's just stop level gen
            //generationIsStopped = true;

            // clean up box colliders
            //CleanUp();

            // spawn our player
            //SpawnPlayer();
            //}
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

    void CreateBranches(int numberOfBranches)
    {
        for (int i = 0; i < numberOfBranches; i++)
        {
            int roomIndexToBranchFrom = Random.Range(1, criticalPathRooms.Count - 1);
            GameObject roomToBranchFrom = criticalPathRooms[roomIndexToBranchFrom];

            print("ADDING : " + roomIndexToBranchFrom + " TO BRANCH");

            if (!roomsToStartBranchFrom.Contains(roomToBranchFrom))
            {
                roomsToStartBranchFrom.Add(roomToBranchFrom);
            }
            else
            {
                print("TRIED ADDING DUPLICATE ROOMS");
            }
        }
    }

        /*
        for(int i = 0; i < numberOfBranches; i++)
        {

            int roomLimitForBranch = 3;

            for (int j = 0; j <= roomLimitForBranch; j++)
            {
                int currentRoomCount = 0;

                int roomIndexToBranchFrom = Random.Range(1, criticalPathRooms.Count - 1);
                GameObject roomToBranchFrom = criticalPathRooms[roomIndexToBranchFrom];

                transform.position = roomToBranchFrom.transform.position;

                int branchDirection = direction = Random.Range(1, 5);

                print("BRANCHING FROM: " + roomIndexToBranchFrom + " AND GOING: " + branchDirection);

                if (branchDirection == 1 || branchDirection == 2)
                {
                    // Get if there is already a room in this direction
                    Vector3 positionToRight = new Vector3(transform.position.x + moveAmountX, transform.position.y, transform.position.z);
                    Collider2D roomDetection = Physics2D.OverlapCircle(positionToRight, 1, roomMask);

                    // is there a room to the right?
                    if (roomDetection != null)
                    {
                        // no, so we can go this way

                        currentRoomCount++;

                        // are we at the limit of rooms for this branch?
                        if (currentRoomCount < roomLimitForBranch)
                        {
                            Vector3 newPos = positionToRight;
                            transform.position = newPos;

                            int rand = Random.Range(1, 4);
                            GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);
                        }
                        else if (currentRoomCount == roomLimitForBranch)
                        {
                            // spawn dead end
                        }
                        else
                        {
                            // finished with this branch
                            continue;
                        }
                    }
                    else
                    {
                        // yes, so stop for now
                        print("CANT GO THIS WAY");
                        continue;
                    }
                }
                else if (branchDirection == 3)
                {
                    // Get if there is already a room in this direction
                    Vector3 positionToDown = new Vector3(transform.position.x, transform.position.y - moveAmountY, transform.position.z);
                    Collider2D roomDetection = Physics2D.OverlapCircle(positionToDown, 1, roomMask);

                    // is there a room below?
                    if (roomDetection != null)
                    {
                        // no, so we can go this way

                        currentRoomCount++;

                        // are we at the limit of rooms for this branch?
                        if (currentRoomCount < roomLimitForBranch)
                        {
                            Vector3 newPos = positionToDown;
                            transform.position = newPos;

                            int rand = Random.Range(1, 4);
                            GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);
                        }
                        else if (currentRoomCount == roomLimitForBranch)
                        {
                            // spawn dead end
                        }
                        else
                        {
                            // finished with this branch
                            print("CANT GO THIS WAY");
                            continue;
                        }
                    }
                    else
                    {
                        // yes, so stop for now
                        continue;
                    }
                }
                else if (branchDirection == 4)
                {
                    // Get if there is already a room in this direction
                    Vector3 positionToUp = new Vector3(transform.position.x, transform.position.y + moveAmountY, transform.position.z);
                    Collider2D roomDetection = Physics2D.OverlapCircle(positionToUp, 1, roomMask);

                    // is there a room above?
                    if (roomDetection != null)
                    {
                        // no, so we can go this way

                        currentRoomCount++;

                        // are we at the limit of rooms for this branch?
                        if (currentRoomCount < roomLimitForBranch)
                        {
                            Vector3 newPos = positionToUp;
                            transform.position = newPos;

                            int rand = Random.Range(1, 4);
                            GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);
                        }
                        else if (currentRoomCount == roomLimitForBranch)
                        {
                            // spawn dead end
                        }
                        else
                        {
                            // finished with this branch
                            print("CANT GO THIS WAY");
                            continue;
                        }
                    }
                    else
                    {
                        // yes, so stop for now
                        continue;
                    }
                }
            }
        }

        // finished with branching
        generationIsStopped = true;
        SpawnPlayer();
    }
        */

    void MoveBranch()
    {

        int roomLimitForBranch = 5;

        int branchRoomCounter;

        for (int i = 0; i < roomsToStartBranchFrom.Count; i++)
        {
            //int roomIndexToBranchFrom = Random.Range(1, criticalPathRooms.Count - 1);
            GameObject roomToBranchFrom;
            if (roomsToContinueBranchFrom.ContainsKey(i))
            {
                roomToBranchFrom = roomsToContinueBranchFrom[i];
                branchRoomCounter = branchRoomCount[i];
            }
            else
            {
                roomToBranchFrom = roomsToStartBranchFrom[i];
                branchRoomCount[i] = 0;
                branchRoomCounter = 0;
            }

            transform.position = roomToBranchFrom.transform.position;

            int branchDirection = direction = Random.Range(1, 5);
            //int branchDirection = 1;

            print("BRANCHING FROM: " + i + " AND GOING: " + branchDirection);

            if (branchDirection == 1 || branchDirection == 2)
            {
                // Get if there is already a room in this direction
                Vector3 positionToRight = new Vector3(transform.position.x + moveAmountX, transform.position.y, transform.position.z);
                print("CHECKING POSITION: " + positionToRight);
                Collider2D roomDetection = Physics2D.OverlapCircle(positionToRight, 1, roomMask);
                print("FOUND: " + roomDetection);

                // is there a room to the right?
                if (roomDetection == null)
                {
                    // no, so we can go this way

                    branchRoomCounter++;

                    branchRoomCount[i] += 1;

                    // are we at the limit of rooms for this branch?
                    if (branchRoomCounter < roomLimitForBranch)
                    {
                        Vector3 newPos = positionToRight;
                        transform.position = newPos;

                        print("CREATING ROOM AT : " + transform.position);

                        int rand = Random.Range(1, 4);
                        GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);

                        //roomsToStartBranchFrom[i] = newRoom;
                        roomsToContinueBranchFrom[i] = newRoom;

                        //roomsToContinueBranchFrom.Add(newRoom);

                        print("CREATED ROOM BY GOING: " + branchDirection);
                    }
                    else if (branchRoomCounter == roomLimitForBranch)
                    {
                        // spawn dead end
                    }
                    else
                    {
                        // finished with this branch
                        print("REACHED LIMIT");
                        continue;
                    }
                }
                else
                {
                    // yes, so stop for now
                    print("CANT GO THIS WAY");
                    continue;
                }
            }
            else if (branchDirection == 3)
            {
                // Get if there is already a room in this direction
                Vector3 positionToDown = new Vector3(transform.position.x, transform.position.y - moveAmountY, transform.position.z);
                Collider2D roomDetection = Physics2D.OverlapCircle(positionToDown, 1, roomMask);

                // is there a room below?
                if (roomDetection == null)
                {
                    // no, so we can go this way

                    branchRoomCounter++;

                    branchRoomCount[i] += 1;

                    // are we at the limit of rooms for this branch?
                    if (branchRoomCounter < roomLimitForBranch)
                    {
                        Vector3 newPos = positionToDown;
                        transform.position = newPos;

                        int rand = Random.Range(1, 4);
                        GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);

                        //roomsToStartBranchFrom[i] = newRoom;
                        roomsToContinueBranchFrom[i] = newRoom;

                        print("CREATED ROOM BY GOING: " + branchDirection);
                    }
                    else if (branchRoomCounter == roomLimitForBranch)
                    {
                        // spawn dead end
                    }
                    else
                    {
                        // finished with this branch
                        print("REACHED LIMIT");
                        continue;
                    }
                }
                else
                {
                    // yes, so stop for now
                    print("CANT GO THIS WAY");
                    continue;
                }
            }
            else if (branchDirection == 4)
            {
                // Get if there is already a room in this direction
                Vector3 positionToUp = new Vector3(transform.position.x, transform.position.y + moveAmountY, transform.position.z);
                Collider2D roomDetection = Physics2D.OverlapCircle(positionToUp, 1, roomMask);

                // is there a room above?
                if (roomDetection == null)
                {
                    // no, so we can go this way

                    branchRoomCounter++;

                    branchRoomCount[i] += 1;

                    // are we at the limit of rooms for this branch?
                    if (branchRoomCounter < roomLimitForBranch)
                    {
                        Vector3 newPos = positionToUp;
                        transform.position = newPos;

                        int rand = Random.Range(1, 4);
                        GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);

                        //roomsToStartBranchFrom[i] = newRoom;
                        roomsToContinueBranchFrom[i] = newRoom;

                        print("CREATED ROOM BY GOING: " + branchDirection);
                    }
                    else if (branchRoomCounter == roomLimitForBranch)
                    {
                        // spawn dead end
                    }
                    else
                    {
                        // finished with this branch
                        print("REACHED LIMIT");
                        continue;
                    }
                }
                else
                {
                    // yes, so stop for now
                    print("CANT GO THIS WAY");
                    continue;
                }
            }
        }
    }

    void CleanUp()
    {
        foreach (GameObject spawnedRoom in criticalPathRooms)
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
