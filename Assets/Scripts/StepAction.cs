using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileRotation;

public class StepAction : PlayerAction
{
    private Vector2Int originalIndex;

    private BoardManager board;
    public StepAction(BoardManager board, Vector2Int originalIndex) {
        this.board = board;
        this.originalIndex = originalIndex;
    }

    public override bool Undo() {
        return board.UndoStep(originalIndex);
    }

}
