using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpCrystalEffect : AbilityEffect
{
    // Start is called before the first frame update
    public override void Activate(EntityBehaviour entity, Ability ability, TileData castTile)
    {
        ApplyEffect(entity, ability, castTile, (x) => {
            if (x.data.alignement == Alignement.Neutral)
            {
                PickUp(entity);
            }
        });
    }

    // Update is called once per frame
    public void PickUp(EntityBehaviour entity)
    {
        MapManager.DeleteEntity(entity);
    }
}
