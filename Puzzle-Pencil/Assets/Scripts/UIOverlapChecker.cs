using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UIOverlapChecker
{
    public static GameObject GetUIUnderRect(RectTransform rect)
    {
        PointerEventData data = new PointerEventData(EventSystem.current);

        // Use the center of your RectTransform
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rect.position);
        data.position = screenPoint;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (var r in results)
        {
            if (r.gameObject == rect.gameObject) continue;

            Tile tile = r.gameObject.GetComponentInParent<Tile>();

            if (tile.gameObject != rect.gameObject)
                return tile.gameObject;
        }

        return null;
    }

    public static Tile GetTileUnderRect(RectTransform rect)
    {
        PointerEventData data = new PointerEventData(EventSystem.current);

        // Use the center of your RectTransform
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rect.position);
        data.position = screenPoint;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (var r in results)
        {
            if (r.gameObject == rect.gameObject) continue;

            Tile tile = r.gameObject.GetComponentInParent<Tile>();

            if (tile.gameObject != rect.gameObject)
                return tile;
        }

        return null;
    }

    public static TileCell GetTileCellUnderRect(RectTransform rect)
    {
        PointerEventData data = new PointerEventData(EventSystem.current);

        // Use the center of your RectTransform
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rect.position);
        data.position = screenPoint;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (var r in results)
        {
            if (r.gameObject == rect.gameObject) continue;

            TileCell tileCell = r.gameObject.GetComponentInParent<TileCell>();
            if(tileCell == null)
            {
                continue;
            }
            if (tileCell.gameObject != rect.gameObject)
                return tileCell;
        }

        return null;
    }
}
