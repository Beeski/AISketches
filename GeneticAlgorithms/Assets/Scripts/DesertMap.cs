using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

public class DesertMap : MonoBehaviour
{
    [Flags]
    private enum Tile
    {
        Unknown = 0,

        Desert_1_0 = 1,
        Desert_1_1 = 2,
        Desert_1_2 = 4,
        Desert_1_3 = 8,
        Desert_1_4 = 16,
        Desert_1_5 = 32,
        Desert_1_6 = 64,
        Desert_1_7 = 128,
        Desert_1_8 = 256,

        Accessible = Desert_1_0 | Desert_1_1 | Desert_1_2 | Desert_1_4 | Desert_1_5,
        NonAccessible = Desert_1_3 | Desert_1_6 | Desert_1_7 | Desert_1_8,
    }

    [SerializeField] private Vector3Int Start;
    [SerializeField] private Vector3Int Target;

    private Tile[,] mMap;
    private bool[,] mRoute;
    private Tilemap mTilemap;
    private Vector3Int mOrigin;
    private Vector3Int mSize;

    private void Awake()
    {
        mTilemap = GetComponentInChildren<Tilemap>();

        if (mTilemap != null)
        {
            Vector3Int position = new Vector3Int();
            mOrigin = mTilemap.origin;
            mSize = mTilemap.size;

            mMap = new Tile[mSize.x, mSize.y];
            mRoute = new bool[mSize.x, mSize.y];

            for (int x = 0; x < mSize.x; ++x)
            {
                position.x = mOrigin.x + x;
                for (int y = 0; y < mSize.y; ++y)
                {
                    position.y = mOrigin.y + y;
                    TileBase tile = mTilemap.GetTile(position);
                    if (tile != null)
                    {
                        Tile result = Tile.Unknown;
                        if (Enum.TryParse(tile.name, out result))
                        {
                            mMap[x, y] = result;
                        }
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (mTilemap != null)
        {
            Vector3Int position = Start;
            Vector3 cellPosition = mTilemap.CellToWorld(position) + mTilemap.cellSize * 0.5f;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(cellPosition, 0.1f);

            position = Target;
            cellPosition = mTilemap.CellToWorld(position) + mTilemap.cellSize * 0.5f;
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(cellPosition, 0.1f);

            for (int x = 0; x < mSize.x; ++x)
            {
                position.x = mOrigin.x + x;
                for (int y = 0; y < mSize.y; ++y)
                {
                    position.y = mOrigin.y + y;
                    if (mRoute[x, y])
                    {
                        cellPosition = mTilemap.CellToWorld(position) + mTilemap.cellSize * 0.5f;
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(cellPosition, 0.1f);
                    }
                }
            }
        }
    }

    private void ClearRoute()
    {
        for (int x = 0; x < mSize.x; ++x)
        {
            for (int y = 0; y < mSize.y; ++y)
            {
                mRoute[x, y] = false;
            }
        }
    }

    private bool IsAccessible(Tile tile)
    {
        return (Tile.Accessible & tile) == tile;
    }

    private Vector3Int UpdatePosition(Vector3Int position, Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                position.y += 1;
                break;
            case Direction.East:
                position.x += 1;
                break;
            case Direction.South:
                position.y -= 1;
                break;
            case Direction.West:
                position.x -= 1;
                break;
        }

        return position;
    }

    public double TestRoute(List<Direction> directions)
    {
        ClearRoute();
        Vector3Int position = Start - mOrigin;
        int steps = 0;
        int penalty = 1;

        mRoute[position.x, position.y] = true;

        for (int count = 0; count < directions.Count; ++count, ++steps)
        {
            Vector3Int updatedPosition = UpdatePosition(position, directions[count]);

            if (updatedPosition.x >= 0 && updatedPosition.x < mSize.x && updatedPosition.y >= 0 && updatedPosition.y < mSize.y)
            {
                Tile potentialTile = mMap[updatedPosition.x, updatedPosition.y];
                if (IsAccessible(potentialTile))
                {
                    position = updatedPosition;

                    if(mRoute[position.x, position.y])
                    {
                        ++penalty;
                    }

                    mRoute[position.x, position.y] = true;

                    if( position == (Target - mOrigin))
                    {
                        break;
                    }
                }
            }
        }

        Vector3Int diff = position - (Target - mOrigin);
        return 1 / (double)(Mathf.Abs(diff.x) + Mathf.Abs(diff.y) + penalty + steps);
    }
}
