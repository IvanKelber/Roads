using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public Color[] colorsManifest;
    public int numberOfColors;

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

    public Camera cam;

    public Tile[,] grid;

    private float cellWidth;
    private float cellHeight;

    [SerializeField]
    private Tile tilePrefab;

    private Bounds outerBoardBounds;
    private Bounds innerBoardBounds;

    private bool rotatingTiles = false;


    [SerializeField]
    private AudioManager SFXManager;

    private AudioSource audioSource;

    private bool Busy {
        get {
            return rotatingTiles;
        }
    }

    private void Awake()
    {
        Gestures.OnSwipe += HandleSwipe;
        audioSource = GetComponent<AudioSource>();
        numberOfColors = Mathf.Clamp(numberOfColors, 0, colorsManifest.Length);
        RenderBoard();
    }

    private void RenderBoard() {

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
        // The height of the frustum at a given distance (both in world units) can be obtained with the following formula:

        float frustumHeight = 2.0f * cam.farClipPlane/2 * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

    
        float frustumWidth = frustumHeight * cam.aspect;

        float boardHeight = frustumHeight - marginY;
        float boardWidth = frustumWidth - marginX;
        cellWidth = boardWidth/numberOfColumns - cellPadding;
        cellHeight = cellWidth;
        outerBoardBounds = new Bounds(cam.transform.position + Vector3.forward * cam.farClipPlane/2, new Vector3(boardWidth, boardHeight, 0));
        innerBoardBounds = new Bounds(outerBoardBounds.center, new Vector3((cellWidth + cellPadding) * numberOfColumns, cellHeight * numberOfRows, 0));
        transform.position = outerBoardBounds.center;
        grid = InitializeGrid();
        // RandomizeBoard();

    }

    private Tile[,] InitializeGrid() {
        Tile[,] grid = new Tile[numberOfColumns,numberOfRows];
        for(int col = 0; col < numberOfColumns; col++) {
            for(int row = 0; row < numberOfRows; row++) {
                grid[col,row] = RenderTile(col, row, cellWidth, cellHeight, RandomColor());
            }
        }
        return grid;
    }

    private void RandomizeBoard() {
        for(int i = 0; i < numberOfColumns; i++) {
            for(int j = 0; j < numberOfRows; j++) {
                if(grid[i,j] != null) {
                    grid[i,j].SetColor(RandomColor());
                } else {
                    grid[i,j] = RenderTile(i, j, cellWidth, cellHeight, RandomColor());
                }
            }
        }
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

    private IEnumerator RotateStack(List<Tile> tiles, Vector3 center, float degrees, float duration) {

        float startMoving = Time.time;
        float endMoving = startMoving + .15f;
        while(Time.time < endMoving) {
            foreach(Tile tile in tiles) {
                tile.transform.Translate(Vector3.forward * Time.deltaTime * -1/duration);
            }
            yield return null;
        }
        yield return new WaitForSeconds(.1f);

        float totalDegrees = 0;
        float startTime = Time.time;
        float endTime = startTime + duration;
        while(Time.time < endTime || totalDegrees < degrees) {
            foreach(Tile tile in tiles) {
                float rotationThisFrame = Time.deltaTime * degrees / duration;
                totalDegrees += Mathf.Abs(rotationThisFrame);
                tile.transform.RotateAround(center, Vector3.forward, rotationThisFrame);
            }
            yield return null;
        }
        SFXManager.Play("Rotate", audioSource);

        yield return new WaitForSeconds(.1f);
    
        foreach(Tile tile in tiles) {
            tile.transform.rotation = Quaternion.identity;
            tile.Position = CalculateGamePosition(tile.Index.x, tile.Index.y, innerBoardBounds);
        }
        yield return null;
    }

    private IEnumerator RotateTile(Tile tile, float degrees, float duration) {
        List<Tile> tiles = new List<Tile>();
        tiles.Add(tile);
        yield return StartCoroutine(RotateStack(tiles, tile.Position, degrees, duration));
    }

    private IEnumerator RotateTiles(SwipeInfo.SwipeDirection direction) {
        for(int i = 0; i < numberOfColumns; i++) {
            for(int j = 0; j < numberOfRows; j++) {
                StartCoroutine(RotateTile(grid[i,j], direction == SwipeInfo.SwipeDirection.LEFT ? -90 : 90, cellRotationDuration));
            }
        }
        yield return new WaitForSeconds(cellRotationDuration);
        rotatingTiles = false;
    }

    private void Update() {
        // RenderBoard();
        if(Input.GetKeyDown(KeyCode.Space)) {
            RenderBoard();
        }
        if(Input.GetKey(KeyCode.LeftArrow)&& !Busy) {
            rotatingTiles = true;
            StartCoroutine(RotateTiles(SwipeInfo.SwipeDirection.LEFT));
        }
        if(Input.GetKey(KeyCode.RightArrow) && !Busy) {
            rotatingTiles = true;
            StartCoroutine(RotateTiles(SwipeInfo.SwipeDirection.RIGHT));
        }
    }

    private Tile RenderTile(int col, int row, float cellWidth, float cellHeight, Color color) {
        Vector3 center = CalculateGamePosition(col, row, innerBoardBounds);
        Tile tile = (Instantiate(tilePrefab, center, Quaternion.identity) as Tile);
        tile.Initialize(cellWidth, cellHeight, color, new Vector2Int(col, row), outerBoardBounds.center.z);
        tile.transform.parent = transform;
        return tile;
    }

    private Color RandomColor() {
        return colorsManifest[Random.Range(0, numberOfColors)];
    }

    public void HandleSwipe(SwipeInfo swipe)
    {
        if (Busy ||
           swipe.Direction == SwipeInfo.SwipeDirection.UP || swipe.Direction == SwipeInfo.SwipeDirection.DOWN)
        {
            return;
        }

        if(!Busy) {
            rotatingTiles = true;
            StartCoroutine(RotateTiles(swipe.Direction));
        }
    }

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
        if(grid != null) {
            Gizmos.color = new Color(.8f, 0, 0, .5f);
            for(int col = 0; col < numberOfColumns; col++) {
                for(int row = 0; row < numberOfRows; row++) {
                    if(grid[col,row] == null) {
                        Gizmos.DrawCube(CalculateGamePosition(col, row, innerBoardBounds), new Vector3(cellWidth, cellHeight, 1));
                    }
                }
            }
        }
        if(outerBoardBounds != null) {
            Gizmos.color = new Color(.7f, 0f, .7f, .3f);

            Gizmos.DrawCube(outerBoardBounds.center, new Vector3(outerBoardBounds.max.x - outerBoardBounds.min.x, outerBoardBounds.max.y - outerBoardBounds.min.y, 1));

            Gizmos.color = new Color(0f, .7f, .7f, .3f);

            Gizmos.DrawCube(innerBoardBounds.center, new Vector3(innerBoardBounds.max.x - innerBoardBounds.min.x, innerBoardBounds.max.y - innerBoardBounds.min.y, 1));
        }
    }

}
