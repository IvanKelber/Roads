using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileRotation;

public class BoardManager : MonoBehaviour
{
    [Range(1, 12)]
    public int numberOfColumns;
    [Range(1,12)]
    public int numberOfRows;

    [Range(0, 100)]
    public float marginX;
    [Range(0,100)]
    public float marginY;

    [Range(0,100)]
    public float cellPadding;


    [SerializeField, Range(.01f,1)]
    private float cellRotationDuration = .1f;

    [SerializeField, Range(0, 50)]
    private int popMagnitude = 1;

    public Camera cam;

    public Tile[,] grid;

    public float cellWidth;
    private float cellHeight;

    [SerializeField]
    private Tile tilePrefab;

    [SerializeField]
    private Level firstLevel;

    private Bounds outerBoardBounds;
    private Bounds innerBoardBounds;

    private bool rotatingTiles = false;
    private bool invalidStep = false;


    [SerializeField]
    private AudioManager SFXManager;

    private AudioSource audioSource;

    private bool Busy {
        get {
            return rotatingTiles || invalidStep;
        }
    }

    public Car car;

    private Stack<PlayerAction> playerActions = new Stack<PlayerAction>();

    private void Awake()
    {
        Gestures.OnSwipe += HandleSwipe;
        Gestures.OnTap += TryStep;
        audioSource = GetComponent<AudioSource>();
        LoadLevel(firstLevel);
    }

    private void RenderBoard(Level level) {

        if(grid != null) {
            for(int i = 0; i < grid.GetLength(0); i++) {
                for(int j = 0; j < grid.GetLength(1); j++) {
                    if(grid[i,j] != null) {
                        grid[i,j].Destroy();
                    }
                }
            }
        }
        if(cam == null) {
            Debug.LogError("Cannot calculate grid bounds because camera is missing.");
            return;
        }
        if(level != null) {
            numberOfColumns = level.numberOfColumns;
            numberOfRows = level.numberOfRows;
        }

        // The height of the frustum at a given distance (both in world units) can be obtained with the following formula:
        float frustumHeight = 2.0f * cam.farClipPlane/2 * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

    
        float frustumWidth = frustumHeight * cam.aspect;

        float boardHeight = frustumHeight - marginY;
        float boardWidth = frustumWidth - marginX;
        cellWidth = boardWidth/numberOfColumns;
        cellHeight = cellWidth;
        outerBoardBounds = new Bounds(cam.transform.position + Vector3.forward * cam.farClipPlane/2, new Vector3(boardWidth, boardHeight, 0));
        innerBoardBounds = new Bounds(outerBoardBounds.center, new Vector3((cellWidth + cellPadding) * numberOfColumns, (cellHeight + cellPadding) * numberOfRows, 0));
        transform.position = outerBoardBounds.center;
        grid = InitializeGrid(level);
    }

    private void RenderBoard() {
        RenderBoard(null);
    }

    private void LoadLevel(Level level) {
        RenderBoard(level);
        car.Initialize(level.startingIndex);
    }

    private Tile[,] InitializeGrid(Level level) {
        Tile[,] grid = new Tile[numberOfColumns,numberOfRows];
        for(int col = 0; col < numberOfColumns; col++) {
            for(int row = 0; row < numberOfRows; row++) { 
                if(level != null) { 
                    grid[col,row] = RenderTile(col, row, cellWidth, level.levelMatrix[row][col].isStraight, level.levelMatrix[row][col].startingOrientation);
                } else {
                    grid[col,row] = RenderTile(col, row, cellWidth, Random.value > .5f, RotationHelper.RandomOrientation());
                }
            }
        }
        return grid;
    }

    private IEnumerator RepositionTile(Tile tile, float duration) {
        Vector3 endPosition = CalculateGamePosition(tile.Index.x, tile.Index.y, innerBoardBounds);
        float step = Mathf.Abs(endPosition.y - tile.Position.y)/duration;

        float startTime = Time.time;
        float endTime = startTime + duration;

        while(Time.time < endTime) {
            tile.transform.Translate(Vector3.down * Time.deltaTime * step);
            yield return null;
        }
        tile.transform.position = endPosition;
    }

    private IEnumerator RotateTile(Tile tile, float degrees, float duration) {

        Orientation endOrientation = tile.Orientation.GetNextOrientation(degrees);

        Vector3 center = tile.Position;
        float startMoving = Time.time;
        float endMoving = startMoving + .15f;
        while(Time.time < endMoving) {
            tile.transform.Translate(Vector3.forward * Time.deltaTime * -popMagnitude/duration);
            yield return null;
        }
        yield return new WaitForSeconds(.1f);

        float totalDegrees = 0;
        float startTime = Time.time;
        float endTime = startTime + duration;
        while(Time.time < endTime || totalDegrees < degrees) {
            float rotationThisFrame = Time.deltaTime * degrees / duration;
            totalDegrees += Mathf.Abs(rotationThisFrame);
            tile.transform.Rotate(Vector3.forward, rotationThisFrame);
            yield return null;
        }
        SFXManager.Play("Rotate", audioSource);

        yield return new WaitForSeconds(.1f);
    
        tile.Position = CalculateGamePosition(tile.Index.x, tile.Index.y, innerBoardBounds);
        tile.Orientation = endOrientation;
        
        yield return null;
    }

    private IEnumerator RotateTiles(float degrees, bool undo) {
        rotatingTiles = true;
        for(int i = 0; i < numberOfColumns; i++) {
            for(int j = 0; j < numberOfRows; j++) {
                if(!grid[i,j].IsLocked) {
                    Coroutine rotation = StartCoroutine(RotateTile(grid[i,j], degrees, cellRotationDuration));
                    if(i == numberOfColumns - 1 && j == numberOfRows -1) {
                        yield return rotation;
                    }
                }
            }
        }
        if(!undo) {
            TryStep();
        }
        rotatingTiles = false;

    }

    private void Rotate(SwipeInfo.SwipeDirection direction) {
        float degrees = direction == SwipeInfo.SwipeDirection.LEFT ? -90 : 90;
        StartCoroutine(RotateTiles(degrees, false));
        playerActions.Push(new RotateAction(this, degrees));
    }

    public void Rotate(float degrees) {
        StartCoroutine(RotateTiles(degrees, true));
    }
    private void Update() {
        if(Input.GetKeyDown(KeyCode.Space)) {
            RenderBoard();
        }
        if(Input.GetKeyDown(KeyCode.LeftArrow)&& !Busy) {
            Rotate(SwipeInfo.SwipeDirection.LEFT);
        }
        if(Input.GetKeyDown(KeyCode.RightArrow) && !Busy) {
            Rotate(SwipeInfo.SwipeDirection.RIGHT);
        }
        if(Input.GetKeyDown(KeyCode.S)) {
            TryStep();
        }
        if(Input.GetKeyDown(KeyCode.Z)) {
            UndoLastAction();
        }
    }

    private void TryStep(Vector3 tapPosition) {
        // tap position doesn't matter atm
        if(invalidStep) {
            UndoLastAction();
        } else {
            TryStep();
        }
    }

    private void UndoLastAction() {
        if(playerActions.Count > 0) {
            playerActions.Pop().Undo();
        }
        invalidStep = false;
    }

    private void TryStep() {
        Tile carCurrentTile = grid[car.Index.x, car.Index.y];

        foreach(Tile neighbor in GetConnectedNeighbors(carCurrentTile)) {
            if(!neighbor.IsLocked) {
                car.Step(neighbor.Index);
                neighbor.Lock();
                return;
            }
        }
        // handle invalid movement here.
        SFXManager.Play("InvalidStep", audioSource);
        invalidStep = true;

    }

    private Tile RenderTile(int col, int row, float cellSize, bool isStraight, Orientation startingOrientation) {
        Vector3 center = CalculateGamePosition(col, row, innerBoardBounds);
        Tile tile = (Instantiate(tilePrefab, center, Quaternion.identity) as Tile);
        tile.Initialize(cellSize, isStraight, new Vector2Int(col, row), outerBoardBounds.center.z, startingOrientation);
        tile.transform.parent = transform;
        return tile;
    }

    private HashSet<Tile> GetConnectedNeighbors(Tile tile) {
        int column = tile.Index.x;
        int row = tile.Index.y;
        HashSet<Tile> neighbors = new HashSet<Tile>();

        if(column - 1 >= 0) {
            //Check neighbor to the left
            Tile leftNeighbor = grid[column - 1, row];
            if(tile.GetEntries().Contains(Tile.TileEntry.Left) && leftNeighbor.GetEntries().Contains(Tile.TileEntry.Right)) {
                neighbors.Add(leftNeighbor);
            }
        }
        if(column + 1 < numberOfColumns) {
            //Check neighbor to the right
            Tile rightNeighbor = grid[column + 1, row];
            if(tile.GetEntries().Contains(Tile.TileEntry.Right) && rightNeighbor.GetEntries().Contains(Tile.TileEntry.Left)) {
                neighbors.Add(rightNeighbor);
            }
        }
        if(row - 1 >= 0) {
            //Check neighbor to the bottom
            Tile bottomNeighbor = grid[column, row - 1];
            if(tile.GetEntries().Contains(Tile.TileEntry.Bottom) && bottomNeighbor.GetEntries().Contains(Tile.TileEntry.Top)) {
                neighbors.Add(bottomNeighbor);
            }
        }            
        if(row + 1 < numberOfRows) {
            //Check neighbor to the top
            Tile topNeighbor = grid[column, row + 1];
            if(tile.GetEntries().Contains(Tile.TileEntry.Top) && topNeighbor.GetEntries().Contains(Tile.TileEntry.Bottom)) {
                neighbors.Add(topNeighbor);
            }
        }
        return neighbors;
    }

    public void HandleSwipe(SwipeInfo swipe)
    {
        if (Busy)
        {
            return;
        }
        if(swipe.Direction == SwipeInfo.SwipeDirection.UP) {
            numberOfColumns++;
            numberOfRows++;
            RenderBoard();
            return;
        } else if (swipe.Direction == SwipeInfo.SwipeDirection.DOWN) {
            numberOfColumns--;
            numberOfRows--;
            RenderBoard();
            return;
        }

        if(!Busy) {
            Rotate(swipe.Direction);
        }
    }

    // public void LockTile(Vector2 index) {
    //     grid[(int)index.x, (int)index.y].Lock();
    // }

    public Vector3 GetPosition(int column, int row) {
        return grid[column,row].Position;
    }

    public Vector3 GetPosition(Vector2Int index) {
        return GetPosition(index.x, index.y);
    }

    public Vector3 GetScreenPosition(int column, int row) {
        return cam.WorldToScreenPoint(grid[column,row].Position);
    }

    public Vector3 GetScreenPosition(Vector2Int index) {
        return GetScreenPosition(index.x, index.y);
    }

    private Vector3 CalculateGamePosition(int column, int row, Bounds bounds) {
            float xPosition = CalculateCoordinate(bounds.min.x, bounds.max.x, numberOfColumns, column);
            float yPosition = CalculateCoordinate(bounds.min.y, bounds.max.y, numberOfRows, row);
            return new Vector3(xPosition, yPosition, bounds.center.z);
    }

    private float CalculateCoordinate(float minimum, float maximum, float totalNumber, float i) {
        float dimensionLength = maximum - minimum; // total grid dimensionLength
        float cellLength = dimensionLength / totalNumber;
        float cellCenter = minimum + cellLength / 2;
        return cellCenter + cellLength * i;
    }
    
    private void OnDrawGizmos() {
        // if(grid != null) {
        //     Gizmos.color = new Color(.8f, 0, 0, .5f);
        //     for(int col = 0; col < numberOfColumns; col++) {
        //         for(int row = 0; row < numberOfRows; row++) {
        //             if(grid[col,row] == null) {
        //                 Gizmos.DrawCube(CalculateGamePosition(col, row, innerBoardBounds), new Vector3(cellWidth, cellHeight, 1));
        //             }
        //         }
        //     }
        // }
        // if(outerBoardBounds != null) {
        //     Gizmos.color = new Color(.7f, 0f, .7f, .3f);

        //     Gizmos.DrawCube(outerBoardBounds.center, new Vector3(outerBoardBounds.max.x - outerBoardBounds.min.x, outerBoardBounds.max.y - outerBoardBounds.min.y, 1));

        //     Gizmos.color = new Color(0f, .7f, .7f, .3f);

        //     Gizmos.DrawCube(innerBoardBounds.center, new Vector3(innerBoardBounds.max.x - innerBoardBounds.min.x, innerBoardBounds.max.y - innerBoardBounds.min.y, 1));
        // }

        if(grid == null) {
            return;
        }

        for(int i = 0; i < numberOfColumns; i++) {
            for(int j = 0; j < numberOfRows; j++) {
                Tile tile = grid[i,j];
                foreach(Tile neighbor in GetConnectedNeighbors(tile)) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(tile.Position + (neighbor.Position - tile.Position)/2, new Vector3(cellWidth/2,cellHeight/2,1));
                }
            }
        }
    }

}
