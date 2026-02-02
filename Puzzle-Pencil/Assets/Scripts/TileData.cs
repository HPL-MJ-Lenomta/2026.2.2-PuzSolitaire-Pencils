using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "ScriptableObjects/TileData", order = 1)]
public class TileData : ScriptableObject
{
    public Sprite tileSprite;
    public Sprite borderSprite;
    public Vector2 correctPos;
    public Vector2 GetPos() { return correctPos; }
}
