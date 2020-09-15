using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public Vector2Int Index;
    public BoardManager board;
    private void Start() {
        Index = new Vector2Int(0,0);
        board.grid[0,0].Lock();
        SetPosition();
        SetScale();
    }

    private void SetPosition() {
        Vector3 indexLocation = board.GetPosition(Index);
        transform.position = new Vector3(indexLocation.x, indexLocation.y, indexLocation.z - 1);
    }

    private void SetScale() {
        transform.localScale = new Vector3(board.cellWidth/2, board.cellWidth/2, 0);
    }

    public void Step(Vector2Int newIndex) {
        Index = newIndex;
        SetPosition();
    }

}
