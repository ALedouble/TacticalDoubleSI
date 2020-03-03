using System.Collections.Generic;
using UnityEngine;

struct EntityPosition
{
    Entity entity;
    Vector2 position;
}

[CreateAssetMenu(fileName = "Map", menuName = "ScriptableObjects/Map", order = 1)]
public class Map : ScriptableObject
{
    List<List<TileData>> map { get; set; }

    List<EntityPosition> listEntityPosition;

    int heigth;
    int width;
}
