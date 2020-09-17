using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public Vector2Int Index;
    public BoardManager board;
    private void Start() {
        SetScale();
    }

    public void Initialize(Vector2Int startingIndex) {
        Debug.Log("initializing car at " + startingIndex);
        Index = startingIndex;
        board.grid[startingIndex.x, startingIndex.y].Lock();
        SetPosition();
    }

    private void SetPosition() {
        Vector3 indexLocation = board.GetPosition(Index);
        transform.position = new Vector3(indexLocation.x, indexLocation.y, indexLocation.z - 1);
    }

    private void SetScale() {
        transform.localScale = new Vector3(board.cellWidth, board.cellWidth, 0);
    }

    public void Step(Vector2Int newIndex) {
        Index = newIndex;
        SetPosition();
    }

}
