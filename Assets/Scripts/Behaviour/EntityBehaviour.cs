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

    public TileData currentTile;

    bool channelingBurst;
    public bool IsChannelingBurst { get => channelingBurst; }

    int currentHealth;
    public int CurrentHealth { get => currentHealth; }

    int currentActionPoints;
    public int CurrentActionPoints { get => currentActionPoints; }

    int currentArmor;
    public int CurrentArmor { get => currentArmor; }

    List<Vector2Int> tilesForCast;

    Vector2Int castCase;

    List<Vector2Int> tilesForEffect;

    public void OnTurn()
    {
        data.brain.OnTurnStart(this);
        GetTileForCast(data.abilities[0].castArea);
    }

    public Sequence MoveTo(ReachableTile reachableTile)
    {
        Sequence moveSequence = DOTween.Sequence();
        Ease movementEase = Ease.InOutSine;

        // TODO : put in scriptableObject
        float tileMovementSpeed = .25f;

        for (int i = 0; i < reachableTile.path.Count; i++)
        {
            moveSequence.Append(transform.DOMove(new Vector3(reachableTile.path[i].Position.x, 0, reachableTile.path[i].Position.y), tileMovementSpeed)
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

            abilitySequence.Append(transform.DOMove(new Vector3(transform.position.x , transform.position.y + 0.5f, transform.position.z), .25f)
            .SetEase(healEase, 2)
            .OnComplete(() =>
            {
                tilesForCast.Clear();
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
            if (SelectionUtils.MapRaycast().position == tilesForCast[i])
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
        List<Vector2Int> tileNoRelative = area.area;
        
        for(int i = 0; i < tilesForCast.Count; i++)
        {
           TileData tile = MapManager.GetTile(tilesForCast[i] + currentTile.position);
            if (tile != null)
            {
                tile.color = Color.blue;
            }
        }
        return tilesForCast;
    }

    public List<Vector2Int> GetTileForEffect(TileArea area)
    {
        tilesForEffect = area.RelativeArea();
        List<Vector2Int> tileNoRelative = area.area;

        for (int i = 0; i < tilesForEffect.Count; i++)
        {

          
            
            TileData tile = MapManager.GetTile(castCase + tilesForEffect[i]);

            if (tile != null)
            {
                tile.color = Color.red;
                Debug.Log(tilesForEffect[i]);
            }
        }
        
        return tilesForEffect;
    }

    /*
    public Vector2 RoundVector(Vector2 vectorToRound)
    {
        
    }
    */
}
