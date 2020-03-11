using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "ScriptableObjects/HealEffect", order = 105)]
public class HealEffect : AbilityEffect
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

        if (entitiesFounded && ability.alignementXP == OnEntityAlignementXp.Allies)
        {
            PlayerTeamManager.Instance.teamXp += 1;
            entity.earnedXPThisAbility = true;
        }


        ApplyEffect(entity, ability, castTile, (x) => {
            if(x.data.alignement == entity.data.alignement)
            {
                float heal = SetHeal(entity, ability);
                x.CurrentHealth += heal;
                HUDManager.DisplayValue("+" + heal.ToString(), Color.green, new Vector3(x.GetPosition().x, .5f, x.GetPosition().y));
            }
        });
    }

    public float SetHeal(EntityBehaviour entity, Ability ability)
    {
        return CombatUtils.ComputeHeal(entity, ability);
    }
}
