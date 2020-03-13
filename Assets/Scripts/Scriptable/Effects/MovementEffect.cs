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
            
            TeleportBegin(entity, castTile.position);

    }

    public Sequence TeleportBegin(EntityBehaviour entity, Vector2Int castTilePosition)
    {
        Sequence teleportSequence = DOTween.Sequence();
        Ease pushEase = Ease.InQuad;
        TeleportEnd(entity, castTilePosition);

        teleportSequence.Append(entity.transform.DOMove(new Vector3(castTilePosition.x, 0, castTilePosition.y), 0)
            .SetDelay(1.5f));

        return teleportSequence;
    }

    public Sequence TeleportEnd(EntityBehaviour entity, Vector2Int castTilePosition)
    {
        Sequence teleportEndSequence = DOTween.Sequence();
        Ease pushEase = Ease.InQuad;

        entity.currentTile = MapManager.MoveEntity(entity, entity.GetPosition(), castTilePosition);
        return teleportEndSequence;
    }
}
