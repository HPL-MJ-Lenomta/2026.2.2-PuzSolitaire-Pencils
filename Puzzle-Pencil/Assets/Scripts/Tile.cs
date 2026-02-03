using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    public TileData tileData;
    public Image fullTileImage;
    public Image borderTileImage;

    private bool _isConnected = false;
    private Vector2 startPosition;
    private RectTransform rectTransform;
    private Canvas canvas;

    public List<Tile> connectedTiles = new List<Tile>();

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        canvas = GetComponentInParent<Canvas>();
        startPosition = rectTransform.anchoredPosition;

        if(!connectedTiles.Contains(this))
        {
            connectedTiles.Add(this);
        }
    }

    private void OnValidate()
    {
        SetTileData(tileData);
    }

    public Vector2 GetInitialPos()
    {
        return startPosition;
    }

    public TileData GetTileData()
    {
        return tileData;
    }

    public void ChangePosition(Vector2 pos)
    {
        rectTransform.anchoredPosition = pos;
        startPosition = pos;
    }

    public void SetTileData(TileData data)
    {
        if(data == null) return;
        tileData = data;
        fullTileImage.sprite = tileData.tileSprite;
        borderTileImage.sprite = tileData.borderSprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        foreach (var tile in connectedTiles)
        {
            tile.transform.SetAsLastSibling();
        }
        TileManager.Instance.OnStartMoving(connectedTiles);
    }

    public void OnDrag(PointerEventData eventData)
    {
        foreach(var tile in connectedTiles)
        {
            tile.MoveTile(eventData);
        }
    }

    public void MoveTile(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        TileManager.Instance.OnDropTiles(connectedTiles);
    }

    public bool IsOverlapping()
    {
        TileCell overlapObject = UIOverlapChecker.GetTileCellUnderRect(rectTransform);
        return overlapObject != null;
    }

    public void AddConnectedTile(List<Tile> tiles)
    {
        foreach(var tile in tiles)
        {
            if (!connectedTiles.Contains(tile))
            {
                connectedTiles.Add(tile);
            }
        }
        HashSet<Tile> uniqueTiles = new HashSet<Tile>(connectedTiles);
        foreach (var tile in connectedTiles)
        {
            foreach(var connected in tile.connectedTiles)
            {
                uniqueTiles.UnionWith(connected.connectedTiles);
            }
        }
        connectedTiles = new List<Tile>(uniqueTiles);
    }

    public RectTransform GetRectTransform()
    {
        if(rectTransform == null)
        {
            rectTransform = transform as RectTransform;
        }
        return rectTransform;
    }
}
