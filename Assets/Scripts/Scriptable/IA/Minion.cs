using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinionBrain", menuName = "ScriptableObjects/IA_Brain/Minion_Brain", order = 999)]
public class Minion : Brain
{
    IAUtils.IAEntity iaEntityFunction;
    IAUtils.SpecificConditionReachable conditionFunction;
    IAUtils.LambdaAbilityCall minionAbilityCall;

    EntityBehaviour minion;
    List<ReachableTile> reachableTiles;

    List<EntityBehaviour> enemyTank;

    EntityBehaviour playerHealer;
    EntityBehaviour playerDPS;
    EntityBehaviour playerTank;

    List<ReachableTile> playerHealerPathToAttack;
    List<ReachableTile> playerDPSPathToAttack;
    List<ReachableTile> playerTankPathToAttack;
    
    static bool firstActionInTurn;
    static bool lowLife;
    static bool haveEndTurn;

    public float percentOfLifeNeedForHelp = 25f;
    public int rangeAttackWhenLowLife = 2;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        iaEntityFunction = IAMinion;
        conditionFunction = SpecificConditionForMove;
        minionAbilityCall = IAUtils.LambdaAbilityCallDelegate;

        Debug.Log("New Minion");

        Init(entityBehaviour);
        iaEntityFunction();
    }

    private void Init(EntityBehaviour entityBehaviour)
    {
        minion = entityBehaviour;

        enemyTank = new List<EntityBehaviour>();

        playerHealer = null;
        playerDPS = null;
        playerTank = null;

        playerHealerPathToAttack = null;
        playerDPSPathToAttack = null;
        playerTankPathToAttack = null;

        firstActionInTurn = true;
        lowLife = false;
        haveEndTurn = false;
    }

    /*
     * Gere un deplacement/attack du Minion
     */
    private void IAMinion()
    {
        if (lowLife) reachableTiles = IAUtils.FindAllReachablePlace(minion.GetPosition(), rangeAttackWhenLowLife, true);
        else reachableTiles = IAUtils.FindAllReachablePlace(minion.GetPosition(), minion.CurrentActionPoints - minion.GetAbilities(0).cost, true);
                
        IAUtils.GetAllEntity(minion, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank);
        IAUtils.GetPlayerInRange(reachableTiles, minion.GetAbilities(0), ref playerHealerPathToAttack, ref playerDPSPathToAttack, ref playerTankPathToAttack, playerHealer, playerDPS, playerTank);

        Debug.LogWarning(playerHealerPathToAttack[0].GetCoordPosition());
        Debug.LogWarning(playerHealerPathToAttack[0].castTile.GetCoordPosition());

        Debug.LogWarning(playerDPSPathToAttack[0].GetCoordPosition());
        Debug.LogWarning(playerDPSPathToAttack[0].castTile.GetCoordPosition());

        Debug.LogWarning(playerTankPathToAttack[0].GetCoordPosition());
        Debug.LogWarning(playerTankPathToAttack[0].castTile.GetCoordPosition());


        if (IAUtils.CheckEndTurn(minion, CanMakeAction())) return;

        if (Explosion()) return;

        if (CheckIfNeedAndCanHaveHelp()) return;

        if (Attack()) return;

        if (Walk()) return;

        IAUtils.CheckEndTurn(minion, CanMakeAction(), true);
    }

    /*
     * Verifie si le Minion peut encore effectue une action 
     */
    private bool CanMakeAction()
    {
        if (haveEndTurn) return false;

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

        return IAUtils.CanWalkAround(minion, ((lowLife) ? (rangeAttackWhenLowLife > minion.CurrentActionPoints ? minion.CurrentActionPoints : rangeAttackWhenLowLife) : (minion.CurrentActionPoints)), true);
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
                return IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(minion, enemyTank, reachableTiles, iaEntityFunction, null, SpecificConditionForMove, null, true);
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
        Debug.LogError(playerHealerPathToAttack[0].castTile.position);
        return IAUtils.AttackWithPriority(minion, playerHealerPathToAttack, playerDPSPathToAttack, playerTankPathToAttack, iaEntityFunction, minionAbilityCall, minion.GetAbilities(0), conditionFunction);
    }

    /*
     * Cherche l'enemy le plus pres et s'en rapproche (si on a encore assez de vie)
     */
    private bool Walk()
    {
        if (!lowLife)
        {
            haveEndTurn = IAUtils.WalkOnShortest(minion, playerHealer, playerDPS, playerTank, iaEntityFunction, conditionFunction);
        }

        return haveEndTurn;
    }

    /*
     * Impossibilite de se deplacer sur la Tile X si X a pour voisin un Minion
     * 
     * S'il y a un Minion => HaveXEntityAround --> true
     * SpecificConditionForMove --> false
     */
    private bool SpecificConditionForMove (ReachableTile target)
    {
        if (target == null || target.path.Count <= 0) return false;

        return !IAUtils.HaveXEntityAround(minion, Alignement.Enemy, target, EntityTag.Minion);
    }
}