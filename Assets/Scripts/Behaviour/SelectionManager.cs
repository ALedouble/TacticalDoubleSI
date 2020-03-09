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

        EntityBehaviour entityHoveredThisFrame = EntityUnderCursor();
        if (entityHoveredThisFrame != entityHoveredLastFrame)
        {
            OnHoveredEntityChanged?.Invoke(entityHoveredThisFrame);
        }
        entityHoveredLastFrame = entityHoveredThisFrame;

        if (Input.GetMouseButtonDown(0))
        {
            EntityBehaviour entityUnderCursor = EntityUnderCursor();
            if (entityUnderCursor != null)
            {
                OnEntitySelect?.Invoke(entityUnderCursor);
                return;
            }

            OnClick?.Invoke(SelectionUtils.MapRaycast());
        }
        if (Input.GetMouseButtonDown(1))
        {
            OnCancel?.Invoke();
        }
    }

    EntityBehaviour EntityUnderCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Physics.Raycast(ray, out hit);

        EntityBehaviour entity;

        if (hit.collider == null) return null;

        if (hit.collider.TryGetComponent<EntityBehaviour>(out entity))
        {
            return entity;
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        DebugUtils.DrawTile(SelectionUtils.MapRaycast().position, new Color(.2f,.2f,.5f,.5f));
    }
}
