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
    public Action OnXPChanged;

    private void Awake()
    {
        Instance = this;

        bool firstInit = false;
        if (SaveManager.Instance.SaveEntitiesWin == null || SaveManager.Instance.SaveEntitiesWin.Count <= 0)
        {
            SaveManager.Instance.SaveEntitiesWin = new List<Entity>(playerEntities);
            SaveManager.Instance.SaveEntitiesLose = new List<Entity>(playerEntities);
            firstInit = true;
        }

        for (int i = 0; i < SaveManager.Instance.SaveEntitiesWin.Count; i++)
        {
            SaveManager.Instance.SaveEntitiesLose[i] = SaveManager.Instance.SaveEntitiesWin[i];
            playerEntities[i] = Instantiate(SaveManager.Instance.SaveEntitiesWin[i]);

            Debug.Log(playerEntities[i]);

            if (firstInit)
            {
                playerEntities[i].abilities.Clear();
                for (int j = 0; j < 4; j++)
                {
                    playerEntities[i].abilityLevels.Add(0);

                    playerEntities[i].abilities.Add(playerProgression[i].abilityProgression[j].abilities[0]);
                }
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
        SoundManager.Instance.PlaySound(playerEntities[placedEntities].placedEntitiesSFX.sound, false);
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

    public int GetPlayerIndex(EntityBehaviour entity)
    {
        return playerEntities.FindIndex(x => x.displayName == entity.data.displayName);
    }
}
