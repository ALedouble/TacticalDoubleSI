using UnityEngine;
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

    private void Start()
    {
        
    }

    public void OnTurn()
    {
        data.brain.OnTurnStart(this);
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
        Ease attackEase = Ease.InBack;
        Ease returnAttackEase = Ease.InOutExpo;

        abilitySequence.Append(transform.DOMove(new Vector3(transform.position.x + 0.5f, 0, transform.position.z), .25f)
            .SetEase(attackEase, 10)
            .OnComplete(() => 
            {
                PlayEffects(ability.numberOfEffects, targetTile);
            }));
            
        abilitySequence.Append(transform.DOMove(new Vector3(transform.position.x, 0, transform.position.z), .25f).SetEase(returnAttackEase, 10));

        return abilitySequence;
    }

    public Sequence PlayEffects(List<AbilityEffect> effects, TileData targetTile)
    {
        Sequence effectSequence = DOTween.Sequence();
        return null;
    }
}
