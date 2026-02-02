using DG.Tweening;
using UnityEngine;

public class TileCell : MonoBehaviour
{
    public Tile CurrentTile;
    public Vector2 CellPosition;
    private RectTransform rectTransform;
    public void SetCurrentTile(Tile tile)
    {
        CurrentTile = tile;

        if (CurrentTile == null) return;

        rectTransform = transform as RectTransform;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(CurrentTile.GetRectTransform().DOAnchorPos(rectTransform.anchoredPosition, 0.2f));
    }

    public void SetCellPos(Vector2 pos)
    {
        CellPosition = pos;
    }

    public Vector2 GetCellPos()
    {
        return CellPosition;
    }

    public Tile GetCurrentTile()
    {
         return CurrentTile;
    }
}
