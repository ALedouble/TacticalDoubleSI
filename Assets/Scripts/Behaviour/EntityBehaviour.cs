﻿using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Handles Entity <-> World interaction
/// </summary>

[System.Serializable]
public class EntityBehaviour : MonoBehaviour
{
    public Entity data;
    public int GetMaxHealth()
    {
        return data.maxHealth;
    }
    public List<Ability> GetAbilities()
    {
        return data.abilities;
    }
    public Ability GetAbilities(int i)
    {
        return data.abilities[i];
    }
    public Alignement GetAlignement()
    {
        return data.alignement;
    }
    public EntityTag GetEntityTag()
    {
        return data.entityTag;
    }

    public TileData currentTile;
    public Vector2Int GetPosition()
    {
        return currentTile.position;
    }

    bool channelingBurst;
    public bool IsChannelingBurst { get => channelingBurst; set => channelingBurst = value; }

    int currentHealth;
    public int CurrentHealth { get => currentHealth; set => currentHealth = value; }

    int currentActionPoints;
    public int CurrentActionPoints { get => currentActionPoints; set => currentActionPoints = value; }

    int currentArmor;
    public int CurrentArmor { get => currentArmor; set => currentArmor = value; }

    Vector2Int effectPosition;


    //For PropertyDrawer
    List<Vector2Int> tilesForCast;
    List<Vector2Int> tilesForEffect;
    Vector2Int castCase;



    [HideInInspector] public int heldCrystalValue = -1;

    public void Init()
    {
        data = Instantiate(data);
        name = data.name;

        // VERY TEMPORARY
        GetComponentInChildren<MeshRenderer>().material.color = data.alignement == Alignement.Enemy ? Color.red : Color.blue;
    }

    private void Update()
    {

    }

    public void OnTurn()
    {
        if (data.brain == null)
        {
            Debug.LogError("Entity " + name + " has no brain, please add a brain to its entity asset", this.gameObject);
            Debug.Break();
        }
        else
        {
            data.brain.OnTurnStart(this);
        }
    }

    public Sequence MoveTo(ReachableTile reachableTile)
    {
        currentTile = MapManager.MoveEntity(this, currentTile.position, reachableTile.GetCoordPosition());
        CurrentActionPoints -= reachableTile.cost;

        Sequence moveSequence = DOTween.Sequence();
        Ease movementEase = Ease.InOutSine;

        // TODO : put in scriptableObject
        float tileMovementSpeed = .25f;

        for (int i = 0; i < reachableTile.path.Count; i++)
        {
            moveSequence.Append(transform.DOMove(new Vector3(reachableTile.path[i].position.x, 0, reachableTile.path[i].position.y), tileMovementSpeed)
                .SetEase(movementEase));

            // first half of the jump
            moveSequence.Insert(i * tileMovementSpeed, transform.DOMoveY(1, tileMovementSpeed * .5f).SetEase(Ease.OutQuad));
            // second half
            moveSequence.Insert(i * tileMovementSpeed + tileMovementSpeed * .5f, transform.DOMoveY(0, tileMovementSpeed * .5f).SetEase(Ease.InQuad));
        }

        RoundManager.Instance.currentMovementSequence = moveSequence;

        return moveSequence;
    }

    public Sequence UseAbility(Ability ability, TileData targetTile)
    {
        Sequence abilitySequence = DOTween.Sequence();
        if (ability.animationType == AnimationType.Movement)
        {
            Ease attackEase = Ease.InBack;
            Ease returnAttackEase = Ease.InOutExpo;

            abilitySequence.Append(transform.DOMove(new Vector3(transform.position.x + 0.5f, 0, transform.position.z), .25f)
            .SetEase(attackEase, 10)
            .OnComplete(() =>
            {
                PlayEffects(ability.numberOfEffects, targetTile);
            }));

            abilitySequence.Append(transform.DOMove(new Vector3(transform.position.x, 0, transform.position.z), .25f).SetEase(returnAttackEase, 10));
        }

        if (ability.animationType == AnimationType.Jump)
        {
            Ease healEase = Ease.OutQuad;
            Ease returnHealEase = Ease.InQuad;

            abilitySequence.Append(transform.DOMove(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), .25f)
            .SetEase(healEase, 2)
            .OnComplete(() =>
            {
                PlayEffects(ability.numberOfEffects, targetTile);
            }));

            abilitySequence.Append(transform.DOMove(new Vector3(transform.position.x, 0, transform.position.z), .25f).SetEase(returnHealEase, 10));
        }

        return abilitySequence;
    }

    public Sequence PlayEffects(List<AbilityEffect> effects, TileData targetTile)
    {
        Sequence effectSequence = DOTween.Sequence();
        for (int i = 0; i < tilesForCast.Count; i++)
        {
            if (SelectionUtils.MapRaycast().position - currentTile.position == tilesForCast[i])
            {
                castCase = tilesForCast[i];
                GetTileForEffect(data.abilities[0].effectArea);
            }
        }
        return effectSequence;

    }

    public List<Vector2Int> GetTileForCast(TileArea area)
    {
        tilesForCast = area.RelativeArea();

        for (int i = 0; i < tilesForCast.Count; i++)
        {
            TileData tile = MapManager.GetTile(tilesForCast[i] + GetPosition());
            if (tile != null && MapManager.IsInsideMap(tile.position))
            {
                tile.color = Color.blue;
            }
        }

        // remove all tiles outside of the map
        tilesForCast.RemoveAll(x => !MapManager.IsInsideMap(x + GetPosition()));
        return tilesForCast;
    }

    public List<Vector2Int> GetTileForEffect(TileArea area)
    {
        tilesForEffect = area.RelativeArea();
        

        for (int i = 0; i < tilesForEffect.Count; i++)
        {
            if (castCase.x > 0)
            {
                tilesForEffect[i] = new Vector2Int(tilesForEffect[i].y, tilesForEffect[i].x);
            }

            if (castCase.x < 0)
            {
                tilesForEffect[i] = new Vector2Int(tilesForEffect[i].y * -1, tilesForEffect[i].x);
            }

            if (castCase.y < 0)
            {
                tilesForEffect[i] = new Vector2Int(tilesForEffect[i].x , tilesForEffect[i].y * -1);
            }

            

            TileData tile = MapManager.GetTile(castCase + GetPosition());
            effectPosition = tile.position + tilesForEffect[i];
            CheckEntity();

            
        }

        

        return tilesForEffect;
    }

    public void CheckEntity()
    {
        for (int y = 0; y < MapManager.GetListOfEntity().Count; y++)
        {
            if (MapManager.GetListOfEntity()[y].GetPosition() == effectPosition)
            {
                Vector2Int enemyPosition = MapManager.GetListOfEntity()[y].GetPosition();
                EntityBehaviour currentEnemy = MapManager.GetListOfEntity()[y];
                

                Vector2Int pushVector = CombatUtils.PushEffect(enemyPosition, currentTile.position);
                CombatUtils.Push(currentEnemy, pushVector);

                CombatUtils.GetEffect(this);

            }
        }
    }

    public void OnDrawGizmos()
    {

        if (tilesForCast != null)
        {
            for (int i = 0; i < tilesForCast.Count; i++)
            {
                DebugUtils.DrawTile(tilesForCast[i] + GetPosition(), Color.yellow, .5f);
            }
        }

        if (tilesForEffect != null)
        {
            for (int i = 0; i < tilesForEffect.Count; i++)
            {
                TileData tile = MapManager.GetTile(castCase + GetPosition());
                effectPosition = tile.position + tilesForEffect[i];
                DebugUtils.DrawTile(effectPosition, Color.blue, .5f);
            }
        }

    }
}

