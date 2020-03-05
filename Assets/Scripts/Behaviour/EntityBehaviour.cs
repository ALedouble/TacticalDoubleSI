using UnityEngine;
using System;
using DG.Tweening;

/// <summary>
/// Handles Entity <-> World interaction
/// </summary>

[System.Serializable]
public class EntityBehaviour : MonoBehaviour
{
    public Entity data;

    public TileData currentTile;

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

    public Sequence UseAbility()
    {
        Sequence abilitySequence = DOTween.Sequence();
        

        return abilitySequence;
    }
}
