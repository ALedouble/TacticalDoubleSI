using System.Collections.Generic;

public class Minion : Brain
{
    IAUtils.IAEntity iaEntityFunction;
    IAUtils.SpecificConditionReachable conditionFunction;
    IAUtils.LambdaAbilityCall minionAbilityCall;

    EntityBehaviour minion;
    List<ReachableTile> reachableTiles;

    List<EntityBehaviour> enemyTank = new List<EntityBehaviour>();

    EntityBehaviour playerHealer = null;
    EntityBehaviour playerDPS = null;
    EntityBehaviour playerTank = null;

    ReachableTile playerHealerPathToAttack = null;
    ReachableTile playerDPSPathToAttack = null;
    ReachableTile playerTankPathToAttack = null;

    static int percentOfLifeNeedForHelp = 25;
    static int rangeAttackWhenLowLife = 2;
    static bool lowLife = false;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        iaEntityFunction = IAMinion;
        conditionFunction = SpecificConditionForMove;
        minionAbilityCall = IAUtils.LambdaAbilityCallDelegate;

        minion = entityBehaviour;

        iaEntityFunction();
    }

    /*
     * Gere un deplacement/attack du Minion
     */
    private void IAMinion()
    {
        if (lowLife) reachableTiles = IAUtils.FindAllReachablePlace(minion.GetPosition(), minion.CurrentActionPoints - minion.GetAbilities(0).cost, true);
        else reachableTiles = IAUtils.FindAllReachablePlace(minion.GetPosition(), rangeAttackWhenLowLife, true);

        if (IAUtils.CheckEndTurn(minion, CanMakeAction())) return;

        IAUtils.GetAllEntity(minion, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank);
        IAUtils.GetPlayerInRange(reachableTiles, minion.GetAbilities(0).effectArea, ref playerHealerPathToAttack, ref playerDPSPathToAttack, ref playerTankPathToAttack, playerHealer, playerDPS, playerTank);

        if (lowLife || !CheckIfNeedAndCanHaveHelp())
        {
            if (!IAUtils.IsThereAnExplosion(minion, playerHealer, playerDPS, playerTank, playerHealerPathToAttack, playerDPSPathToAttack, playerTankPathToAttack, 
                                                iaEntityFunction, minionAbilityCall, minion.GetAbilities(0), conditionFunction))
            {
                if (!IAUtils.AttackWithPriority(minion, playerHealerPathToAttack, playerDPSPathToAttack, playerTankPathToAttack, iaEntityFunction, 
                                                minionAbilityCall, minion.GetAbilities(0), playerHealer, playerDPS, playerTank, conditionFunction))
                {
                    if (!lowLife)
                    {
                        ReachableTile pathToShortestEnemy = IAUtils.PathToShortestEnemy(true, minion, playerHealer, playerDPS, playerTank);
                        IAUtils.MoveAndTriggerAbilityIfNeed(minion, pathToShortestEnemy, iaEntityFunction, SpecificConditionForMove(pathToShortestEnemy));
                    }

                    else
                    {
                        IAUtils.CheckEndTurn(minion, CanMakeAction(), true);
                    }
                }
            }
        }
    }

    /*
     * Verifie si le Minion peut encore effectue une action 
     */
    private bool CanMakeAction()
    {
        List<TileData> around = IAUtils.TilesAround(minion.currentTile);
        for (int i = 0; i < around.Count; i++)
        {
            if (around[i].IsWalkable && (int)around[i].tileType <= minion.CurrentActionPoints)
            {
                return true;
            }

            else if (!around[i].IsWalkable)
            {
                for (int j = 0; j < around[i].entities.Count; j++)
                {
                    if (around[i].entities[j].GetAlignement().Equals(Alignement.Player))
                    {
                        if (minion.GetAbilities(0).cost <= minion.CurrentActionPoints)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    /*
     * Regarde si le Minion à besoin d'aide (PV < percentOfLifeNeedForHelp%) et s'il reste au moins un Tank (Enemy)
     */
    private bool CheckIfNeedAndCanHaveHelp()
    {
        if (minion.CurrentHealth < ((minion.GetMaxHealth() * percentOfLifeNeedForHelp) / 100))
        {
            lowLife = true;
            return IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(minion, enemyTank, reachableTiles, iaEntityFunction, null, SpecificConditionForMove);
        }

        return false;
    }

    /*
     * Impossibilite de se deplacer sur la Tile X si X a pour voisin un Minion
     * 
     * S'il y a un Minion => HaveXEntityAround --> true
     * SpecificConditionForMove --> false
     */
    private bool SpecificConditionForMove (ReachableTile target)
    {
        return !IAUtils.HaveXEntityAround(Alignement.Enemy, target, EntityTag.Minion);
    }
}