using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class LevelGeneration2 : MonoBehaviour
{
    public Dynamic_Difficulty_Adjustment dda;

    public Prison_Loading_Screen_Manager loadingScreen;

    public GameObject endRoom;

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

    // dictionary of room to branch from on the critical path and the current count of times it has gone down
    public Dictionary<int, int> branchRoomDownCount;

    // dictionary of room to branch from on the critical path and the current count of times it has gone up
    public Dictionary<int, int> branchRoomUpCount;

    // dictionary of room to branch from on the critical path and the amount of attempts made to make a room
    public Dictionary<int, int> branchRoomAttemptCount;

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

        roomsToContinueBranchFrom = new Dictionary<int, GameObject>();

        branchRoomCount = new Dictionary<int, int>();

        branchRoomDownCount = new Dictionary<int, int>();

        branchRoomUpCount = new Dictionary<int, int>();

        branchRoomAttemptCount = new Dictionary<int, int>();

        // get a random starting point, set this objects position to it, and spawn our first room
        int randStartingPos = Random.Range(0, startingPositions.Length);
        transform.position = startingPositions[randStartingPos].position;

        // spawn a room with just a left opening first
        GameObject newRoom = Instantiate(rooms[9], transform.position, Quaternion.identity);

        criticalPathRooms.Add(newRoom);

        // set direction to random number between 1 and 2
        // start with going right
        direction = Random.Range(1, 3);
    }

    private void Update()
    {
        if (timeBetweenRoomSpawn <= 0 && !criticalPathFinished)
        {
            Move();
            timeBetweenRoomSpawn = spawnRoomCooldown;
        }
        else if(timeBetweenRoomSpawn <= 0 && criticalPathFinished && !branchingFinished)
        {
            MoveBranch();
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
            if(transform.position.x <= maxX)
            {

                // Get the room we just created before this
                Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomMask);

                if (roomDetection != null)
                {

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
                        roomDetection.GetComponent<RoomType>().RoomDestruction();

                        // check which room type should replace this room
                        // it can be 9, 10, 12
                        // if previous room was a left opening room
                        if (roomDetectionType == 9)
                        {
                            // need to spawn a room with left and right openings
                            GameObject newRoom_local = Instantiate(rooms[0], transform.position, Quaternion.identity);
                            criticalPathRooms.Add(newRoom_local);
                        }
                        // if it was a bottom opening room
                        else if (roomDetectionType == 10)
                        {
                            // need to spawn a room with bottom and right openings
                            GameObject newRoom_local = Instantiate(rooms[6], transform.position, Quaternion.identity);
                            criticalPathRooms.Add(newRoom_local);
                        }
                        // if it was a top opening room
                        else if (roomDetectionType == 12)
                        {
                            // need to spawn a room with top and right openings
                            GameObject newRoom_local = Instantiate(rooms[7], transform.position, Quaternion.identity);
                            criticalPathRooms.Add(newRoom_local);
                        }
                    }

                    // we are within the maxX so we can move right
                    Vector2 newPos = new Vector2(transform.position.x + moveAmountX, transform.position.y);
                    transform.position = newPos;

                    if (transform.position.x == maxX)
                    {
                        GameObject newRoom = Instantiate(endRoom, transform.position, Quaternion.identity);

                        criticalPathFinished = true;

                        int numberOfBranches = Random.Range(10, criticalPathRooms.Count / 3);

                        CreateBranches(numberOfBranches);
                    }
                    else
                    {

                        // Spawn a room with a left opening
                        GameObject newRoom = Instantiate(rooms[9], transform.position, Quaternion.identity);

                        criticalPathRooms.Add(newRoom);

                    }

                    // reset down and up counter
                    downCounter = 0;
                    upCounter = 0;

                    // set direction to random number between 1 and 4
                    direction = Random.Range(1, 5);

                    if (transform.position.x == maxX)
                    {

                        //print(transform.position);

                        //GameObject finalRoom = Instantiate(endRoom, transform.position, Quaternion.identity);

                        //criticalPathFinished = true;

                        //int numberOfBranches = Random.Range(10, criticalPathRooms.Count / 3);

                        //CreateBranches(numberOfBranches);

                    }

                }
            }
            /*
            else if(transform.position.x == maxX)
            {

                Vector2 positionToRight = new Vector2(transform.position.x + moveAmountX, transform.position.y);

                // Get the room we just created before this
                //Vector2 positionToLeft = new Vector2(transform.position.x - moveAmountX, transform.position.y);
                Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomMask);
                int roomDetectionType = roomDetection.GetComponent<RoomType>().type;
                //print(roomToLeftType);

                //Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomMask);
                //int roomDetectionType = roomDetection.GetComponent<RoomType>().type;

                //criticalPathRooms.Remove(roomDetection.gameObject.transform.parent.gameObject);
                roomDetection.GetComponent<RoomType>().RoomDestruction();

                // check which room type should replace this room
                // it can be 9, 10, 12
                // if previous room was a left opening room
                if (roomDetectionType == 9)
                {
                    // need to spawn a room with left and right openings
                    GameObject newRoom_local = Instantiate(rooms[0], transform.position, Quaternion.identity);
                    //criticalPathRooms.Add(newRoom_local);
                }
                // if it was a bottom opening room
                else if (roomDetectionType == 10)
                {
                    // need to spawn a room with bottom and right openings
                    GameObject newRoom_local = Instantiate(rooms[6], transform.position, Quaternion.identity);
                    //criticalPathRooms.Add(newRoom_local);
                }
                // if it was a top opening room
                else if (roomDetectionType == 12)
                {
                    // need to spawn a room with top and right openings
                    GameObject newRoom_local = Instantiate(rooms[7], transform.position, Quaternion.identity);
                    //criticalPathRooms.Add(newRoom_local);
                }

                //Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomMask);
                //roomDetection.GetComponent<RoomType>().RoomDestruction();

                GameObject finalRoom = Instantiate(endRoom, transform.position, Quaternion.identity);

                criticalPathFinished = true;

                int numberOfBranches = Random.Range(10, criticalPathRooms.Count / 3);

                CreateBranches(numberOfBranches);
            }
            */
        }
        else if(direction == 3)
        {
            // increment down counter
            downCounter++;

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

                        // we destroy the room that might have not had a bottom opening
                        criticalPathRooms.Remove(roomDetection.gameObject.transform.parent.gameObject);
                        roomDetection.GetComponent<RoomType>().RoomDestruction();

                        // since this is the second time going down, the only room type that can be spawned here is the top and bottom opening room
                        GameObject newRoom_local = Instantiate(rooms[8], transform.position, Quaternion.identity);

                        criticalPathRooms.Add(newRoom_local);
                    }
                    else
                    {
                        int roomDetectionType = roomDetection.GetComponent<RoomType>().type;

                        // if not, we destroy the last room and spwan a room with all 4 openings or with LRB
                        criticalPathRooms.Remove(roomDetection.gameObject.transform.parent.gameObject);
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
                    }
                }

                // We are within the minY so we can move down
                Vector2 newPos = new Vector2(transform.position.x, transform.position.y - moveAmountY);
                transform.position = newPos;

                // only spawn a room with a top opening
                GameObject newRoom = Instantiate(rooms[12], transform.position, Quaternion.identity);

                criticalPathRooms.Add(newRoom);

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
                        roomDetection.GetComponent<RoomType>().RoomDestruction();

                        // since this is the second time going up, the only room type that can be spawned here is the top and bottom opening room
                        GameObject newRoom_local = Instantiate(rooms[8], transform.position, Quaternion.identity);

                        //GameObject newRoom_local = Instantiate(rooms[3], transform.position, Quaternion.identity);
                        criticalPathRooms.Add(newRoom_local);
                    }
                    else
                    {
                        int roomDetectionType = roomDetection.GetComponent<RoomType>().type;

                        // if not, we destroy the last room and spwan a room with all 4 openings or with LRT
                        criticalPathRooms.Remove(roomDetection.gameObject.transform.parent.gameObject);
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
                    }
                }

                // We are within the minY so we can move down
                Vector2 newPos = new Vector2(transform.position.x, transform.position.y + moveAmountY);
                transform.position = newPos;

                // only spawn a room with a bottom opening
                GameObject newRoom = Instantiate(rooms[10], transform.position, Quaternion.identity);

                criticalPathRooms.Add(newRoom);

                // check if we are making a corner or continuing up

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
    }

    void CreateBranches(int numberOfBranches)
    {
        for (int i = 0; i < numberOfBranches; i++)
        {
            int roomIndexToBranchFrom = Random.Range(1, criticalPathRooms.Count - 1);
            GameObject roomToBranchFrom = criticalPathRooms[roomIndexToBranchFrom];

            if (!roomsToStartBranchFrom.Contains(roomToBranchFrom))
            {
                roomsToStartBranchFrom.Add(roomToBranchFrom);
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
            currentBranchToMoveIndex = 0;
        }

        GameObject roomToBranchFrom;
        if (roomsToContinueBranchFrom.ContainsKey(currentBranchToMoveIndex))
        {
            roomToBranchFrom = roomsToContinueBranchFrom[currentBranchToMoveIndex];
        }
        else
        {
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
                    branchRoomCount[i] = roomLimitForBranch;
                }
            }
        }

        bool allLimitsMet = true;

        foreach (KeyValuePair<int, int> pair in branchRoomCount)
        {
            if (!(pair.Value >= roomLimitForBranch))
            {
                allLimitsMet = false;
            }
        }

        if (allLimitsMet)
        {
            branchingFinished = true;
            SpawnPlayer();
            return;
        }

        transform.position = roomToBranchFrom.transform.position;

        // 1 & 2 = right
        // 3 & 4 = left
        // 5 = down
        // 6 = up
        branchDirection = Random.Range(1, 7);

        if (branchDirection == 1 || branchDirection == 2)
        {
            // Get if there is already a room in this direction
            Vector2 positionToRight = new Vector2(transform.position.x + moveAmountX, transform.position.y);
            Collider2D roomDetectionToRight = Physics2D.OverlapCircle(positionToRight, 1, roomMask);

            // is there a room to the right?
            if (roomDetectionToRight == null)
            {
                // no, so we can go this way

                // are we at the limit of rooms for this branch?
                if (branchRoomCount[currentBranchToMoveIndex] < roomLimitForBranch)
                {

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
                        return;
                    }

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
                    return;
                }
            }
            else
            {
                // yes, so stop for now
                branchRoomAttemptCount[currentBranchToMoveIndex] += 1;
                return;
            }
        }
        else if (branchDirection == 3 || branchDirection == 4)
        {
            // Get if there is already a room in this direction
            Vector2 positionToLeft = new Vector2(transform.position.x - moveAmountX, transform.position.y);
            Collider2D roomDetectionToLeft = Physics2D.OverlapCircle(positionToLeft, 1, roomMask);

            // is there a room to the left?
            if (roomDetectionToLeft == null)
            {
                // no, so we can go this way

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
                        return;
                    }

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
                    return;
                }
            }
            else
            {
                branchRoomAttemptCount[currentBranchToMoveIndex] += 1;
                return;
            }
        }
        else if (branchDirection == 5)
        {

            if (branchRoomDownCount[currentBranchToMoveIndex] >= 1)
            {
                branchRoomAttemptCount[currentBranchToMoveIndex] += 1;
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
                        return;
                    }

                    branchRoomCount[currentBranchToMoveIndex] += 1;

                    branchRoomDownCount[currentBranchToMoveIndex] += 1;

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

                    Vector2 newPos = positionToDown;
                    transform.position = newPos;

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
                    return;
                }
            }
            else
            {
                branchRoomAttemptCount[currentBranchToMoveIndex] += 1;
                return;
            }
        }
        else if (branchDirection == 6)
        {

            if (branchRoomUpCount[currentBranchToMoveIndex] >= 1)
            {
                branchRoomAttemptCount[currentBranchToMoveIndex] += 1;
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
                        return;
                    }

                    branchRoomCount[currentBranchToMoveIndex] += 1;

                    branchRoomUpCount[currentBranchToMoveIndex] += 1;

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

                    Vector2 newPos = positionToUp;
                    transform.position = newPos;

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
                    return;
                }
            }
            else
            {
                branchRoomAttemptCount[currentBranchToMoveIndex] += 1;
                return;
            }
        }
    }

    void SpawnPlayer()
    {
        player.transform.position = playerSpawnPoint.position;
        player.SetActive(true);

        int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        int totalRoomCount = GameObject.FindGameObjectsWithTag("Room").Length;

        dda.SetEnemyCount(enemyCount);
        dda.SetBranchCount(roomsToStartBranchFrom.Count);
        dda.SetTotalRoomCount(totalRoomCount);

        dda.LevelGenIsCompleted();
        loadingScreen.FinishedLoading();
    }
}
