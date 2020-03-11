using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExplodeEffect", menuName = "ScriptableObjects/ExplodeEffect", order = 108)]
public class ExplodeEffect : AbilityEffect
{
    public override void Activate(EntityBehaviour entity, Ability ability, TileData castTile)
    {
        PlayerTeamManager.Instance.LevelUpPlayerEntity(entity.data);
        entity.IsChannelingBurst = true;
        MapManager.GetListOfEntity().Remove(entity);
        ApplyEffect(entity, ability, castTile, (x) => {
            if (x.data.entityTag == EntityTag.Totem)
            {
                PlayerTeamManager.Instance.LevelUpPlayerAbility(entity.data, x.data.totemValue);

                MapManager.DeleteEntity(x);
                Destroy(x.gameObject);
            }
        });
    }
}