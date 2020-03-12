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
            if (x.data.alignement == entity.data.alignement)
            {
                int armor = UpgradeArmor(entity, ability);
                x.CurrentArmor += armor;
                x.Stretch();
                HUDManager.DisplayValue("+" + armor.ToString(), Color.yellow, new Vector3(x.GetPosition().x, .5f, x.GetPosition().y));
            }
        });
    }

    public int UpgradeArmor(EntityBehaviour entity, Ability ability)
    {
        return entity.CurrentArmor + 1;
    }
}
