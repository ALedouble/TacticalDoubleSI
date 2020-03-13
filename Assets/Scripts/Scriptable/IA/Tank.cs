using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TankBrain", menuName = "ScriptableObjects/IA_Brain/Tank_Brain", order = 999)]
public class Tank : Brain
{
    IAUtils.IAEntity iaEntityFunction;
    IAUtils.LambdaAbilityCall tankAbilityCall;

    EntityBehaviour tank;
    List<ReachableTile> reachableTiles;

    EntityBehaviour playerHealer;
    EntityBehaviour playerDPS;
    EntityBehaviour playerTank;

    List<ReachableTile> playerHealerPathToAttack;
    List<ReachableTile> playerDPSPathToAttack;
    List<ReachableTile> playerTankPathToAttack;

    static Ability ability1;
    static Ability ability2;

    static bool haveUseFirstAttack;
    static bool haveEndTurn;

    public float percentOfLifeNeedForChangePatern = 50f;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        iaEntityFunction = IATank;
        tankAbilityCall = IAUtils.LambdaAbilityCallDelegate;


        InitStart(entityBehaviour);
        iaEntityFunction();
    }

    private void InitStart(EntityBehaviour entityBehaviour)
    {
        tank = entityBehaviour;

        ability1 = tank.GetAbilities(0);
        ability2 = tank.GetAbilities(1);

        haveUseFirstAttack = false;
        haveEndTurn = false;
    }

    private void InitEachLoop()
    {
        playerHealer = null;
        playerDPS = null;
        playerTank = null;

        playerHealerPathToAttack = null;
        playerDPSPathToAttack = null;
        playerTankPathToAttack = null;
    }

    /*
     * Gere un deplacement/attack du Tank
     */
    private void IATank()
    {
        InitEachLoop();

        IAUtils.GetAllEntity(tank, ref playerHealer, ref playerDPS, ref playerTank);

        if (IAUtils.CheckEndTurn(tank, CanMakeAction())) return;

        if (Explosion()) return;
        
        if (Attack()) return;

        if (Walk()) return;

        if (LastActionPossible()) return;

        IAUtils.CheckEndTurn(tank, CanMakeAction(), true);
    }

    /*
     * Verifie si le Tank peut encore effectue une action 
     */
    private bool CanMakeAction()
    {
        if (haveEndTurn) return false;
        if (tank.CurrentActionPoints >= ability1.cost) return true;
        if (tank.CurrentActionPoints >= ability2.cost) return true;

        return IAUtils.CanWalkAround(tank, tank.CurrentActionPoints, true);
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

            reachableTiles = new List<ReachableTile>() { new ReachableTile(new List<TileData>() { tank.currentTile }, 0) };
            IAUtils.GetPlayerInRange(reachableTiles, tank.GetAbilities(0), ref playerHealerPathToAttack, ref playerDPSPathToAttack, ref playerTankPathToAttack, playerHealer, playerDPS, playerTank);

            if (IAUtils.AttackWithPriority(tank, playerDPSPathToAttack, playerTankPathToAttack, playerHealerPathToAttack, iaEntityFunction, tankAbilityCall, ability2))
            {
                return true;
            }

            return false;
        }

        reachableTiles = IAUtils.FindAllReachablePlace(tank.GetPosition(), tank.CurrentActionPoints - ability1.cost, true);
        IAUtils.GetPlayerInRange(reachableTiles, tank.GetAbilities(0), ref playerHealerPathToAttack, ref playerDPSPathToAttack, ref playerTankPathToAttack, playerHealer, playerDPS, playerTank);

        if (IAUtils.AttackWithPriority(tank, playerDPSPathToAttack, playerTankPathToAttack, playerHealerPathToAttack, iaEntityFunction, tankAbilityCall, ability1))
        {
            haveUseFirstAttack = true;
            return true;
        }

        return false;

    }

    /*
     * Cherche l'enemy le plus pres et s'en rapproche
     */
    private bool Walk()
    {
        haveEndTurn = IAUtils.WalkOnShortest(tank, playerDPS, playerTank, playerHealer, iaEntityFunction);
        return haveEndTurn;
    }

    /*
     * Permet de se deplacer meme si aucun chemin n'est disponible jusque le player
     */
    private bool LastActionPossible()
    {
        haveEndTurn = IAUtils.LastChancePath(tank, playerDPS, playerTank, playerHealer, iaEntityFunction);
        return haveEndTurn;
    }
}