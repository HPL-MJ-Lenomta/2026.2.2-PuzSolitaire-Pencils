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


    /*public void OnDropTiles(List<Tile> movingCluster)
    {
        List<TileCell> targetCells = new List<TileCell>();
        HashSet<Tile> clustersToDisplace = new HashSet<Tile>();

        // 1. Check if the drop is valid (all tiles in cluster must hit a cell)
        foreach (var tile in movingCluster)
        {
            TileCell cell = UIOverlapChecker.GetTileCellUnderRect(tile.GetRectTransform());
            if (cell == null)
            {
                ResetCluster(movingCluster);
                return;
            }
            targetCells.Add(cell);

            if (cell.CurrentTile != null && !movingCluster.Contains(cell.CurrentTile))
            {
                // Collect all tiles from the clusters we are about to bump out
                foreach (var displacedTile in cell.CurrentTile.connectedTiles)
                {
                    clustersToDisplace.Add(displacedTile);
                }
            }
        }

        // 2. Size Validation: Can only swap if the clusters displaced match the size 
        // of the empty slots available or if they are the same size.
        if (clustersToDisplace.Count > 0 && clustersToDisplace.Count != movingCluster.Count)
        {
            Debug.LogWarning("Cluster size mismatch! Cannot swap.");
            ResetCluster(movingCluster);
            return;
        }

        // 3. Execution: Swap the clusters
        // Temporarily clear the displaced tiles from the grid
        List<TileCell> sourceCells = new List<TileCell>(emptyTileCellList);

        foreach (var displaced in clustersToDisplace)
        {
            TileCell cell = GetCellOfTile(displaced);
            cell.CurrentTile = null;
        }

        // Move the movingCluster into targetCells
        for (int i = 0; i < movingCluster.Count; i++)
        {
            targetCells[i].SetCurrentTile(movingCluster[i]);
        }

        // Move displaced tiles into the old source cells
        int cellIndex = 0;
        foreach (var displaced in clustersToDisplace)
        {
            if (cellIndex < sourceCells.Count)
            {
                sourceCells[cellIndex].SetCurrentTile(displaced);
                cellIndex++;
            }
        }

        emptyTileCellList.Clear();
        TryMergeCorrectAdjacentTiles(); // Check for new connections after drop
    }*/

    private void ResetCluster(List<Tile> cluster)
    {
        for (int i = 0; i < emptyTileCellList.Count; i++)
        {
            emptyTileCellList[i].SetCurrentTile(cluster[i]);
        }
        emptyTileCellList.Clear();
    }

    /*public void TryMergeCorrectAdjacentTiles()
    {
        foreach (var cell in AllTileCells)
        {
            if (cell.CurrentTile != null && cell.CurrentTile.connectedTiles.Count == AllTiles.Count)
            {
                Debug.LogWarning("All tiles connected! Puzzle solved!");
            }

            if (cell.CurrentTile != null)
            {
                Debug.Log($"Tile at cell {cell.CellPosition} has {cell.CurrentTile.connectedTiles.Count} connected tiles.");

                //check left and right
                Vector2 referenceCellPos = cell.GetCellPos();
                Vector2 leftCellPos = referenceCellPos + new Vector2(-1, 0);
                Vector2 rightCellPos = referenceCellPos + new Vector2(1, 0);
                Vector2 upCellPos = referenceCellPos + new Vector2(0,1);
                Vector2 downCellPos = referenceCellPos + new Vector2(0,-1);

                Vector2[] targetPositions = { leftCellPos, rightCellPos, upCellPos, downCellPos };

                List<TileCell> tileCells = AllTileCells
                    .Where(c => targetPositions.Contains(c.CellPosition))
                    .ToList();

                Tile referenceTile = cell.CurrentTile;
                Vector2 referenceCorrectPos = referenceTile.GetTileData().GetPos();

                List<Tile> tiles = AddNeighbors(tileCells, referenceCellPos);

                cell.GetCurrentTile().AddConnectedTile(tiles);
            }
        }
    }

    public List<Tile> AddNeighbors(List<TileCell> cells, Vector2 cellPos)
    {
        List<Tile> tiles = new List<Tile>();
        Vector2 leftCellPos = cellPos + new Vector2(-1, 0);
        Vector2 rightCellPos = cellPos + new Vector2(1, 0);
        Vector2 upCellPos = cellPos + new Vector2(0, 1);
        Vector2 downCellPos = cellPos + new Vector2(0, -1);

        Vector2[] targetPositions = { leftCellPos, rightCellPos, upCellPos, downCellPos };
        Debug.Log(targetPositions);

        foreach (TileCell cell in cells)
        {
            Debug.LogWarning($"Attempting adding tile {cell.GetCurrentTile().GetTileData().GetPos()}");
            if(targetPositions.Contains(cell.GetCurrentTile().GetTileData().GetPos()))
            {
                Debug.LogError($"Adding neighbor tile at cell position: {cell.CellPosition}");
                tiles.Add(cell.GetCurrentTile());
            }
        }

        return tiles;
    }*/

    public void TryMergeCorrectAdjacentTiles()
    {
        // Directional offsets: Right, Left, Up, Down
        Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };

        foreach (var cell in AllTileCells)
        {
            Tile currentTile = cell.GetCurrentTile();
            if (currentTile == null) continue;

            Vector2 currentCorrectPos = currentTile.GetTileData().GetPos();

            foreach (Vector2 dir in directions)
            {
                Vector2 neighborCellPos = cell.CellPosition + dir;

                // O(1) Lookup instead of .Where()
                if (cellLookup.TryGetValue(neighborCellPos, out TileCell neighborCell))
                {
                    Tile neighborTile = neighborCell.GetCurrentTile();
                    if (neighborTile == null) continue;

                    // Check if this neighbor is the logically correct piece for this direction
                    Vector2 neighborCorrectPos = neighborTile.GetTileData().GetPos();

                    if (neighborCorrectPos == currentCorrectPos + dir)
                    {
                        // They match! Add the neighbor. 
                        // Note: Wrap this in a list because your method expects one
                        currentTile.AddConnectedTile(new List<Tile> { neighborTile });
                    }
                }
            }
        }
    }
}
