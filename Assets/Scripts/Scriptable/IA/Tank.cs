using System.Collections.Generic;

public class Tank : Brain
{
    IAUtils.IAEntity iaEntityFunction;
    IAUtils.LambdaAbilityCall tankAbilityCall;

    EntityBehaviour tank;
    List<ReachableTile> reachableTiles;

    EntityBehaviour playerHealer = null;
    EntityBehaviour playerDPS = null;
    EntityBehaviour playerTank = null;

    ReachableTile playerHealerPathToAttack = null;
    ReachableTile playerDPSPathToAttack = null;
    ReachableTile playerTankPathToAttack = null;

    static string nameAbility1 = "Cac";
    static Ability ability1;

    static string nameAbility2 = "Repousse";
    static Ability ability2;

    static int percentOfLifeNeedForChangePatern = 50;
    static bool haveUseFirstAttack;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        iaEntityFunction = IATank;
        tankAbilityCall = IAUtils.LambdaAbilityCallDelegate;

        tank = entityBehaviour;
        haveUseFirstAttack = false;

        iaEntityFunction();
    }

    /*
     * Gere un deplacement/attack du Minion
     */
    private void IATank()
    {
        IAUtils.GetAbility(tank, nameAbility1, nameAbility2, ref ability1, ref ability2);
        IAUtils.GetAllEntity(tank, ref playerHealer, ref playerDPS, ref playerTank);
               

        if (IAUtils.CheckEndTurn(tank, CanMakeAction())) return;

        if (Explosion()) return;
        
        if (Attack()) return;

        WalkVersPrio();

    }

    /*
     * Verifie si le Tank peut encore effectue une action 
     */
    private bool CanMakeAction()
    {
        if (tank.CurrentActionPoints >= ability2.cost) return true;

        return IAUtils.CanWalkAround(tank, tank.CurrentActionPoints);
    }

    /*
     * Regarde si un joueur dans la range d'attaque prepare une explosion et le focus
     * 
     * Si oui, return true
     * Sinon, return false
     */
    private bool Explosion()
    {
        reachableTiles = IAUtils.FindAllReachablePlace(tank.GetPosition(), tank.CurrentActionPoints - ability2.cost, true);
        IAUtils.GetPlayerInRange(reachableTiles, tank.GetAbilities(0), ref playerHealerPathToAttack, ref playerDPSPathToAttack, ref playerTankPathToAttack, playerHealer, playerDPS, playerTank);

        return IAUtils.IsThereAnExplosion(tank, playerDPS, playerTank, playerHealer, playerDPSPathToAttack, playerTankPathToAttack, playerHealerPathToAttack,
                                               iaEntityFunction, tankAbilityCall, ability2);
    }

    /*
     * Si le tank a moins de "percentOfLifeNeedForChangePatern" % HP, cherche un enemy dans sa range a repousser
     */
    private bool Attack()
    {
        if (tank.CurrentHealth < ((tank.GetMaxHealth() * percentOfLifeNeedForChangePatern) / 100))
        {
            reachableTiles = IAUtils.FindAllReachablePlace(tank.GetPosition(), tank.CurrentActionPoints - ability2.cost, true);
            IAUtils.GetPlayerInRange(reachableTiles, tank.GetAbilities(0), ref playerHealerPathToAttack, ref playerDPSPathToAttack, ref playerTankPathToAttack, playerHealer, playerDPS, playerTank);

            return IAUtils.AttackWithPriority(tank, playerDPSPathToAttack, playerTankPathToAttack, playerHealerPathToAttack, iaEntityFunction, tankAbilityCall, ability2);
        }

        if (haveUseFirstAttack)
        {
            haveUseFirstAttack = false;

            reachableTiles = new List<ReachableTile>() { new ReachableTile(new List<TileData>(), 0) };
            IAUtils.GetPlayerInRange(reachableTiles, tank.GetAbilities(0), ref playerHealerPathToAttack, ref playerDPSPathToAttack, ref playerTankPathToAttack, playerHealer, playerDPS, playerTank);

            if (IAUtils.AttackWithPriority(tank, playerDPSPathToAttack, playerTankPathToAttack, playerHealerPathToAttack, iaEntityFunction, tankAbilityCall, ability2))
            {
                return true;
            }
        }

        haveUseFirstAttack = true;

        reachableTiles = IAUtils.FindAllReachablePlace(tank.GetPosition(), tank.CurrentActionPoints - ability1.cost, true);
        IAUtils.GetPlayerInRange(reachableTiles, tank.GetAbilities(0), ref playerHealerPathToAttack, ref playerDPSPathToAttack, ref playerTankPathToAttack, playerHealer, playerDPS, playerTank);

        return IAUtils.AttackWithPriority(tank, playerHealerPathToAttack, playerDPSPathToAttack, playerTankPathToAttack, iaEntityFunction, tankAbilityCall, ability1);

    }

    /*
     * Regarde vers quel entity il va se deplace selon l'ordre de priorite Heal > DPS > Tank
     */
    private void WalkVersPrio()
    {
        reachableTiles = IAUtils.FindAllReachablePlace(tank.GetPosition(), tank.CurrentActionPoints, true);
        ReachableTile target = IAUtils.PathToShortestEnemy(false, tank, playerDPS, playerTank, playerHealer, true, -1, true);

        if (target != null)
        {
            IAUtils.MoveAndTriggerAbilityIfNeed(tank, target, null);
        }

        IAUtils.CheckEndTurn(tank, CanMakeAction(), true);
    }
}