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

    public bool IsChannelingBurst { get => channelingRoundsLeft > 0;}

    float currentHealth;
    public float CurrentHealth { get => currentHealth; set => currentHealth = value; }

    int currentActionPoints;
    public int CurrentActionPoints { get => currentActionPoints; set => currentActionPoints = value; }

    int currentArmor;
    public int CurrentArmor { get => currentArmor; set => currentArmor = value; }

    Vector2Int effectPosition;
    public int channelingRoundsLeft = -1;
    public int stasisRoundsLeft = -1;

    public bool stasis { get => stasisRoundsLeft > 0; }

    //For PropertyDrawer
    List<Vector2Int> tilesForCast;
    List<Vector2Int> tilesForEffect;
    Vector2Int castCase;

    [HideInInspector] public int heldCrystalValue = -1;

    EntityAnimator animator;

    public Ability channelingAbility; 

    public void Init()
    {
        data = Instantiate(data);
        name = data.name;

        // TODO : set armor
        currentHealth = GetMaxHealth();

        InitAnimations();
    }

    private void Start()
    {
        SelectionManager.Instance.OnHoveredEntityChanged += Squish;
    }

    private void OnDestroy()
    {
        SelectionManager.Instance.OnHoveredEntityChanged -= Squish;
    }

    Tween squishTween;
    void Squish(EntityBehaviour entity)
    {
        if (entity != this) return;

        squishTween?.Kill(true);
        squishTween = transform.DOPunchScale(Quaternion.AngleAxis(-45, Vector3.up) * new Vector3(.4f,-.4f,0), .2f, 15, 1f);
    }

    void InitAnimations()
    {
        if (data.animations == null) return;

        animator = gameObject.AddComponent<EntityAnimator>();
        animator.mat = Resources.Load("Mat_Entity") as Material;

        animator.Init();

        animator.PlayAnimation(data.animations.idleAnimation);

        animator.Update();
    }

    private void Update()
    {

    }

    public void OnTurn()
    {
        if (data.brain == null)
        {
            if (data.entityTag == EntityTag.Totem)
            {
                RoundManager.Instance.EndTurn();
                return;
            }
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

        for (int i = 0; i < reachableTile.path.Count; i++)
        {
            moveSequence.AppendCallback(() =>
            {
                if (data.alignement == Alignement.Player) animator.PlayAnimation(data.animations.moveAnimation);
            });
            moveSequence.Append(transform.DOMove(new Vector3(reachableTile.path[i].position.x, 0, reachableTile.path[i].position.y), data.alignement == Alignement.Player ? data.animations.moveAnimation.Length-.1f : .3f)
                .SetEase(movementEase)
                .SetDelay(data.alignement == Alignement.Player? .1f : 0)
                .OnComplete(()=>
                {
                    if (data.alignement == Alignement.Player) animator.PlayAnimation(data.animations.idleAnimation);
                }));

            /*
            if(GetAlignement() == Alignement.Player)
            {
                // first half of the jump
                moveSequence.Insert(i * tileMovementSpeed, transform.DOMoveY(.8f, tileMovementSpeed * .5f).SetEase(Ease.OutSine));
                // second half
                moveSequence.Insert(i * tileMovementSpeed + tileMovementSpeed * .5f, transform.DOMoveY(0, tileMovementSpeed * .5f).SetEase(Ease.InSine));
            }*/
            
        }

        RoundManager.Instance.currentMovementSequence = moveSequence;

        return moveSequence;
    }


    public bool earnedXPThisAbility;
    public Sequence UseAbility(Ability ability, TileData targetTile)
    {
        CurrentActionPoints -= ability.cost;
        Sequence abilitySequence = DOTween.Sequence();

        Ease attackEase = Ease.InBack;
        Ease returnAttackEase = Ease.InOutExpo;

        Debug.Log(name + " is using " + ability.name);

        EntityAnimation anim = data.alignement == Alignement.Player ? data.animations.GetAbilityAnimation(data.GetAbilityNumber(ability)) : null;
        float duration = (anim == null || anim.frames.Count == 0) ? 1 : anim.Length;


        abilitySequence.AppendCallback(() =>
        {
            animator.PlayAnimation(anim);
            
        });

        abilitySequence.AppendInterval(duration);

        abilitySequence.AppendCallback(() =>
        {
            earnedXPThisAbility = false;

            for (int i = 0; i < ability.abilityEffect.Count; i++)
            {
                ability.abilityEffect[i].Activate(this, ability, targetTile);
            }
            animator.PlayAnimation(data.animations.idleAnimation);

        });


        /*
        abilitySequence.Append(ability.GetStartTween(transform, targetTile.position)
        .SetEase(attackEase, 10)
        .OnComplete(() =>
        {
            for(int i = 0; i < ability.abilityEffect.Count; i++)
            {
                ability.abilityEffect[i].Activate(this, ability, targetTile);
            }
        }));
        abilitySequence.Append(ability.GetEndTween(transform)
        .SetEase(returnAttackEase, 10)
        .OnComplete(()=>
        {
            animator.PlayAnimation(data.animations.idleAnimation);
        }));
           */

        return abilitySequence;
    }
    
}

