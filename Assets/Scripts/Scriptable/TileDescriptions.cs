using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="TileDescriptions", menuName ="ScriptableObjects/Singleton/TileDescriptions")]
public class TileDescriptions : ScriptableObject
{
    public string[] tileNames;
    public string[] tileEffects;
    public Sprite[] tileSprites;
}
