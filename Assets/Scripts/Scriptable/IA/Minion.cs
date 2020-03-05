using System.Collections.Generic;

public class Minion : Brain
{
    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        List<ReachableTile> reachableTiles = IAUtils.FindAllReachablePlace(entityBehaviour.currentTile.position, MapManager.GetMap(), entityBehaviour.CurrentActionPoints - entityBehaviour.data.abilities[0].cost, true);
    }
}
