using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileRotation;

public class RotateAction : PlayerAction
{
    private float degreesRotated;

    private BoardManager board;
    public RotateAction(BoardManager board, float degreesRotated) {
        this.board = board;
        this.degreesRotated = degreesRotated;
    }

    public override bool Undo() {
        board.Rotate(-degreesRotated);
        return true;
    }

}
