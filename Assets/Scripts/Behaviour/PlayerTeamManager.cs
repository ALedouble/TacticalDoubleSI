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
    [HideInInspector] public int teamXp = 0;

    public List<EntityBehaviour> playerEntitybehaviours = new List<EntityBehaviour>();

    private void Awake()
    {
        Instance = this;

        for (int i = 0; i < playerEntities.Count; i++)
        {
            playerEntities[i] = Instantiate(playerEntities[i]);

            playerEntities[i].abilities.Clear();
            for (int j = 0; j < 4; j++)
            {
                playerEntities[i].abilityLevels.Add(0);

                playerEntities[i].abilities.Add(playerProgression[i].abilityProgression[j].abilities[0]);
            }
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

    public void LevelUpPlayerEntity(Entity entity)
    {
        if (entity.alignement != Alignement.Player)
        {
            Debug.LogError("Can't level up a enemy or neutral entity");
        }


        int index = playerEntities.FindIndex(x => x.displayName == entity.displayName);
        Debug.Log(index);
        entity.maxActionPoints += Mathf.Ceil(playerProgression[index].actionPointsIncrement);
        entity.maxHealth += Mathf.Ceil(playerProgression[index].healthIncrement);
        entity.armor += Mathf.Ceil(entity.armor);

        entity.power++;
    }

    public void LevelUpPlayerAbility(Entity entity, int totemValue)
    {
        if (entity.alignement != Alignement.Player)
        {
            Debug.LogError("Can't level up a enemy or neutral entity");
        }

        int index = playerEntities.FindIndex(x => x.displayName == entity.displayName);

        int abilityNumber = totemValue;

        entity.abilityLevels[abilityNumber]++;

        entity.abilities[abilityNumber] = playerProgression[index].abilityProgression[abilityNumber].abilities[entity.abilityLevels[abilityNumber]];
    }
}
