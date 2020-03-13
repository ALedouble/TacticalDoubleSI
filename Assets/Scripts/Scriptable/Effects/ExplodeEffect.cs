using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "ExplodeEffect", menuName = "ScriptableObjects/ExplodeEffect", order = 108)]
public class ExplodeEffect : AbilityEffect
{
    public override void Activate(EntityBehaviour entity, Ability ability, TileData castTile)
    {
        PlayerTeamManager.Instance.LevelUpPlayerEntity(entity.data);
        entity.stasisRoundsLeft = 2;
        MapManager.GetListOfEntity().Remove(entity);
        ApplyEffect(entity, ability, castTile, (x) => {
            if (x.data.entityTag == EntityTag.Totem)
            {
                x.animator.PlayAnimation(x.data.animations.deathAnimation);
                PlayerTeamManager.Instance.LevelUpPlayerAbility(entity.data, x.data.totemValue);

                Death(x);
                
            }
        });
    }

    public Sequence Death(EntityBehaviour entity)
    {
        Sequence deathSequence = DOTween.Sequence();

        deathSequence.AppendInterval(2.5f);
        deathSequence.AppendCallback(() =>
        {
            MapManager.DeleteEntity(entity);
            Destroy(entity.gameObject);
        });

        return deathSequence;
    }
}