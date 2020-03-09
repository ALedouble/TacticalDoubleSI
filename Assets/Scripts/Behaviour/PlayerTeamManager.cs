using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerTeamManager : MonoBehaviour
{
    public static PlayerTeamManager Instance;

    public List<EntityProgression> playerProgression = new List<EntityProgression>();
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
        if (placedEntities > playerEntities.Count - 1) return;

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

    void LevelUpPlayerEntity(Entity entity)
    {
        if (entity.alignement != Alignement.Player)
        {
            Debug.LogError("Can't level up a enemy or neutral entity");
        }

        int index = playerEntities.FindIndex(x => x == entity);

        entity.maxActionPoints += playerProgression[index].actionPointsIncrement;
        entity.maxHealth += playerProgression[index].healthIncrement;
        entity.armor += entity.armor;

        entity.power++;
    }
}
