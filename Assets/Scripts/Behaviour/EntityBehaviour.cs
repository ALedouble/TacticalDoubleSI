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
    public int GetMaxHealth()
    {
        return Mathf.CeilToInt(data.maxHealth);
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

    float currentHealth;
    public float CurrentHealth { get => currentHealth; set => currentHealth = value; }

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

    EntityAnimator animator;

    public void Init()
    {
        data = Instantiate(data);
        name = data.name;

        // TODO : set armor
        currentHealth = GetMaxHealth();

        InitAnimations();
    }

    void InitAnimations()
    {
        if (data.animations == null) return;

        animator = gameObject.AddComponent<EntityAnimator>();
        animator.mat = Resources.Load("Mat_Entity") as Material;

        animator.Init();

        animator.PlayAnimation(data.animations.GetAnimation(0));

        animator.Update();
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
        currentTile = MapManager.MoveEntity(this, currentTile.position, reachableTile);
        CurrentActionPoints -= reachableTile.cost;

        Sequence moveSequence = DOTween.Sequence();
        Ease movementEase = Ease.InOutSine;

        // TODO : put in scriptableObject
        float tileMovementSpeed = .25f;

        for (int i = 0; i < reachableTile.path.Count; i++)
        {
            moveSequence.Append(transform.DOMove(new Vector3(reachableTile.path[i].position.x, 0, reachableTile.path[i].position.y), tileMovementSpeed)
                .SetEase(movementEase));

            
            if(GetAlignement() == Alignement.Player)
            {
                // first half of the jump
                moveSequence.Insert(i * tileMovementSpeed, transform.DOMoveY(1, tileMovementSpeed * .5f).SetEase(Ease.OutQuad));
                // second half
                moveSequence.Insert(i * tileMovementSpeed + tileMovementSpeed * .5f, transform.DOMoveY(0, tileMovementSpeed * .5f).SetEase(Ease.InQuad));
            }
            
        }

        RoundManager.Instance.currentMovementSequence = moveSequence;

        return moveSequence;
    }

    public Sequence UseAbility(Ability ability, TileData targetTile)
    {
        CurrentActionPoints -= ability.cost;
        Sequence abilitySequence = DOTween.Sequence(); 

        Ease attackEase = Ease.InBack;
        Ease returnAttackEase = Ease.InOutExpo;

        abilitySequence.Append(ability.GetStartTween(transform, targetTile.position)
        .OnComplete(() =>
        {
            
            for(int i = 0; i < ability.abilityEffect.Count; i++)
            {
                ability.abilityEffect[i].Activate(this, ability, targetTile);
            }
        }));

        abilitySequence.Append(ability.GetEndTween(transform, targetTile.position)
        .SetEase(returnAttackEase, 10));
    

        return abilitySequence;
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

