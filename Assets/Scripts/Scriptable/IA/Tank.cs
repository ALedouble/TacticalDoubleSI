using System;
using System.Collections.Generic;

public class Tank : Brain
{
    IAUtils.IAEntity iaEntityFunction;
    IAUtils.SpecificConditionReachable conditionFunction;
    IAUtils.LambdaAbilityCall tankAbilityCall;

    EntityBehaviour tank;
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

    static string nameAbility1 = "Cac";
    static Ability ability1;

    static string nameAbility2 = "Dist";
    static Ability ability2;

    static int percentOfLifeNeedForHealer = 25;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        iaEntityFunction = IATank;
        conditionFunction = SpecificConditionForMove;
        tankAbilityCall = IAUtils.LambdaAbilityCallDelegate;

        tank = entityBehaviour;

        iaEntityFunction();
    }

    /*
     * Gere un deplacement/attack du Minion
     */
    private void IATank()
    {
        IAUtils.GetAbility(tank, nameAbility1, nameAbility2, ref ability1, ref ability2);
        IAUtils.GetAllEntity(tank, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank);

        reachableTiles = IAUtils.FindAllReachablePlace(tank.GetPosition(), tank.CurrentActionPoints, true);


        if (IAUtils.CheckEndTurn(tank, CanMakeAction())) return;

        if (Explo()) return;

        if (Danger()) return;

        if (Attack()) return;

        WalkVersPrio();
    }

    /*
     * Verifie si le Tank peut encore effectue une action 
     */
    private bool CanMakeAction()
    {
        return IAUtils.CanWalkAround(tank, tank.CurrentActionPoints);
    }

    private bool Explo()
    {
        throw new NotImplementedException();
    }

    private bool Danger()
    {
        throw new NotImplementedException();
    }

    private bool Attack()
    {
        throw new NotImplementedException();
    }

    private void WalkVersPrio()
    {
        throw new NotImplementedException();
    }


    private bool SpecificConditionForMove(ReachableTile target)
    {
        return !IAUtils.HaveXEntityAround(Alignement.Enemy, target, EntityTag.Minion);
    }
}