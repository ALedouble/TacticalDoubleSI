using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMapVisualizer : MonoBehaviour
{
    public bool drawDebug;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Color normal = new Color(.9f, .9f, .9f, .1f);
    private Color solid = new Color(.1f, .1f, .1f, .5f);
    private Color fast = new Color(1, .6f, 0.9f, .8f);
    private Color slow = new Color(.5f, 0f, .5f, .4f);
    private Color playerPlacement = new Color(.0f, 0f, .8f, .2f);

    private void OnDrawGizmos()
    {

        if (!drawDebug) return;

        if (Application.isPlaying && MapManager.Instance != null)
        {
            for (int x = 0; x < MapManager.GetSize(); x++)
            {
                for (int y = 0; y < MapManager.GetSize(); y++)
                {
                    DebugUtils.DrawTile(new Vector2Int(x, y), MapManager.GetTile(new Vector2Int(x, y)).TileType == TileType.Normal ? normal :
                        MapManager.GetTile(new Vector2Int(x, y)).TileType == TileType.Solid ? solid :
                        MapManager.GetTile(new Vector2Int(x, y)).TileType == TileType.Fast ? fast : slow);

                }
            }
        }
    }
}
