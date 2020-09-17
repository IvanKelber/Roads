using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileRotation;

public class CompositePlayerAction : PlayerAction
{
    // If a series of actions are done in one step they should be undone in one step

    private Stack<PlayerAction> playerActions;

    public bool Empty {
        get {
            return playerActions == null || playerActions.Count > 0;
        }
    }

    public CompositePlayerAction() {
        playerActions = new Stack<PlayerAction>();
    }

    public void AddAction(PlayerAction action) {
        playerActions.Push(action);
    }

    public override bool Undo() {
        foreach(PlayerAction playerAction in playerActions) {
            playerAction.Undo();
        }
        return true;
    }


}
