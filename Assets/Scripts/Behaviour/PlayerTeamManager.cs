using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerTeamManager : MonoBehaviour
{
    public static PlayerTeamManager Instance;

    public List<Entity> playerEntities = new List<Entity>();
    private int placedEntities = 0;

    public List<EntityBehaviour> playerEntitybehaviours = new List<EntityBehaviour>();

    private void Awake()
    {
        Instance = this;

        for (int i = 0; i < playerEntities.Count; i++)
        {
            playerEntities[i] = Instantiate(playerEntities[i]);
        }
    }

    public void BeginPlacement()
    {
        SelectionManager.Instance.OnClick += PlacePlayerEntity;

        HUDManager.Instance.OnFinishPlacement += OnPressedFinishedPlacement;
    }

    public Action OnFinishPlacement;
    public Action OnPlacedAllPlayers;

    void PlacePlayerEntity(MapRaycastHit hit)
    {
        if (hit.tile == null) return;
        if (!hit.tile.canPlacePlayerEntity) return;
        if (hit.tile.entities.Count > 0) return;

        GameObject entityPrefab = Resources.Load("Entity") as GameObject;

        EntityBehaviour entityBehaviour = MapManager.SpawnEntity(playerEntities[placedEntities], hit.position, -1);

        playerEntitybehaviours.Add(entityBehaviour);

        placedEntities++;

        if (placedEntities == playerEntities.Count)
        {
            OnPlacedAllPlayers?.Invoke();
        }
    }

    void OnPressedFinishedPlacement()
    {
        if (placedEntities >= playerEntities.Count)
        {
            OnFinishPlacement?.Invoke();

            HUDManager.Instance.OnFinishPlacement -= OnPressedFinishedPlacement;
            SelectionManager.Instance.OnClick -= PlacePlayerEntity;
        }
    }
}
