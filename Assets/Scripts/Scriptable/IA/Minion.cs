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

    static bool firstActionInTurn;
    static bool lowLife;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        iaEntityFunction = IAMinion;
        conditionFunction = SpecificConditionForMove;
        minionAbilityCall = IAUtils.LambdaAbilityCallDelegate;

        minion = entityBehaviour;
        firstActionInTurn = true;
        lowLife = false;

        iaEntityFunction();
    }

    /*
     * Gere un deplacement/attack du Minion
     */
    private void IAMinion()
    {
        if (lowLife) reachableTiles = IAUtils.FindAllReachablePlace(minion.GetPosition(), rangeAttackWhenLowLife, true);
        else reachableTiles = IAUtils.FindAllReachablePlace(minion.GetPosition(), minion.CurrentActionPoints - minion.GetAbilities(0).cost, true);
        
        IAUtils.GetAllEntity(minion, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank);
        IAUtils.GetPlayerInRange(reachableTiles, minion.GetAbilities(0).effectArea, ref playerHealerPathToAttack, ref playerDPSPathToAttack, ref playerTankPathToAttack, playerHealer, playerDPS, playerTank);


        if (IAUtils.CheckEndTurn(minion, CanMakeAction())) return;

        if (Explosion()) return;

        if (CheckIfNeedAndCanHaveHelp()) return;

        if (Attack()) return;

        WalkOnShortest();

    }

    /*
     * Verifie si le Minion peut encore effectue une action 
     */
    private bool CanMakeAction()
    {
        List<TileData> around = IAUtils.TilesAround(minion.currentTile);
        for (int i = 0; i < around.Count; i++)
        {
            if (!around[i].IsWalkable)
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

        return IAUtils.CanWalkAround(minion, lowLife ? (rangeAttackWhenLowLife > minion.CurrentActionPoints ? minion.CurrentActionPoints : rangeAttackWhenLowLife) : minion.CurrentActionPoints);
    }

    /*
     * Regarde si un joueur dans la range d'attaque prepare une explosion et le focus
     * 
     * Si oui, return true
     * Sinon, return false
     */
    private bool Explosion()
    {
        return IAUtils.IsThereAnExplosion(minion, playerHealer, playerDPS, playerTank, playerHealerPathToAttack, playerDPSPathToAttack, playerTankPathToAttack,
                                                iaEntityFunction, minionAbilityCall, minion.GetAbilities(0), conditionFunction);
    }

    /*
     * Regarde si le Minion à besoin d'aide (PV < percentOfLifeNeedForHelp) et s'il reste au moins un Tank (Enemy)
     * 
     * Si oui, ce deplace au tank et previent qu'il est "lowLife"
     * Sinon, continue
     */
    private bool CheckIfNeedAndCanHaveHelp()
    {
        if (firstActionInTurn)
        {
            firstActionInTurn = false;
            if (minion.CurrentHealth < ((minion.GetMaxHealth() * percentOfLifeNeedForHelp) / 100))
            {
                lowLife = true;
                IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(minion, enemyTank, reachableTiles, iaEntityFunction, null, SpecificConditionForMove);
                return true;
            }
        }

        return false;
    }

    /*
     * Regarde s'il peut attaquer un joueur dans l'ordre Heal > Dps > Tank
     * 
     * Si oui, return true
     * Sinon, return false
     */
    private bool Attack()
    {
        return IAUtils.AttackWithPriority(minion, playerHealerPathToAttack, playerDPSPathToAttack, playerTankPathToAttack, iaEntityFunction,
                                                minionAbilityCall, minion.GetAbilities(0), playerHealer, playerDPS, playerTank, conditionFunction);
    }

    /*
     * Cherche l'enemy le plus pres et s'en rapproche (si on a encore assez de vie)
     */
    private void WalkOnShortest()
    {
        if (!lowLife)
        {
            ReachableTile pathToShortestEnemy = IAUtils.PathToShortestEnemy(false, minion, playerHealer, playerDPS, playerTank, true, minion.CurrentActionPoints);
            IAUtils.MoveAndTriggerAbilityIfNeed(minion, pathToShortestEnemy, iaEntityFunction, SpecificConditionForMove(pathToShortestEnemy));
        }

        IAUtils.CheckEndTurn(minion, CanMakeAction(), true);
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