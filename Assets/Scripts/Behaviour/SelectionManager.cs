using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;

    public bool drawDebug;

    public Action<MapRaycastHit> OnClick;
    public Action<EntityBehaviour> OnAttack;
    public Action<EntityBehaviour> OnEntitySelect;
    public Action<MapRaycastHit> OnHoveredTileChanged;
    public Action<EntityBehaviour> OnHoveredEntityChanged;
    public Action OnCancel;

    private void Awake()
    {
        Instance = this;
    }

    MapRaycastHit mapRaycastLastFrame;
    EntityBehaviour entityHoveredLastFrame;

    void Update()
    {
        MapRaycastHit mapRaycastThisFrame = SelectionUtils.MapRaycast();
        if (mapRaycastLastFrame.tile != mapRaycastThisFrame.tile)
        {
            OnHoveredTileChanged?.Invoke(mapRaycastThisFrame);
        }
        mapRaycastLastFrame = mapRaycastThisFrame;

        EntityBehaviour entityUnderCursor = mapRaycastThisFrame.tile == null ? null : 
            mapRaycastThisFrame.tile.entities.Count <= 0 ? null : mapRaycastThisFrame.tile.entities[0];

        EntityBehaviour entityHoveredThisFrame = entityUnderCursor;
        if (entityHoveredThisFrame != entityHoveredLastFrame)
        {
            OnHoveredEntityChanged?.Invoke(entityHoveredThisFrame);
        }
        entityHoveredLastFrame = entityHoveredThisFrame;

        if (Input.GetMouseButtonDown(0))
        {
            OnClick?.Invoke(SelectionUtils.MapRaycast());

            if (entityUnderCursor != null)
            {
                OnEntitySelect?.Invoke(entityUnderCursor);
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            OnCancel?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        DebugUtils.DrawTile(SelectionUtils.MapRaycast().position, new Color(.2f,.2f,.5f,.5f));
    }
}
