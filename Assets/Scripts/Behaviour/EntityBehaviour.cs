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

    [HideInInspector] public EntityAnimator animator;

    public Ability channelingAbility; 

    public void Init()
    {
        data = Instantiate(data);
        name = data.name;
        
        // TODO : set armor
        currentHealth = GetMaxHealth();

        if (data.isFx)
        {
            data.brain = Instantiate(data.brain);
            GameObject go = Instantiate(data.fxEntity, transform.position, Quaternion.identity, this.transform);
            return;
        }

        InitAnimations();
    }

    private void Start()
    {
        SelectionManager.Instance.OnHoveredEntityChanged += Squish;
    }

    private void OnDestroy()
    {
        squishTween?.Kill(true);
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
        animator.transform.GetChild(0).localPosition = new Vector3(data.pivot.x - .5f, animator.transform.GetChild(0).localPosition.y, data.pivot.y - .5f);

        animator.PlayAnimation(data.animations.idleAnimation);

        animator.Update();
    }


    public void OnTurn()
    {
        if (data.brain == null)
        {

            RoundManager.Instance.EndTurn();
            return;

            Debug.LogError("Entity " + name + " has no brain, please add a brain to its entity asset", this.gameObject);
            Debug.Break();
        }
        else
        {
            data.brain.OnTurnStart(this);
            currentArmor = (int)data.armor;
        }
    }

    public Sequence MoveTo(ReachableTile reachableTile)
    {
        currentTile = MapManager.MoveEntity(this, currentTile.position, reachableTile);
        CurrentActionPoints -= reachableTile.cost;

        HUDManager.Instance.UpdateEntityInfo(null);

        Sequence moveSequence = DOTween.Sequence();
        Ease movementEase = Ease.InOutSine;

        for (int i = 0; i < reachableTile.path.Count; i++)
        {
            
            moveSequence.AppendCallback(() =>
            {
                animator.PlayAnimation(data.animations.moveAnimation);
                if(data.walkSFX != null) SoundManager.Instance.PlaySound(data.walkSFX.sound, false);
            });
            
            moveSequence.Append(transform.DOMove(new Vector3(reachableTile.path[i].position.x, 0, reachableTile.path[i].position.y), data.alignement == Alignement.Player ? data.animations.moveAnimation.Length-.1f : .3f)
                .SetEase(movementEase)
                .SetDelay(data.alignement == Alignement.Player? .1f : 0)
                .OnComplete(()=>
                {
                    animator.PlayAnimation(data.animations.idleAnimation);
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

        HUDManager.Instance.UpdateEntityInfo(null);

        Sequence abilitySequence = DOTween.Sequence();
        Debug.Log(ability);
        Debug.Log(ability.displayName);
        SoundManager.Instance.PlaySound(ability.abilitySFX.sound, false);
        Ease attackEase = Ease.InBack;
        Ease returnAttackEase = Ease.InOutExpo;

        Debug.Log(name + " is using " + ability.name);

        EntityAnimation anim = data.animations.GetAbilityAnimation(data.GetAbilityNumber(ability));
        float duration = (anim == null || anim.frames.Count == 0) ? 1 : anim.Length;


        abilitySequence.AppendCallback(() =>
        {
            animator.PlayAnimation(anim);

            if (ability.playEffectsAtStart)
            {
                
                List<Vector2Int> fxPositions = ability.effectArea.GetWorldSpaceRotated(GetPosition(), targetTile.position);

                for (int i = 0; i < fxPositions.Count; i++)
                {
                    FXManager.SpawnFX(ability.vfxCast, fxPositions[i], targetTile.position - GetPosition());
                }

                earnedXPThisAbility = false;

                for (int i = 0; i < ability.abilityEffect.Count; i++)
                {
                    ability.abilityEffect[i].Activate(this, ability, targetTile);
                }
            }
        });
        
        abilitySequence.AppendInterval(duration);

        abilitySequence.AppendCallback(() =>
        {
            if (!ability.playEffectsAtStart)
            {
                List<Vector2Int> fxPositions = ability.effectArea.GetWorldSpaceRotated(GetPosition(), targetTile.position);

                for (int i = 0; i < fxPositions.Count; i++)
                {
                    FXManager.SpawnFX(ability.vfxCast, fxPositions[i], targetTile.position - GetPosition());
                }

                earnedXPThisAbility = false;

                for (int i = 0; i < ability.abilityEffect.Count; i++)
                {
                    ability.abilityEffect[i].Activate(this, ability, targetTile);
                }
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

    public void CheckCurrentHealthAndDestroy()
    {
        HUDManager.Instance.UpdateEntityInfo(null);

        if (currentHealth <= 0)
        {
            SoundManager.Instance.PlaySound(data.deathSFX.sound, false);
            MapManager.GetListOfEntity().Remove(this);
            RoundManager.Instance.CheckRemainingEntities();
            MapManager.DeleteEntity(this);
            Destroy(gameObject);
            if(this.GetAlignement() == Alignement.Enemy)
            {
                PlayerTeamManager.Instance.teamXp += 2;
                PlayerTeamManager.Instance.OnXPChanged?.Invoke();
            }

        }
    }
    
    public void Shake()
    {
        transform.DOShakePosition(.5f, Quaternion.AngleAxis(-45, Vector3.up) * new Vector3(.8f, 0, 0), 30, 90, false, true);
    }

    public void Stretch()
    {
        transform.DOPunchScale(Quaternion.AngleAxis(-45, Vector3.up) * new Vector3(-.2f, .2f, 0), .2f, 15, 1f);
    }
}

