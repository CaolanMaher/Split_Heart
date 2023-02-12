using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class LevelGeneration2 : MonoBehaviour
{
    // array of where start of level can be
    public Transform[] startingPositions;

    //public enum RoomOpeningType { LR = 0, LRB, LRT, LRBT, LB, LT, RB, RT, L, B, R, T }

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

    // dictionary of room to branch from on the critical path and the current count of times it has gone down
    public Dictionary<int, int> branchRoomDownCount;

    // dictionary of room to branch from on the critical path and the current count of times it has gone up
    public Dictionary<int, int> branchRoomUpCount;

    // dictionary of room to branch from on the critical path and the amount of attempts made to make a room
    public Dictionary<int, int> branchRoomAttemptCount;

    //public enum RoomContext { START, END, UP_TO_RIGHT_CORNER, DOWN_TO_RIGHT_CORNER, RIGHT_TO_UP_CORNER, RIGHT_TO_DOWN_CORNER, DOWN_STRAIGHT, UP_STRAIGHT, RIGHT_STRAIGHT }

    // dictionary for room and context of room e.g corner, straigh path etc
    //public Dictionary<GameObject, RoomContext> criticalPathDictionary;

    // pick which direction to go next
    // 1 & 2 = right
    // 3 = down
    // 4 = up
    private int direction;
    private int branchDirection;
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

    int currentBranchToMoveIndex = 0;

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

        branchRoomDownCount = new Dictionary<int, int>();

        branchRoomUpCount = new Dictionary<int, int>();

        branchRoomAttemptCount = new Dictionary<int, int>();

        //criticalPathDictionary = new Dictionary<GameObject, RoomContext>();

        // get a random starting point, set this objects position to it, and spawn our first room
        int randStartingPos = Random.Range(0, startingPositions.Length);
        transform.position = startingPositions[randStartingPos].position;

        //print("SPAWNING ROOM AT : " + transform.position);

        // spawn a room with just a left opening first
        GameObject newRoom = Instantiate(rooms[9], transform.position, Quaternion.identity);

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

            //print("CALLING MOVE BRANCH");

            MoveBranch();
            timeBetweenRoomSpawn = spawnRoomCooldown;
        }
        //else if(timeBetweenRoomSpawn <= 0 && generationIsStopped)
        //{
            //SpawnPlayer();
        //}
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
            if(transform.position.x <= maxX)
            {

                // Get the room we just created before this
                Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomMask);

                // check if the room found has a right opening
                if (roomDetection.GetComponent<RoomType>().type != 0 
                    && roomDetection.GetComponent<RoomType>().type != 1 
                    && roomDetection.GetComponent<RoomType>().type != 2
                    && roomDetection.GetComponent<RoomType>().type != 3
                    && roomDetection.GetComponent<RoomType>().type != 6
                    && roomDetection.GetComponent<RoomType>().type != 7
                    && roomDetection.GetComponent<RoomType>().type != 11)
                {
                    // if not, get the current rooms type, destroy the room and create one with a right opening and whatever openings it currently has

                    int roomDetectionType = roomDetection.GetComponent<RoomType>().type;

                    criticalPathRooms.Remove(roomDetection.gameObject.transform.parent.gameObject);
                    //criticalPathDictionary.Remove(roomDetection.gameObject.transform.parent.gameObject);
                    roomDetection.GetComponent<RoomType>().RoomDestruction();

                    //print("GOING RIGHT, LOOKING TO REPLACE ROOM : " + roomDetectionType);

                    // check which room type should replace this room
                    // it can be 9, 10, 12
                    // if previous room was a left opening room
                    if(roomDetectionType == 9)
                    {
                        //print("REPLACING CURRENT ROOM WITH : " + 0 + " AT : " + transform.position);
                        // need to spawn a room with left and right openings
                        GameObject newRoom_local = Instantiate(rooms[0], transform.position, Quaternion.identity);
                        criticalPathRooms.Add(newRoom_local);
                    }
                    // if it was a bottom opening room
                    else if(roomDetectionType == 10)
                    {
                        //print("REPLACING CURRENT ROOM WITH : " + 6 + " AT : " + transform.position);
                        // need to spawn a room with bottom and right openings
                        GameObject newRoom_local = Instantiate(rooms[6], transform.position, Quaternion.identity);
                        criticalPathRooms.Add(newRoom_local);
                    }
                    // if it was a top opening room
                    else if (roomDetectionType == 12)
                    {
                        //print("REPLACING CURRENT ROOM WITH : " + 7 + " AT : " + transform.position);
                        // need to spawn a room with top and right openings
                        GameObject newRoom_local = Instantiate(rooms[7], transform.position, Quaternion.identity);
                        criticalPathRooms.Add(newRoom_local);
                    }
                }

                // we are within the maxX so we can move right
                Vector2 newPos = new Vector2(transform.position.x + moveAmountX, transform.position.y);
                transform.position = newPos;

                // all rooms have openings on right, so we pick one at random
                //int rand = Random.Range(0, rooms.Length);
                //GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);

                // Spawn a room with a left opening
                GameObject newRoom = Instantiate(rooms[9], transform.position, Quaternion.identity);

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

                if(transform.position.x == maxX)
                {
                    //generationIsStopped = true;
                    criticalPathFinished = true;

                    int numberOfBranches = Random.Range(5, criticalPathRooms.Count / 4);

                    //int numberOfBranches = 2;

                    print("STARTING BRANCHING");

                    CreateBranches(numberOfBranches);
                    
                }
            }
            /*
            else
            {
                //generationIsStopped = true;
                //criticalPathFinished = true;

                //int numberOfBranches = Random.Range(5, criticalPathRooms.Count / 4);

                //print("STARTING BRANCHING");

                //CreateBranches(numberOfBranches);

                // spawn our player
                //SpawnPlayer();
            }
            */
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
                if (roomDetection.GetComponent<RoomType>().type != 1 
                    && roomDetection.GetComponent<RoomType>().type != 3
                    && roomDetection.GetComponent<RoomType>().type != 4
                    && roomDetection.GetComponent<RoomType>().type != 6
                    && roomDetection.GetComponent<RoomType>().type != 8
                    && roomDetection.GetComponent<RoomType>().type != 10)
                {

                    // has our level gen has moved down more than 2 times in a row
                    if (downCounter >= 2)
                    {
                        // if so, we want to spawn a room that has openings on all 4 ways
                        // previously, if we went down twice or more, there's a chance a room with no bottom opening could spawn
                        // resulting in a dead end

                        //int roomDetectionType = roomDetection.GetComponent<RoomType>().type;

                        // we destroy the room that might have not had a bottom opening
                        criticalPathRooms.Remove(roomDetection.gameObject.transform.parent.gameObject);
                        //criticalPathDictionary.Remove(roomDetection.gameObject.transform.parent.gameObject);
                        roomDetection.GetComponent<RoomType>().RoomDestruction();

                        // since this is the second time going down, the only room type that can be spawned here is the top and bottom opening room
                        GameObject newRoom_local = Instantiate(rooms[8], transform.position, Quaternion.identity);

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
                        int roomDetectionType = roomDetection.GetComponent<RoomType>().type;

                        //print("GOING DOWN, LOOKING TO REPLACE ROOM : " + roomDetectionType);

                        // if not, we destroy the last room and spwan a room with all 4 openings or with LRB
                        criticalPathRooms.Remove(roomDetection.gameObject.transform.parent.gameObject);
                        //criticalPathDictionary.Remove(roomDetection.gameObject.transform.parent.gameObject);
                        roomDetection.GetComponent<RoomType>().RoomDestruction();

                        // check which room type should replace this room
                        // it can be 9, 11, 12
                        // if previous room was a left opening room
                        if (roomDetectionType == 9)
                        {
                            // need to spawn a room with left and bottom openings
                            GameObject newRoom_local = Instantiate(rooms[4], transform.position, Quaternion.identity);
                            criticalPathRooms.Add(newRoom_local);
                        }
                        // if it was a bottom opening room
                        else if (roomDetectionType == 11)
                        {
                            // need to spawn a room with bottom and right openings
                            GameObject newRoom_local = Instantiate(rooms[6], transform.position, Quaternion.identity);
                            criticalPathRooms.Add(newRoom_local);
                        }
                        // if it was a top opening room
                        else if (roomDetectionType == 12)
                        {
                            // need to spawn a room with top and right openings
                            GameObject newRoom_local = Instantiate(rooms[8], transform.position, Quaternion.identity);
                            criticalPathRooms.Add(newRoom_local);
                        }

                        // we want to create a room that has a bottom opening
                        // we want an index of 1 or 3
                        //int randomBottomRoom = Random.Range(1, 4);
                        // if we get 2
                        //if (randomBottomRoom == 2)
                        //{
                        // make it into 1
                        //randomBottomRoom = 1;
                        //}
                        //GameObject newRoom_local = Instantiate(rooms[randomBottomRoom], transform.position, Quaternion.identity);

                        //criticalPathRooms.Add(newRoom_local);

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
                //int rand = Random.Range(2, 4);
                //GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);

                // only spawn a room with a top opening
                GameObject newRoom = Instantiate(rooms[12], transform.position, Quaternion.identity);

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
                if (roomDetection.GetComponent<RoomType>().type != 2 
                    && roomDetection.GetComponent<RoomType>().type != 3
                    && roomDetection.GetComponent<RoomType>().type != 5
                    && roomDetection.GetComponent<RoomType>().type != 7
                    && roomDetection.GetComponent<RoomType>().type != 8
                    && roomDetection.GetComponent<RoomType>().type != 12)
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

                        // since this is the second time going up, the only room type that can be spawned here is the top and bottom opening room
                        GameObject newRoom_local = Instantiate(rooms[8], transform.position, Quaternion.identity);

                        //GameObject newRoom_local = Instantiate(rooms[3], transform.position, Quaternion.identity);
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
                        int roomDetectionType = roomDetection.GetComponent<RoomType>().type;

                        //print("GOING UP, LOOKING TO REPLACE ROOM : " + roomDetectionType);

                        // if not, we destroy the last room and spwan a room with all 4 openings or with LRT
                        criticalPathRooms.Remove(roomDetection.gameObject.transform.parent.gameObject);
                        //criticalPathDictionary.Remove(roomDetection.gameObject.transform.parent.gameObject);
                        roomDetection.GetComponent<RoomType>().RoomDestruction();

                        // check which room type should replace this room
                        // it can be 9, 10, 11
                        // if previous room was a left opening room
                        if (roomDetectionType == 9)
                        {
                            // need to spawn a room with left and bottom openings
                            GameObject newRoom_local = Instantiate(rooms[5], transform.position, Quaternion.identity);
                            criticalPathRooms.Add(newRoom_local);
                        }
                        // if it was a bottom opening room
                        else if (roomDetectionType == 10)
                        {
                            // need to spawn a room with bottom and right openings
                            GameObject newRoom_local = Instantiate(rooms[8], transform.position, Quaternion.identity);
                            criticalPathRooms.Add(newRoom_local);
                        }
                        // if it was a top opening room
                        else if (roomDetectionType == 11)
                        {
                            // need to spawn a room with top and right openings
                            GameObject newRoom_local = Instantiate(rooms[7], transform.position, Quaternion.identity);
                            criticalPathRooms.Add(newRoom_local);
                        }

                        // we want to create a room that has a bottom opening
                        // we want an index of 2 or 3
                        //int randomBottomRoom = Random.Range(2, 4);

                        //GameObject newRoom_local = Instantiate(rooms[randomBottomRoom], transform.position, Quaternion.identity);

                        //criticalPathRooms.Add(newRoom_local);

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
                //int rand = Random.Range(1, 4);

                //if(rand == 2)
                //{
                //    rand = 1;
                //}

                //GameObject newRoom = Instantiate(rooms[rand], transform.position, Quaternion.identity);

                // only spawn a room with a bottom opening
                GameObject newRoom = Instantiate(rooms[10], transform.position, Quaternion.identity);

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

                //print("JUST WENT UP, LOOKING TO GO " + direction);
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

            //print("ADDING : " + roomIndexToBranchFrom + " TO BRANCH");

            if (!roomsToStartBranchFrom.Contains(roomToBranchFrom))
            {
                roomsToStartBranchFrom.Add(roomToBranchFrom);
            }
            else
            {
                //print("TRIED ADDING DUPLICATE ROOMS");
            }
        }
    }

    void MoveBranch()
    {

        int roomLimitForBranch = 5;

        int branchAttemptLimit = 5;

        currentBranchToMoveIndex++;

        if (currentBranchToMoveIndex >= roomsToStartBranchFrom.Count)
        {
            //print("MOVING ONTO NEXT BRANCH");
            currentBranchToMoveIndex = 0;
        }

        //for (int i = 0; i < roomsToStartBranchFrom.Count; i++)
        //{
        GameObject roomToBranchFrom;
            if (roomsToContinueBranchFrom.ContainsKey(currentBranchToMoveIndex))
            {
                roomToBranchFrom = roomsToContinueBranchFrom[currentBranchToMoveIndex];
            }
            else
            {
                print("SETTING UP " + currentBranchToMoveIndex);
                roomToBranchFrom = roomsToStartBranchFrom[currentBranchToMoveIndex];
                roomsToContinueBranchFrom[currentBranchToMoveIndex] = roomToBranchFrom;
                branchRoomCount[currentBranchToMoveIndex] = 0;
                branchRoomDownCount[currentBranchToMoveIndex] = 0;
                branchRoomUpCount[currentBranchToMoveIndex] = 0;
                branchRoomAttemptCount[currentBranchToMoveIndex] = 0;
            }

        for (int i = 0; i < roomsToStartBranchFrom.Count; i++)
        {
            if (roomsToContinueBranchFrom.ContainsKey(i))
            {
                if (branchRoomAttemptCount[i] >= branchAttemptLimit)
                {
                    print("THIS BRANCH " + i + " HAS NO MORE ATTEMPTS");
                    branchRoomCount[i] = roomLimitForBranch;

                    //currentBranchToMoveIndex++;
                    //return;
                    //continue;
                }
            }
        }

        bool allLimitsMet = true;

        foreach (KeyValuePair<int, int> pair in branchRoomCount)
        {
            print("CHECKING " + pair.Key + " ROOM LIMIT " + pair.Value);
            if (!(pair.Value >= roomLimitForBranch))
            {
                allLimitsMet = false;
            }
        }

        if (allLimitsMet)
        {
            branchingFinished = true;
            print("CALLING SPAWN PLAYER");
            SpawnPlayer();
            return;
        }

        transform.position = roomToBranchFrom.transform.position;

            // 1 & 2 = right
            // 3 & 4 = left
            // 5 = down
            // 6 = up
            branchDirection = Random.Range(1, 7);

        //print("BRANCHING FROM: " + i + " AND GOING: " + branchDirection);

        if (branchDirection == 1 || branchDirection == 2)
        {
            // Get if there is already a room in this direction
            Vector2 positionToRight = new Vector2(transform.position.x + moveAmountX, transform.position.y);
            //print("CHECKING POSITION: " + positionToRight);
            Collider2D roomDetectionToRight = Physics2D.OverlapCircle(positionToRight, 1, roomMask);
            //print("FOUND: " + roomDetection);

            // is there a room to the right?
            if (roomDetectionToRight == null)
            {
                // no, so we can go this way

                // are we at the limit of rooms for this branch?
                if (branchRoomCount[currentBranchToMoveIndex] < roomLimitForBranch)
                {
                    //branchRoomCounter++;

                    Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomMask);

                    int roomDetectionType = roomDetection.GetComponent<RoomType>().type;

                    // check if this room already has an opening in the direction we are trying to go
                    if (roomDetectionType == 0
                        || roomDetectionType == 1
                        || roomDetectionType == 2
                        || roomDetectionType == 3
                        || roomDetectionType == 6
                        || roomDetectionType == 7
                        || roomDetectionType == 11
                        || roomDetectionType == 14)
                    {
                        // don't risk breaking a path by trying to go this way (rare occurance)
                        //continue;
                        return;
                    }

                    print("GOING RIGHT AND REPLACING ROOM " + roomDetectionType);

                    roomDetection.GetComponent<RoomType>().RoomDestruction();

                    // replace this room with the necessary room
                    // it couldn't have been a room with a right opening so we can remove those from our checks
                    // it can be 4, 5, 8, 9, 10, 12

                    GameObject replacementRoom;

                    // if it had a left and bottom opening
                    if (roomDetectionType == 4)
                    {
                        // replace it with a left, bottom, and right opening
                        replacementRoom = Instantiate(rooms[1], transform.position, Quaternion.identity);
                    }
                    // if it had a left and top opening
                    else if (roomDetectionType == 5)
                    {
                        // replace it with a left, top, and right opening
                        replacementRoom = Instantiate(rooms[2], transform.position, Quaternion.identity);
                    }
                    // if it had a bottom and top opening
                    else if (roomDetectionType == 8)
                    {
                        // replace it with a bottom, top, and right opening
                        replacementRoom = Instantiate(rooms[14], transform.position, Quaternion.identity);
                    }
                    // if it had a left opening
                    else if (roomDetectionType == 9)
                    {
                        // replace it with a left and right opening
                        replacementRoom = Instantiate(rooms[0], transform.position, Quaternion.identity);
                    }
                    // if it had a bottom opening
                    else if (roomDetectionType == 10)
                    {
                        // replace it with a bottom and right opening
                        replacementRoom = Instantiate(rooms[6], transform.position, Quaternion.identity);
                    }
                    // if it had a top opening
                    else if (roomDetectionType == 12)
                    {
                        // replace it with a top and right opening
                        replacementRoom = Instantiate(rooms[7], transform.position, Quaternion.identity);
                    }

                    branchRoomCount[currentBranchToMoveIndex] += 1;

                    // reset up and down counters
                    branchRoomDownCount[currentBranchToMoveIndex] = 0;
                    branchRoomUpCount[currentBranchToMoveIndex] = 0;

                    Vector2 newPos = positionToRight;
                    transform.position = newPos;

                    //print("CREATING ROOM AT : " + transform.position);

                    //int rand = Random.Range(1, 4);

                    // always make the new room a room with just a left opening
                    GameObject newRoom = Instantiate(rooms[9], transform.position, Quaternion.identity);

                    roomsToContinueBranchFrom[currentBranchToMoveIndex] = newRoom;
                }
                else if (branchRoomCount[currentBranchToMoveIndex] == roomLimitForBranch)
                {
                    // spawn dead end
                }
                else
                {
                    // finished with this branch
                    //print("REACHED LIMIT");
                    //continue;
                    return;
                }
            }
            else
            {
                // yes, so stop for now
                //print("CANT GO THIS WAY");
                //branchRoomCount[i] = roomLimitForBranch;
                //branchAttemptCounter++;
                print("ADDING TO ATTEMPT : " + currentBranchToMoveIndex);
                branchRoomAttemptCount[currentBranchToMoveIndex] += 1;
                return;
                //continue;
            }
        }
        else if (branchDirection == 3 || branchDirection == 4)
        {
            // Get if there is already a room in this direction
            Vector2 positionToLeft = new Vector2(transform.position.x - moveAmountX, transform.position.y);
            //print("CHECKING POSITION: " + positionToLeft);
            Collider2D roomDetectionToLeft = Physics2D.OverlapCircle(positionToLeft, 1, roomMask);
            //print("FOUND: " + roomDetection);

            // is there a room to the left?
            if (roomDetectionToLeft == null)
            {
                // no, so we can go this way

                //branchRoomCounter++;

                //branchRoomCount[i] += 1;

                // are we at the limit of rooms for this branch?
                if (branchRoomCount[currentBranchToMoveIndex] < roomLimitForBranch)
                {
                    // get the room we created just before this
                    Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomMask);

                    int roomDetectionType = roomDetection.GetComponent<RoomType>().type;

                    // check if this room already has an opening in the direction we are trying to go
                    if (roomDetectionType == 0
                        || roomDetectionType == 1
                        || roomDetectionType == 2
                        || roomDetectionType == 3
                        || roomDetectionType == 4
                        || roomDetectionType == 5
                        || roomDetectionType == 9
                        || roomDetectionType == 13)
                    {
                        // don't risk breaking a path by trying to go this way (rare occurance)
                        //continue;
                        return;
                    }

                    print("GOING LEFT AND REPLACING ROOM " + roomDetectionType);

                    roomDetection.GetComponent<RoomType>().RoomDestruction();

                    // replace this room with the necessary room
                    // it couldn't have been a room with a left opening so we can remove those from our checks
                    // it can be 6, 7, 8, 10, 11, 12

                    GameObject replacementRoom;

                    // if it had a right and bottom opening
                    if (roomDetectionType == 6)
                    {
                        // replace it with a bottom, right, and left opening
                        replacementRoom = Instantiate(rooms[1], transform.position, Quaternion.identity);
                    }
                    // if it had a right and top opening
                    else if (roomDetectionType == 7)
                    {
                        // replace it with a top, right, and left opening
                        replacementRoom = Instantiate(rooms[2], transform.position, Quaternion.identity);
                    }
                    // if it had a bottom and top opening
                    else if (roomDetectionType == 8)
                    {
                        // replace it with a bottom, top, and left opening
                        replacementRoom = Instantiate(rooms[13], transform.position, Quaternion.identity);
                    }
                    // if it had a bottom opening
                    else if (roomDetectionType == 10)
                    {
                        // replace it with a bottom and left opening
                        replacementRoom = Instantiate(rooms[4], transform.position, Quaternion.identity);
                    }
                    // if it had a right opening
                    else if (roomDetectionType == 11)
                    {
                        // replace it with a right an left opening
                        replacementRoom = Instantiate(rooms[0], transform.position, Quaternion.identity);
                    }
                    // if it had a top opening
                    else if (roomDetectionType == 12)
                    {
                        // replace it with a top and left opening
                        replacementRoom = Instantiate(rooms[5], transform.position, Quaternion.identity);
                    }

                    branchRoomCount[currentBranchToMoveIndex] += 1;

                    // reset up and down counters
                    branchRoomDownCount[currentBranchToMoveIndex] = 0;
                    branchRoomUpCount[currentBranchToMoveIndex] = 0;

                    Vector2 newPos = positionToLeft;
                    transform.position = newPos;

                    //print("CREATING ROOM AT : " + transform.position);

                    //int rand = Random.Range(1, 4);

                    // always spawn a room with just a right opening
                    GameObject newRoom = Instantiate(rooms[11], transform.position, Quaternion.identity);

                    roomsToContinueBranchFrom[currentBranchToMoveIndex] = newRoom;
                }
                else if (branchRoomCount[currentBranchToMoveIndex] == roomLimitForBranch)
                {
                    // spawn dead end
                }
                else
                {
                    // finished with this branch
                    //print("REACHED LIMIT");
                    //continue;
                    return;
                }
            }
            else
            {
                // yes, so stop for now
                //print("CANT GO THIS WAY");
                //branchAttemptCounter++;
                print("ADDING TO ATTEMPT : " + currentBranchToMoveIndex);
                branchRoomAttemptCount[currentBranchToMoveIndex] += 1;
                //continue;
                return;
            }
        }
        else if (branchDirection == 5)
        {

            if (branchRoomDownCount[currentBranchToMoveIndex] >= 1)
            {
                // don't want branch to go down multiple times
                //print("REACHED DOWN LIMIT");
                print("ADDING TO ATTEMPT : " + currentBranchToMoveIndex);
                branchRoomAttemptCount[currentBranchToMoveIndex] += 1;
                //continue;
                return;
            }

            // Get if there is already a room in this direction
            Vector2 positionToDown = new Vector2(transform.position.x, transform.position.y - moveAmountY);
            Collider2D roomDetectionToDown = Physics2D.OverlapCircle(positionToDown, 1, roomMask);

            // is there a room below?
            if (roomDetectionToDown == null)
            {
                // no, so we can go this way

                // are we at the limit of rooms for this branch?
                if (branchRoomCount[currentBranchToMoveIndex] < roomLimitForBranch)
                {

                    // Get the room we just created before this
                    Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomMask);

                    int roomDetectionType = roomDetection.GetComponent<RoomType>().type;

                    // check if this room already has an opening in the direction we are trying to go
                    if (roomDetectionType == 1
                        || roomDetectionType == 3
                        || roomDetectionType == 4
                        || roomDetectionType == 6
                        || roomDetectionType == 8
                        || roomDetectionType == 10
                        || roomDetectionType == 13
                        || roomDetectionType == 14)
                    {
                        // don't risk breaking a path by trying to go this way (rare occurance)
                        //continue;
                        return;
                    }

                    branchRoomCount[currentBranchToMoveIndex] += 1;

                    branchRoomDownCount[currentBranchToMoveIndex] += 1;

                    // Get the room we just created before this
                    //Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomMask);

                    //int roomDetectionType = roomDetection.GetComponent<RoomType>().type;

                    print("GOING DOWN AND REPLACING ROOM " + roomDetectionType);

                    roomDetection.GetComponent<RoomType>().RoomDestruction();

                    // replace this room with the necessary room
                    // it couldn't have been a room with a bottom opening so we can remove those from our checks
                    // it can be 0, 2, 5, 7, 9, 11, 12

                    GameObject replacementRoom;

                    // if it had a right and left opening
                    if (roomDetectionType == 0)
                    {
                        // replace it with a right, left and bottom opening
                        replacementRoom = Instantiate(rooms[1], transform.position, Quaternion.identity);
                    }
                    // if it had a right, left and top opening
                    else if (roomDetectionType == 2)
                    {
                        // replace it with a top, right, left and bottom opening
                        replacementRoom = Instantiate(rooms[3], transform.position, Quaternion.identity);
                    }
                    // if it had a left and top opening
                    else if (roomDetectionType == 5)
                    {
                        // replace it with a left, top and bottom opening
                        replacementRoom = Instantiate(rooms[13], transform.position, Quaternion.identity);
                    }
                    // if it had a right and top opening
                    else if (roomDetectionType == 7)
                    {
                        // replace it with a right, top and bottom opening
                        replacementRoom = Instantiate(rooms[14], transform.position, Quaternion.identity);
                    }
                    // if it had a left opening
                    else if (roomDetectionType == 9)
                    {
                        // replace it with a left and bottom opening
                        replacementRoom = Instantiate(rooms[4], transform.position, Quaternion.identity);
                    }
                    // if it had a right opening
                    else if (roomDetectionType == 11)
                    {
                        // replace it with a right and bottom opening
                        replacementRoom = Instantiate(rooms[6], transform.position, Quaternion.identity);
                    }
                    // if it had a top opening
                    else if (roomDetectionType == 12)
                    {
                        // replace it with a top and bottom opening
                        replacementRoom = Instantiate(rooms[8], transform.position, Quaternion.identity);
                    }

                    /*
                    // Check does this room have a bottom opening
                    if(roomDetection.GetComponent<RoomType>().type != 1 && roomDetection.GetComponent<RoomType>().type != 3)
                    {
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
                        //GameObject newRoom_local = Instantiate(rooms[randomBottomRoom], transform.position, Quaternion.identity);
                        Instantiate(rooms[3], transform.position, Quaternion.identity);
                    }
                    */

                    Vector2 newPos = positionToDown;
                    transform.position = newPos;

                    // rooms with index 2 and 3 have top openings
                    //int rand = Random.Range(2, 4);

                    // always spawn a room with just a top opening
                    GameObject newRoom = Instantiate(rooms[12], transform.position, Quaternion.identity);

                    roomsToContinueBranchFrom[currentBranchToMoveIndex] = newRoom;
                }
                else if (branchRoomCount[currentBranchToMoveIndex] == roomLimitForBranch)
                {
                    // spawn dead end
                }
                else
                {
                    // finished with this branch
                    //print("REACHED LIMIT");
                    //continue;
                    return;
                }
            }
            else
            {
                // yes, so stop for now
                //print("CANT GO THIS WAY");
                //branchAttemptCounter++;
                print("ADDING TO ATTEMPT : " + currentBranchToMoveIndex);
                branchRoomAttemptCount[currentBranchToMoveIndex] += 1;
                //continue;
                return;
            }
        }
        else if (branchDirection == 6)
        {

            if (branchRoomUpCount[currentBranchToMoveIndex] >= 1)
            {
                // don't want branch to go up multiple times
                //print("REACHED UP LIMIT");
                print("ADDING TO ATTEMPT : " + currentBranchToMoveIndex);
                branchRoomAttemptCount[currentBranchToMoveIndex] += 1;
                //continue;
                return;
            }

            // Get if there is already a room in this direction
            Vector2 positionToUp = new Vector2(transform.position.x, transform.position.y + moveAmountY);
            Collider2D roomDetectionToUp = Physics2D.OverlapCircle(positionToUp, 1, roomMask);

            // is there a room above?
            if (roomDetectionToUp == null)
            {
                // no, so we can go this way

                // are we at the limit of rooms for this branch?
                if (branchRoomCount[currentBranchToMoveIndex] < roomLimitForBranch)
                {

                    // Get the room we just created before this
                    Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomMask);

                    int roomDetectionType = roomDetection.GetComponent<RoomType>().type;

                    // check if this room already has an opening in the direction we are trying to go
                    if (roomDetectionType == 2
                        || roomDetectionType == 3
                        || roomDetectionType == 5
                        || roomDetectionType == 7
                        || roomDetectionType == 8
                        || roomDetectionType == 12
                        || roomDetectionType == 13
                        || roomDetectionType == 14)
                    {
                        // don't risk breaking a path by trying to go this way (rare occurance)
                        //continue;
                        return;
                    }

                    branchRoomCount[currentBranchToMoveIndex] += 1;

                    branchRoomUpCount[currentBranchToMoveIndex] += 1;

                    // Get the room we just created before this
                    //Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomMask);

                    //int roomDetectionType = roomDetection.GetComponent<RoomType>().type;

                    print("GOING UP AND REPLACING ROOM " + roomDetectionType);

                    roomDetection.GetComponent<RoomType>().RoomDestruction();

                    // replace this room with the necessary room
                    // it couldn't have been a room with a top opening so we can remove those from our checks
                    // it can be 0, 1, 4, 6, 9, 10, 11

                    GameObject replacementRoom;

                    // if it had a right and left opening
                    if (roomDetectionType == 0)
                    {
                        // replace it with a right, left and top opening
                        replacementRoom = Instantiate(rooms[2], transform.position, Quaternion.identity);
                    }
                    // if it had a right, left and bottom opening
                    else if (roomDetectionType == 1)
                    {
                        // replace it with a right, left, bottom and top opening
                        replacementRoom = Instantiate(rooms[3], transform.position, Quaternion.identity);
                    }
                    // if it had a left and bottom opening
                    else if (roomDetectionType == 4)
                    {
                        // replace it with a left, bottom and top opening
                        replacementRoom = Instantiate(rooms[13], transform.position, Quaternion.identity);
                    }
                    // if it had a right and bottom opening
                    else if (roomDetectionType == 6)
                    {
                        // replace it with a right, bottom and top opening
                        replacementRoom = Instantiate(rooms[14], transform.position, Quaternion.identity);
                    }
                    // if it had a left opening
                    else if (roomDetectionType == 9)
                    {
                        // replace it with a left and top opening
                        replacementRoom = Instantiate(rooms[5], transform.position, Quaternion.identity);
                    }
                    // if it had a bottom opening
                    else if (roomDetectionType == 10)
                    {
                        // replace it with a bottom and top opening
                        replacementRoom = Instantiate(rooms[8], transform.position, Quaternion.identity);
                    }
                    // if it had a right opening
                    else if (roomDetectionType == 11)
                    {
                        // replace it with a right and top opening
                        replacementRoom = Instantiate(rooms[7], transform.position, Quaternion.identity);
                    }

                    // check if the room found has a top opening
                    /*
                    if (roomDetection.GetComponent<RoomType>().type != 2 && roomDetection.GetComponent<RoomType>().type != 3)
                    {
                        roomDetection.GetComponent<RoomType>().RoomDestruction();

                        // we want to create a room that has a bottom opening
                        // we want an index of 2 or 3
                        //int randomBottomRoom_local = Random.Range(2, 4);

                        //GameObject newRoom_local = Instantiate(rooms[randomBottomRoom], transform.position, Quaternion.identity);
                        //Instantiate(rooms[3], transform.position, Quaternion.identity);
                    }
                    */

                    Vector2 newPos = positionToUp;
                    transform.position = newPos;

                    // rooms with index 1 and 3 have bottom openings
                    //int rand = Random.Range(1, 4);

                    //if(rand == 2)
                    //{
                    //    rand = 1;
                    //}

                    // always just spawn a room with a bottom opening
                    GameObject newRoom = Instantiate(rooms[10], transform.position, Quaternion.identity);

                    roomsToContinueBranchFrom[currentBranchToMoveIndex] = newRoom;
                }
                else if (branchRoomCount[currentBranchToMoveIndex] == roomLimitForBranch)
                {
                    // spawn dead end
                }
                else
                {
                    // finished with this branch
                    //print("REACHED LIMIT");
                    //continue;
                    return;
                }
            }
            else
            {
                // yes, so stop for now
                //print("CANT GO THIS WAY");
                //branchAttemptCounter++;
                print("ADDING TO ATTEMPT : " + currentBranchToMoveIndex);
                branchRoomAttemptCount[currentBranchToMoveIndex] += 1;
                //continue;
                return;
            }
        }
        //}

        /*
        bool allLimitsMet = true;

        foreach(KeyValuePair<int, int> pair in branchRoomCount)
        {
            print("CHECKING " + pair.Key + " ROOM LIMIT " + pair.Value);
            if (!(pair.Value >= roomLimitForBranch))
            {
                allLimitsMet = false;
            }
        }

        if(allLimitsMet)
        {
            branchingFinished = true;
            print("CALLING SPAWN PLAYER");
            SpawnPlayer();
        }
        */

        //currentBranchToMoveIndex++;

        //if(currentBranchToMoveIndex >= roomsToStartBranchFrom.Count)
        //{
            //print("MOVING ONTO NEXT BRANCH");
        //    currentBranchToMoveIndex = 0;
        //}
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
