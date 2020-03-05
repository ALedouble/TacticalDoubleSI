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

    private void OnDrawGizmos()
    {

        if (!drawDebug) return;

        if (Application.isPlaying && MapManager.Instance != null)
        {
            for (int x = 0; x < MapManager.GetSize(); x++)
            {
                for (int y = 0; y < MapManager.GetSize(); y++)
                {
                    DebugUtils.DrawTile(new Vector2Int(x, y), new Color(.9f, .9f, .9f, .5f));

                }
            }
        }
    }
}
