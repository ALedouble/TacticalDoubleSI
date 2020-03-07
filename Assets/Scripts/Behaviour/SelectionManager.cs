using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;

    public bool drawDebug;

    public Action<MapRaycastHit> OnClick;
    public Action<EntityBehaviour> OnEntitySelect;

    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (RaycastForEntities()) return;

            OnClick?.Invoke(SelectionUtils.MapRaycast());
        }
    }

    bool RaycastForEntities()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Physics.Raycast(ray, out hit);

        EntityBehaviour entity;

        if (hit.collider == null) return false;

        if (hit.collider.TryGetComponent<EntityBehaviour>(out entity))
        {
            OnEntitySelect?.Invoke(entity);
            return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        DebugUtils.DrawTile(SelectionUtils.MapRaycast().position, new Color(.2f,.2f,.5f,.5f));
    }
}
