using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;

    public bool drawDebug;

    public EntityBehaviour selectedEntity;

    public Action<MapRaycastHit> OnClick;

    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnClick?.Invoke(SelectionUtils.MapRaycast());
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        DebugUtils.DrawTile(SelectionUtils.MapRaycast().position, new Color(.2f,.2f,.5f,.5f));
    }
}
