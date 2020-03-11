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
        bool entitiesFounded = false;
        List<EntityBehaviour> entities = new List<EntityBehaviour>();
        List<Vector2Int> effectTiles = ability.effectArea.GetWorldSpaceRotated(entity.GetPosition(), castTile.position);
        for (int i = 0; i < effectTiles.Count; i++)
        {
            entities.AddRange(MapManager.GetTile(effectTiles[i]).entities);

            if (entities.Count > 0)
            {
                entitiesFounded = true;
            }
        }

        if (entitiesFounded && ability.alignementXP == OnEntityAlignementXp.Ennemies)
        {
            if (!entity.earnedXPThisAbility)
            {
                PlayerTeamManager.Instance.teamXp += 1;
                entity.earnedXPThisAbility = true;
            }
        }


        ApplyEffect(entity, ability, castTile, (x) => {
            if (x.data.alignement != entity.data.alignement)
            {
                float damage = Mathf.Ceil(SetDamage(entity, ability) - x.CurrentArmor);
                x.CurrentHealth -= damage;
                HUDManager.DisplayValue("-" + damage.ToString(), Color.red, new Vector3(x.GetPosition().x, .5f, x.GetPosition().y));
                x.CheckCurrentHealthAndDestroy();
            }
        });

        
    }

    public float SetDamage(EntityBehaviour entity, Ability ability)
    {
        return CombatUtils.ComputeDamage(entity, ability);
    }
}
