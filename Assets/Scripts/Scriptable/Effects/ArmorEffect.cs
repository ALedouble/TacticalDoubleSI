using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArmorEffect", menuName = "ScriptableObjects/ArmorEffect", order = 103)]
public class ArmorEffect : AbilityEffect
{
    public override void Activate(EntityBehaviour entity, Ability ability, TileData castTile)
    {
        ApplyEffect(entity, ability, castTile, (x) =>
        {
            x.CurrentArmor += UpgradeArmor(entity, ability);
        });
    }

    public int UpgradeArmor(EntityBehaviour entity, Ability ability)
    {
        return entity.CurrentArmor + 1;
    }
}
