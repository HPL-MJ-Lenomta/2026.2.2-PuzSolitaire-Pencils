using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<Tile> AllTiles = new List<Tile>();
    public List<TileCell> AllTileCells = new List<TileCell>();
    public Vector2 size = new Vector2(5,5);
    public Dictionary<Vector2, TileCell> cellLookup = new Dictionary<Vector2, TileCell>();
    private List<TileCell> emptyTileCellList = new List<TileCell>();

    private void Start()
    {
        for (int i = 0; i < size.y; i++)
        {
            for (int j = 0; j < size.x; j++)
            {
                int index = i * (int)size.x + j;
                if(index < AllTiles.Count)
                {
                    AllTileCells[index].SetCellPos(new Vector2(i, j));
                    AllTileCells[index].SetCurrentTile(AllTiles[index]);
                }
            }
        }
        InitializeLookup();
    }

    private void InitializeLookup()
    {
        cellLookup.Clear();
        foreach (var cell in AllTileCells)
        {
            cellLookup[cell.CellPosition] = cell;
        }
    }

    public void OnStartMoving(List<Tile> connectedTiles)
    {
        //empty the cells of the moving tiles
        foreach (var tile in connectedTiles)
        {
            foreach (var cell in AllTileCells)
            {
                if (cell.CurrentTile == tile)
                {
                    cell.SetCurrentTile(null);
                    emptyTileCellList.Add(cell);
                }
            }
        }
    }

    public void OnDropTiles(List<Tile> connectedTiles)
    {
        bool canDrop = true;
        Debug.LogWarning($"Connected tiles count: {connectedTiles.Count}");
        foreach (var tile in connectedTiles)
        {
            if (!tile.IsOverlapping())
            {
                canDrop = false;
                break;
            }
        }

        Debug.LogWarning($"Can drop: {canDrop}");

        List<Tile> tilesToMove = new List<Tile>();

        if (canDrop)
        {
            foreach (var tile in connectedTiles)
            {
                TileCell overlapCell = UIOverlapChecker.GetTileCellUnderRect(tile.GetComponent<RectTransform>());
                if (overlapCell)
                {
                    tilesToMove.Add(overlapCell.GetCurrentTile());
                    overlapCell.SetCurrentTile(tile);
                }
            }

            foreach (var movingTile in tilesToMove)
            {
                if (movingTile != null)
                {
                    //find an empty cell to place the tile
                    foreach (var cell in emptyTileCellList)
                    {
                        if (cell.GetCurrentTile() == null)
                        {
                            cell.SetCurrentTile(movingTile);
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogError(("Cannot drop tiles here! Reverting to original positions."));
            for (int i = 0; i < emptyTileCellList.Count; i++)
            {
                emptyTileCellList[i].SetCurrentTile(connectedTiles[i]);
            }
        }
        emptyTileCellList.Clear();
        TryMergeCorrectAdjacentTiles();

    }

    public void TryMergeCorrectAdjacentTiles()
    {
        foreach (var cell in AllTileCells)
        {
            if (cell.CurrentTile == null) continue;

            Tile referenceTile = cell.CurrentTile;
            Vector2 referenceCorrectPos = referenceTile.GetTileData().GetPos();

            // Get actual neighboring cells in the grid
            Vector2[] neighborCellPositions =
            {
                cell.CellPosition + Vector2.left,
                cell.CellPosition + Vector2.right,
                cell.CellPosition + Vector2.up,
                cell.CellPosition + Vector2.down
            };

            // Get existing neighbor cells
            List<TileCell> neighborCells = AllTileCells
                .Where(c => neighborCellPositions.Contains(c.CellPosition))
                .ToList();

            // Get correctly-placed neighbors
            List<Tile> correctNeighbors = AddNeighbors(referenceTile, neighborCells);

            // Add them to this tile's connected list
            referenceTile.AddConnectedTile(correctNeighbors);

            Debug.Log($"Tile at {cell.CellPosition} now has {referenceTile.connectedTiles.Count} connected tiles.");
        }

        // Check if puzzle is solved
        if (AllTiles.All(t => t.connectedTiles.Count == AllTiles.Count - 1))
        {
            Debug.LogWarning("ALL TILES CONNECTED CORRECTLY! PUZZLE SOLVED!");
        }
    }

    public List<Tile> AddNeighbors(Tile referenceTile, List<TileCell> neighborCells)
    {
        List<Tile> correctNeighbors = new List<Tile>();

        Vector2 refCorrectPos = referenceTile.GetTileData().GetPos();

        Vector2[] expectedNeighborCorrectPositions =
        {
            refCorrectPos + Vector2.left,
            refCorrectPos + Vector2.right,
            refCorrectPos + Vector2.up,
            refCorrectPos + Vector2.down
        };

        foreach (TileCell cell in neighborCells)
        {
            Tile neighborTile = cell.GetCurrentTile();
            if (neighborTile == null) continue;

            Vector2 neighborCorrectPos = neighborTile.GetTileData().GetPos();

            if (expectedNeighborCorrectPositions.Contains(neighborCorrectPos))
            {
                Debug.Log($"Correct neighbor found: {neighborCorrectPos}");
                correctNeighbors.Add(neighborTile);
            }
        }

        return correctNeighbors;
    }


}
