using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "ScriptableObjects/DamageEffect", order = 100)]
public class DamageEffect : AbilityEffect
{


    [Tooltip("SFX quand l'attaque touche")]
    public AudioSource sfxHit;

    [Tooltip("VFX quand l'attaque touche")]
    public GameObject vfxHit;

    public override void Activate(EntityBehaviour entity, Ability ability, TileData castTile)
    {
        List<EntityBehaviour> entities = new List<EntityBehaviour>();
        List<Vector2Int> effectTiles = ability.effectArea.GetWorldSpaceRotated(entity.GetPosition(), castTile.position);
        for (int i = 0; i < effectTiles.Count; i++)
        {
            entities.AddRange(MapManager.GetTile(effectTiles[i]).entities);

        }

        ApplyEffect(entity, ability, castTile, (x) => {
            if (x.data.alignement != entity.data.alignement && !x.data.isNotDestructible)
            {
                Debug.Log(x.IsChannelingBurst);
                if (!x.IsChannelingBurst)
                {
                    x.animator.PlayAnimation(x.data.animations.hitAnimation);
                    returnInIdle(x);
                }
                    

                SoundManager.Instance.PlaySound(x.data.hitBySomeoneSFX.sound, false);
                float damage = Mathf.Ceil(SetDamage(entity, ability) - x.CurrentArmor);
                x.CurrentHealth -= (int)damage;
                HUDManager.DisplayValue("-" + damage.ToString(), Color.red, new Vector3(x.GetPosition().x, .5f, x.GetPosition().y));
                x.CheckCurrentHealthAndDestroy();
                x.Shake();

                if (!entity.earnedXPThisAbility && entity.data.alignement == Alignement.Player)
                {
                    PlayerTeamManager.Instance.teamXp += 1;
                    entity.earnedXPThisAbility = true;
                    PlayerTeamManager.Instance.OnXPChanged?.Invoke();
                }
            }

            
        });

        
    }

    public float SetDamage(EntityBehaviour entity, Ability ability)
    {
        return CombatUtils.ComputeDamage(entity, ability);
    }

    public Sequence returnInIdle(EntityBehaviour entity)
    {
        Sequence returnSequence = DOTween.Sequence();

        returnSequence.AppendInterval(0.2f);
        returnSequence.AppendCallback(() =>
        {
            entity.animator.PlayAnimation(entity.data.animations.idleAnimation);
        });

        return returnSequence;
    }
}
