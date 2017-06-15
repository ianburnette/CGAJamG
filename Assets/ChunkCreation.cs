using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Chunk
{
    public bool placed = false;
    public GameObject thisChunkGO;
    public List<Transform> pieces;
    public Direction entranceDirection, exitDirection, secondaryExit, tertiaryExit = Direction.None;
    public bool openUp, openDown, openLeft, openRight;
}

public enum Direction {None, Up, Down, Left, Right};

public class ChunkCreation : MonoBehaviour {

    #region Private Variables
    [Header("Chunks")]
    [SerializeField] GameObject chunkPrefab;
    [SerializeField] Chunk[,] chunks;
    [SerializeField] Chunk lastChunk;
    [SerializeField] Transform[] chunkRow;
    [SerializeField] GameObject primaryRoute, secondaryRoute;
    [SerializeField] Vector2 indicatorOffset, baseOffset;

    [Header("Pieces")]
    [SerializeField] GameObject[] openSides;
    [SerializeField] GameObject[] closedides;
    [SerializeField] GameObject levelStart, levelEnd;
    [SerializeField] Transform thisStart, thisEnd;

    [Header("Level Specs")]
    [SerializeField] Vector2 chunkSize;
    [SerializeField] Vector2 levelSize;
    [SerializeField]Vector2 startOffset;

    [Header("Level Behaviour")]
    [SerializeField] float horizontalTendency;
    [SerializeField] float chunkConnectednessLikelihood;
    [SerializeField] float waitTime;

    [Header("References")]
    [SerializeField] Transform player;
    #endregion

    #region Public Properties
    #endregion

    #region Unity Functions
    void OnEnable()
    {
        chunks = new Chunk[Mathf.RoundToInt(levelSize.x), Mathf.RoundToInt(levelSize.y)];   // establishes the 2D array will all chunks needed for level
        DeterminePath();
    }

    void OnDisable()
    {
        for (int i = 0; i < chunkRow.Length; i++) {
            foreach (Transform child in chunkRow[i])
                Destroy(child.gameObject);
        }
        Destroy(thisStart.gameObject);
        Destroy(thisEnd.gameObject);
    }
    #endregion

    #region Custom Functions
    void DeterminePath()
    {
        int currentColumn = 0;
        currentColumn = Random.Range(0, Mathf.RoundToInt(levelSize.x));                 //select starting column - where are you entering the level
        print("starting node selected as " + currentColumn);
        StartCoroutine("Generate", currentColumn);
    }

    IEnumerator Generate(int currentColumn)
    {
        int currentRow = 0;
        thisStart = CreateCap(0, currentRow, currentColumn);
        SetPlayer();
        while (currentRow < levelSize.y )                                                  //now we have a node to start populating
        {
            Direction dir = CreateChunks(currentRow, currentColumn);                              //creates a chunk, returns the exit direction from that chunk
            MoveToNextChunk(dir, ref currentRow, ref currentColumn);                        //moves the iterator 
            yield return new WaitForSeconds(waitTime);
            yield return new WaitForEndOfFrame();
        }
        thisEnd = CreateCap(1, currentRow, currentColumn);
        StartCoroutine("FillGaps");
        yield return null;
    }

    void SetPlayer()
    {
        player.position = (Vector2)thisStart.position + startOffset;
    }

    Transform CreateCap(int which, int row, int column)
    {
        GameObject cap = null;
        if (which == 0)
        {
             cap = Instantiate(levelStart, new Vector2(chunkSize.x * (column+1), 0), Quaternion.identity);
            cap.transform.parent = chunkRow[0].parent;
        }
        else
        {
             cap = Instantiate(levelEnd, new Vector2(chunkSize.x * (column+1), -chunkSize.y * (levelSize.y + 1)), Quaternion.identity);
            cap.transform.parent = chunkRow[0].parent;
        }
        return cap.transform;
    }

    IEnumerator FillGaps()
    {
        for (int i = 0; i<levelSize.y; i++)
        {
            for (int j = 0; j<levelSize.x; j++)
            {
                if (chunks[j, i] == null)
                    chunks[j, i] = new Chunk();
                if (!chunks[j, i].placed)
                {
                    PlaceExtraChunk(j, i);
                    yield return new WaitForSeconds(waitTime);
                }                
                yield return new WaitForEndOfFrame();
             }
        }
        CalculateOpenings();
        StartCoroutine ("PlaceSecondaryRouteIndicators");
        yield return null;
    }

    void CalculateOpenings()
    {
        for (int i = 0; i < levelSize.y; i++)
        {
            for (int j = 0; j < levelSize.x; j++)
            {
                if (chunks[i, j].entranceDirection == Direction.Up ||
                    chunks[i, j].exitDirection == Direction.Up)
                    chunks[i, j].openUp = true;
                if (chunks[i, j].entranceDirection == Direction.Down ||
                    chunks[i, j].exitDirection == Direction.Down)
                    chunks[i, j].openDown = true;
                if (chunks[i, j].entranceDirection == Direction.Left||
                    chunks[i, j].exitDirection == Direction.Left)
                    chunks[i, j].openLeft = true;
                if (chunks[i, j].entranceDirection == Direction.Right ||
                    chunks[i, j].exitDirection == Direction.Right)
                    chunks[i, j].openRight = true;
            }
        }
    }

    IEnumerator PlaceSecondaryRouteIndicators()
    {
        for (int i = 0; i < levelSize.y; i++)
        {
            for (int j = 0; j < levelSize.x; j++)
            {
                Chunk currentChunk = chunks[i, j];
                if (currentChunk.openUp && currentChunk.entranceDirection != Direction.Up && currentChunk.exitDirection != Direction.Up)
                {
                    CreateRouteIndicator(Direction.Up, currentChunk.thisChunkGO, 1);
                    currentChunk.thisChunkGO.transform.GetChild(0).GetChild(0).GetComponent<Text>().text += "\n secondary = up";
                    chunks[i - 1, j].thisChunkGO.transform.GetChild(0).GetChild(0).GetComponent<Text>().text += "\n secondary = down";
                    chunks[i - 1, j].openDown = true;
                    currentChunk.openUp = true;
                //    currentChunk.thisChunkGO.transform.GetChild(0).GetChild(0).GetComponent<Text>().text += "\n secondary = up";
                    yield return new WaitForSeconds(waitTime);
                    yield return new WaitForEndOfFrame();
                }
                if (currentChunk.openDown && currentChunk.entranceDirection != Direction.Down && currentChunk.exitDirection != Direction.Down)
                {
                    CreateRouteIndicator(Direction.Down, currentChunk.thisChunkGO, 1);
                    currentChunk.thisChunkGO.transform.GetChild(0).GetChild(0).GetComponent<Text>().text += "\n secondary = down";
                    chunks[i + 1, j ].thisChunkGO.transform.GetChild(0).GetChild(0).GetComponent<Text>().text += "\n secondary = up";
                    chunks[i + 1, j].openUp = true;
                    currentChunk.openDown = true;
                    yield return new WaitForSeconds(waitTime);
                    yield return new WaitForEndOfFrame();
                }
                if (currentChunk.openLeft && currentChunk.entranceDirection != Direction.Left && currentChunk.exitDirection != Direction.Left)
                {
                    CreateRouteIndicator(Direction.Left, currentChunk.thisChunkGO, 1);
                    currentChunk.thisChunkGO.transform.GetChild(0).GetChild(0).GetComponent<Text>().text += "\n secondary = left";
                    chunks[i , j - 1].thisChunkGO.transform.GetChild(0).GetChild(0).GetComponent<Text>().text += "\n secondary = right";
                    currentChunk.openLeft = true;
                    chunks[i, j - 1].openRight = true;
                    yield return new WaitForSeconds(waitTime);
                    yield return new WaitForEndOfFrame();
                }
                if (currentChunk.openRight && currentChunk.entranceDirection != Direction.Right && currentChunk.exitDirection != Direction.Right)
                {
                    CreateRouteIndicator(Direction.Right, currentChunk.thisChunkGO, 1);
                    currentChunk.thisChunkGO.transform.GetChild(0).GetChild(0).GetComponent<Text>().text += "\n secondary = right";
                    chunks[i, j + 1].thisChunkGO.transform.GetChild(0).GetChild(0).GetComponent<Text>().text += "\n secondary = left";
                    currentChunk.openRight = true;
                    chunks[i, j + 1].openLeft = true;
                    yield return new WaitForSeconds(waitTime);
                    yield return new WaitForEndOfFrame();
                }  
            }
            StartCoroutine("BuildChunkPieces");
        }
        yield return null;
    }

    IEnumerator BuildChunkPieces()
    {
        foreach (Chunk chunk in chunks)
        {
            if (chunk.openUp)
                PlacePiece(openSides[0], chunk.thisChunkGO);
            else
                PlacePiece(closedides[0], chunk.thisChunkGO);
            if (chunk.openDown)
                PlacePiece(openSides[1], chunk.thisChunkGO);
            else
                PlacePiece(closedides[1], chunk.thisChunkGO);
            if (chunk.openLeft)
                PlacePiece(openSides[2], chunk.thisChunkGO);
            else
                PlacePiece(closedides[2], chunk.thisChunkGO);
            if (chunk.openRight)
                PlacePiece(openSides[3], chunk.thisChunkGO);
            else
                PlacePiece(closedides[3], chunk.thisChunkGO);
            yield return new WaitForSeconds(waitTime);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    void PlacePiece(GameObject type, GameObject parentGO)
    {
        GameObject newChunk = GameObject.Instantiate(type, parentGO.transform.position, Quaternion.identity);
        newChunk.transform.parent = parentGO.transform;
    }

    void PlaceExtraChunk(int currentRow, int currentColumn)
    {
       // print("filling gap " + currentRow + ", " + currentColumn);
        chunks[currentRow, currentColumn] = new Chunk();
        chunks[currentRow, currentColumn].placed = true;
        //should I go left
        if (currentColumn != 0 && Random.value > chunkConnectednessLikelihood)
        {
            chunks[currentRow, currentColumn].openLeft = true;
            if (chunks[currentRow, currentColumn-1]==null)
                chunks[currentRow, currentColumn-1] = new Chunk();
            chunks[currentRow, currentColumn-1].openRight = true;
        }
        //should I go right
        if (currentColumn != levelSize.x -1 && Random.value > chunkConnectednessLikelihood)
        {
            chunks[currentRow, currentColumn].openRight = true;
            if (chunks[currentRow, currentColumn + 1] == null)
                chunks[currentRow, currentColumn + 1] = new Chunk();
            chunks[currentRow, currentColumn+1].openLeft = true;
        }
        //should I go down
        if (currentRow != levelSize.y -1 && Random.value > chunkConnectednessLikelihood)
        {
            chunks[currentRow, currentColumn].openDown = true;
            if (chunks[currentRow + 1, currentColumn] == null)
                chunks[currentRow + 1, currentColumn] = new Chunk();
            chunks[currentRow+1, currentColumn].openUp = true;
        }
        //should I go up
        if (currentRow != 0 && Random.value > chunkConnectednessLikelihood)
        {
            chunks[currentRow, currentColumn].openUp = true;
            if (chunks[currentRow - 1, currentColumn] == null)
                chunks[currentRow - 1, currentColumn] = new Chunk();
            chunks[currentRow-1, currentColumn].openDown = true;
        }
        //print("still filling gap " + currentRow + ", " + currentColumn);
        PlaceChunk(currentRow, currentColumn);
      
    }

    Direction CreateChunks(int currentRow, int currentColumn)
    {
        //print ("Creating " + currentRow +  ", " + currentColumn);
        chunks[currentRow, currentColumn] = new Chunk();
        chunks[currentRow, currentColumn].placed = true;
        chunks[currentRow, currentColumn].entranceDirection = lastChunk == null ? Direction.Up : OppositeDirection(lastChunk.exitDirection);
        
        bool canGoLeft = true;
        bool canGoRight = true;
        if (currentColumn == 0)
            canGoLeft = false;
        else if (currentColumn == 3)
            canGoRight = false;
        if (chunks[currentRow, currentColumn].entranceDirection == Direction.Left)
            canGoLeft = false;
        if (chunks[currentRow, currentColumn].entranceDirection == Direction.Right)
            canGoRight = false;
        /* else if (currentColumn == 1 || currentColumn == 2) {
             print("in row 1 or 2");
             if (chunks[currentRow, currentColumn--].exitDirection == Direction.Right)
                 canGoLeft = false;
             if (chunks[currentRow, currentColumn++].exitDirection == Direction.Left)
                 canGoRight = false;
         }*/
        Direction nextDir = DetermineNextStep(canGoLeft, canGoRight);
        chunks[currentRow, currentColumn].exitDirection = nextDir;
        PlaceChunk(currentRow, currentColumn);
        CreateRouteIndicator(chunks[currentRow, currentColumn].entranceDirection, chunks[currentRow, currentColumn].thisChunkGO, 0);
        return nextDir;
    }

    void CreateRouteIndicator(Direction dir, GameObject thisChunk, int type)
    {
        GameObject indicatorType = type == 0 ? primaryRoute : secondaryRoute;
        GameObject newIndicator = GameObject.Instantiate(indicatorType);
        newIndicator.transform.parent = thisChunk.transform;
        Vector2 vec2 = DirectionOffset(dir) + baseOffset;
        Vector3 vec3 = new Vector3(vec2.x, vec2.y, 0);
        newIndicator.transform.position = thisChunk.transform.position + vec3;
        if (dir == Direction.Up || dir == Direction.Down)
            newIndicator.transform.rotation = Quaternion.Euler(0, 0, 90);
    }

    Vector2 DirectionOffset(Direction dir)
    {
        if (dir == Direction.Up)
            return new Vector2(0, indicatorOffset.y);
        else if (dir == Direction.Down)
            return new Vector2(0, -indicatorOffset.y);
        else if (dir == Direction.Left)
            return new Vector2(-indicatorOffset.x, 0);
        else if (dir == Direction.Right)
            return new Vector2(indicatorOffset.x, 0);
        else
            return Vector2.zero;
    }

    void PlaceChunk(int currentRow, int currentColumn)
    {
        print("placing chunk at " + currentRow + ", " + currentColumn);
        GameObject currentChunkGO = GameObject.Instantiate(chunkPrefab);
        currentChunkGO.transform.position = new Vector2((currentColumn + 1) * chunkSize.x, (currentRow + 1) * -chunkSize.y);
       // if (chunkRow.Length <= currentRow)
            currentChunkGO.transform.parent = chunkRow[currentRow];
        Text description = currentChunkGO.transform.GetChild(0).GetChild(0).GetComponent<Text>();
        description.text = "entrance = " + chunks[currentRow, currentColumn].entranceDirection + "\n exit = " + chunks[currentRow, currentColumn].exitDirection;
        chunks[currentRow, currentColumn].thisChunkGO = currentChunkGO;
        lastChunk = chunks[currentRow, currentColumn];
    }

    Direction OppositeDirection(Direction incomingDirection) {
        if (incomingDirection == Direction.Left)
            return Direction.Right;
        else if (incomingDirection == Direction.Right)
            return Direction.Left;
        else
            return Direction.Up;
    }

    void MoveToNextChunk(Direction dir, ref int currentRow, ref int currentColumn)
    {
        string status = "We're moving from " + currentRow + ", " + currentColumn + " to ";
        if (dir == Direction.Down)
            currentRow++;
        else if (dir == Direction.Left)
            currentColumn--;
        else if (dir == Direction.Right)
            currentColumn++;
        status += "" + currentRow + ", " + currentColumn;
        //print(status);
    }

    Direction DetermineNextStep(bool canGoLeft, bool canGoRight)
    {
        //print("can go left? " + canGoLeft + " can go right? " + canGoRight);
        Direction dir = Direction.None;


        int directionInt = 0;
        if ((!canGoLeft && canGoRight) || (!canGoRight && canGoLeft))
        {
            if (Random.value > horizontalTendency && horizontalTendency != 0)
                directionInt = 1;
            else
                directionInt = 0;
            //directionInt = Random.Range(0, 1);
        }
        else if (!canGoLeft && !canGoRight)
            directionInt = 0;
        else
        {
            if (Random.value > horizontalTendency && horizontalTendency != 0)
            {
                directionInt = Random.Range(2, 3);
                if (directionInt == 2)
                    dir = Direction.Left;
                else
                    dir = Direction.Right;
            }
            else
                directionInt = 0;
            
        }
        if (directionInt == 1)
            if (canGoRight)
                dir = Direction.Right;
            else
                dir = Direction.Left;
        else if (directionInt == 0)
            dir = Direction.Down;
        //print("returning " + dir);

        return dir;
    }
    #endregion
}
