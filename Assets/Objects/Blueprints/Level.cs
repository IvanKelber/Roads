using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName="Levels/Level"), Serializable]
public class Level : ScriptableObject
{

    // public Vector2Int startingIndex;

    // public Vector2Int endingIndex;

    // [Range(0,12)]
    // public int numberOfColumns;
    // [Range(0,12)]
    // public int numberOfRows;
    public TileInfo[] levelMatrix;

}
