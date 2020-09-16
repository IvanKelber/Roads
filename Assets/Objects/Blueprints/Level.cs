using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName="Levels/Level"), Serializable]
public class Level : ScriptableObject
{

    public Vector2Int startingIndex;

    public Vector2Int endingIndex;

    [Range(0,12)]
    public int numberOfColumns;
    [Range(0,12)]
    public int numberOfRows;

    public TileMatrix levelMatrix;

    [Serializable]
    public class TileMatrix {
        public int numberOfColumns;
        public int numberOfRows;

        public TileRow[] rows;

        // Define the indexer to allow client code to use [] notation.
        public TileRow this[int i]
        {
            get { return rows[i]; }
            set { rows[i] = value; }
        }

        public TileMatrix(int numberOfColumns, int numberOfRows) {
            this.numberOfColumns = numberOfColumns;
            this.numberOfRows = numberOfRows;

            rows = new TileRow[numberOfRows];
            for(int i = 0; i < numberOfRows; i++) {
                rows[i] = new TileRow(numberOfColumns);
            }
        }
    }
    [Serializable]
    public class TileRow {
        public TileInfo[] row;

                // Define the indexer to allow client code to use [] notation.
        public TileInfo this[int i]
        {
            get { return row[i]; }
            set { row[i] = value; }
        }

        public TileRow(int numberOfColumns) {
            row = new TileInfo[numberOfColumns];
        }
    }

}
