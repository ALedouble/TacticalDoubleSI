using System.Collections.Generic;
using DG.Tweening;

public class Minion : Brain
{
    EntityBehaviour minion;
    List<ReachableTile> reachableTiles;

    List<EntityBehaviour> enemyTank = new List<EntityBehaviour>();

    EntityBehaviour playerHealer = null;
    EntityBehaviour playerDPS = null;
    EntityBehaviour playerTank = null;

    ReachableTile playerHealerPathToAttack = null;
    ReachableTile playerDPSPathToAttack = null;
    ReachableTile playerTankPathToAttack = null;

    int percentOfLifeNeedForHelp = 25;
    int rangeAttackWhenLowLife = 2;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        minion = entityBehaviour;
        IAMinion();
    }

    /*
     * Gere un deplacement/attack du Minion
     */
    private void IAMinion(bool lowLife = false)
    {
        if (lowLife) reachableTiles = IAUtils.FindAllReachablePlace(minion.currentTile.position, minion.CurrentActionPoints - minion.data.abilities[0].cost, true);
        else reachableTiles = IAUtils.FindAllReachablePlace(minion.currentTile.position, rangeAttackWhenLowLife, true);

        if (CheckEndTurn()) return;

        GetAllPlayers();
        GetPlayerInRange();

        if (lowLife || !CheckIfNeedAndCanHaveHelp())
        {
            if (!IsThereAnExplosion())
            {
                if (AttackWithPriority(lowLife))
                {
                    CheckEndTurn(true);
                }
            }
        }
    }

    /*
     * Regarde si le turn du Minion est fini
     */
    private bool CheckEndTurn(bool pass = false)
    {
        if (pass || minion.CurrentActionPoints.Equals(0))
        {
            //RoundManager.EndTurn(minion);
            return true;
        }

        return false;
    }

    /*
     * Récupère l'ensemble des personnages du player, ou qu'ils soit sur la map
     */
    private void GetAllPlayers()
    {
        for (int i = 0; i < MapManager.GetListOfEntity().Count; i++)
        {
            if (MapManager.GetListOfEntity()[i].data.alignement.Equals(Alignement.Player))
            {
                if (MapManager.GetListOfEntity()[i].data.entityTag.Equals(EntityTag.Healer))
                {
                    playerHealer = MapManager.GetListOfEntity()[i];
                }

                else if (MapManager.GetListOfEntity()[i].data.entityTag.Equals(EntityTag.DPS))
                {
                    playerDPS = MapManager.GetListOfEntity()[i];
                }

                else if (MapManager.GetListOfEntity()[i].data.entityTag.Equals(EntityTag.Tank))
                {
                    playerTank = MapManager.GetListOfEntity()[i];
                }
            }

            else if (MapManager.GetListOfEntity()[i].data.alignement.Equals(Alignement.Enemy))
            {
                if (MapManager.GetListOfEntity()[i].data.entityTag.Equals(EntityTag.Tank))
                {
                    enemyTank.Add(MapManager.GetListOfEntity()[i]);
                }
            }
        }
    }

    /*
     * Regarde si les entity du players sont dans l'area que peut atteindre le minion avec mouvement + attack
     */
    private void GetPlayerInRange()
    {
        for (int i = 0; i < reachableTiles.Count; i++)
        {
            playerHealerPathToAttack = IAUtils.ValidCastFromTile(minion.data.abilities[0].effectArea, reachableTiles, playerHealer.currentTile.position)[0];
            playerDPSPathToAttack = IAUtils.ValidCastFromTile(minion.data.abilities[0].effectArea, reachableTiles, playerDPS.currentTile.position)[0];
            playerTankPathToAttack = IAUtils.ValidCastFromTile(minion.data.abilities[0].effectArea, reachableTiles, playerTank.currentTile.position)[0];
        }
    }

    /*
     * Regarde si le Minion à besoin d'aide (PV < 25%) et s'il reste au moins un Tank (Enemy)
     */
    private bool CheckIfNeedAndCanHaveHelp()
    {
        if (minion.CurrentHealth < ((minion.data.maxHealth * percentOfLifeNeedForHelp ) / 100))
        {
            if (enemyTank.Count > 0)
            {
                ReachableTile pathToTank = RunForHelp();
                MoveToTank(pathToTank);

                return true;
            }
        }

        return false;
    }

    /*
     * Va vers le Tank le plus proche si les PV sont < 25%
     */
    private ReachableTile RunForHelp()
    {
        List<ReachableTile> listOfPathToTanks = new List<ReachableTile>();

        for (int i = 0; i < enemyTank.Count; i++)
        {
            ReachableTile pathToTank = IAUtils.FindShortestPath(minion.currentTile.position, enemyTank[i].currentTile.position);
            if (pathToTank != null) listOfPathToTanks.Add(pathToTank);
        }

        listOfPathToTanks.Sort();
        return listOfPathToTanks[0];
    }

    /*
     * Regarde si l'un des players prepare une explosion et agit en consequence
     */
    private bool IsThereAnExplosion()
    {
        bool aJouer = false;

        if (playerHealer.IsChannelingBurst)
        {
            aJouer = MoveAndTriggerAbility(playerHealerPathToAttack);
        }

        if (!aJouer && playerDPS.IsChannelingBurst)
        {
            aJouer = MoveAndTriggerAbility(playerHealerPathToAttack);
        }

        if (!aJouer && playerTank.IsChannelingBurst)
        {
            aJouer = MoveAndTriggerAbility(playerHealerPathToAttack);
        }

        return aJouer;
    }

    /*
     * Si l'on à aucune situation particulière, attaque par ordre de priorité
     */
    private bool AttackWithPriority(bool lowLife)
    {
        if (!MoveAndTriggerAbility(playerHealerPathToAttack))
        {
            if (!MoveAndTriggerAbility(playerDPSPathToAttack))
            {
                if (!MoveAndTriggerAbility(playerTankPathToAttack))
                {
                    if (!lowLife)
                    {
                        ReachableTile pathToShortestEnemy = CantAttack();
                        MoveTo(pathToShortestEnemy);
                    }

                    else
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /*
     * Si aucune attaque n'est possible, se rapproche du player le plus proche
     */
    private ReachableTile CantAttack()
    {
        List<ReachableTile> listOfPathPlayers = new List<ReachableTile>();

        ReachableTile pathToHealer = IAUtils.FindShortestPath(minion.currentTile.position, playerHealer.currentTile.position);
        if (pathToHealer != null) listOfPathPlayers.Add(pathToHealer);

        ReachableTile pathToDPS = IAUtils.FindShortestPath(minion.currentTile.position, playerDPS.currentTile.position);
        if (pathToDPS != null) listOfPathPlayers.Add(pathToDPS);

        ReachableTile pathToTank = IAUtils.FindShortestPath(minion.currentTile.position, playerTank.currentTile.position);
        if (pathToTank != null) listOfPathPlayers.Add(pathToTank);

        listOfPathPlayers.Sort();
        return listOfPathPlayers[0];
    }

    /*
     * Deplace le minion sur "target" apres avoir verifier qu'aucun minion ne se trouve autour et rappelle le comportement general
     */
    private bool MoveTo (ReachableTile target)
    {
        if (CanMoveTo(target))
        {
            minion.MoveTo(target).OnComplete(() => { IAMinion(); });

            return true;
        }

        return false;
    }

    /*
     * Deplace le minion sur "target" apres avoir verifier qu'aucun minion ne se trouve autour et appelle le comportement "low life"
     */
    private bool MoveToTank (ReachableTile target)
    {
        if (CanMoveTo(target))
        {
            minion.MoveTo(target).OnComplete(() => { IAMinion(true); });

            return true;
        }

        return false;
    }

    /*
     * Deplace le minion sur "target" apres avoir verifier qu'aucun minion ne se trouve autour et attack le player sur "target"
     */
    private bool MoveAndTriggerAbility(ReachableTile target)
    {
        if (target != null && CanMoveTo(target))
        {
            minion.MoveTo(target).OnComplete(() => { minion.UseAbility(minion.data.abilities[0], target.GetLastTile()).OnComplete(() => { IAMinion(); }); });
            
            return true;
        }

        return false;
    }

    /*
     * Regarde s'il y a un Minion autour de "target"
     */
    private bool CanMoveTo(ReachableTile target)
    {
        List<TileData> around = IAUtils.TilesAround(target.GetLastTile());

        for (int i = 0; i < around.Count; i++)
        {
            for (int j = 0; j < around[i].entities.Count; j++)
            {
                if (around[i].entities[j].data.entityTag.Equals(EntityTag.Minion))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
