using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "MovementEffect", menuName = "ScriptableObjects/MovementEffect", order = 104)]
public class MovementEffect : AbilityEffect
{

    [Tooltip("SFX quand l'attaque touche")]
    public AudioSource sfxHit;

    [Tooltip("VFX quand l'attaque touche")]
    public GameObject vfxHit;

    public override void Activate(EntityBehaviour entity, Ability ability, TileData castTile)
    {
        ApplyEffect(entity, ability, castTile, (x) =>
        {
            TeleportBegin(entity, castTile.position);
        });
    }

    public Sequence TeleportBegin(EntityBehaviour entity, Vector2Int castTilePosition)
    {
        Sequence teleportSequence = DOTween.Sequence();
        Ease pushEase = Ease.InQuad;
        teleportSequence.Append(entity.transform.DOMove(
            new Vector3(entity.currentTile.position.x, 10, entity.currentTile.position.y),
            0.25f).SetEase(pushEase)).OnComplete(() =>
            {
                TeleportEnd(entity, castTilePosition);
            });

        return teleportSequence;
    }

    public Sequence TeleportEnd(EntityBehaviour entity, Vector2Int castTilePosition)
    {
        Sequence teleportEndSequence = DOTween.Sequence();
        Ease pushEase = Ease.InQuad;
        teleportEndSequence.Append(entity.transform.DOMove(
            new Vector3(castTilePosition.x, 0, castTilePosition.y),
            0.25f).SetEase(pushEase));

        entity.currentTile = MapManager.MoveEntity(entity, entity.GetPosition(), castTilePosition);
        return teleportEndSequence;
    }
}
