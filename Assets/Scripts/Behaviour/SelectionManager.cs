using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public bool drawDebug;

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                DebugUtils.DrawTile(new Vector2Int(x,y));
            }
        }

        DebugUtils.DrawTile(SelectionUtils.MapRaycast().position, new Color(.2f,.2f,.5f,.5f));
    }
}
