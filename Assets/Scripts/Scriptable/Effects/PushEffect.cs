using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "PushEffect", menuName = "ScriptableObjects/PushEffect", order = 101)]
public class PushEffect : AbilityEffect
{
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
            PlayerTeamManager.Instance.teamXp += 1;
            entity.earnedXPThisAbility = true;
            PlayerTeamManager.Instance.OnXPChanged?.Invoke();
        }
    

        entities.Sort((x, y) => Vector2Int.Distance(entity.GetPosition(), x.GetPosition()).
                CompareTo(Vector2Int.Distance(entity.GetPosition(), y.GetPosition())));


        Vector2Int grabDirection = entity.GetPosition() - castTile.position;
        grabDirection.x = grabDirection.x == 0 ? 0 : (int)Mathf.Sign(grabDirection.x);
        grabDirection.y = grabDirection.y == 0 ? 0 : (int)Mathf.Sign(grabDirection.y);
        Debug.Log(grabDirection);

        for (int i = 0; i < entities.Count; i++)
        {
            
            Vector2Int finalTile = entities[i].GetPosition() - grabDirection;
            if (MapManager.IsInsideMap(finalTile))
            {
                if (MapManager.GetTile(finalTile).IsWalkable)
                {
                    Push(entities[i], finalTile);
                }
            }


          //  finalTile -= grabDirection;

           
        }
    }

    public Sequence Push(EntityBehaviour entity, Vector2Int grabVector)
    {
        Sequence pushSequence = DOTween.Sequence();
        Ease pushEase = Ease.InQuad;

        pushSequence.Append(entity.transform.DOMove(new Vector3(grabVector.x, 0, grabVector.y), 0.5f));
        entity.currentTile = MapManager.MoveEntity(entity, entity.GetPosition(), grabVector);

        return pushSequence;
    }
}
