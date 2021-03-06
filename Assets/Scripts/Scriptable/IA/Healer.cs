﻿using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealerBrain", menuName = "ScriptableObjects/IA_Brain/Healer_Brain", order = 999)]
public class Healer : Brain
{
    IAUtils.IAEntity iaEntityFunction;
    IAUtils.SpecificConditionEntity conditionFunction;
    IAUtils.LambdaAbilityCall healerAbilityCall;

    EntityBehaviour healer;
    List<ReachableTile> reachableTiles;

    List<EntityBehaviour> enemyTank;
    List<EntityBehaviour> enemyDPS;
    List<EntityBehaviour> enemyHealer;
    List<EntityBehaviour> enemyMinion;

    EntityBehaviour playerHealer;
    EntityBehaviour playerDPS;
    EntityBehaviour playerTank;

    List<ReachableTile> playerHealerPathToAttack;
    List<ReachableTile> playerDPSPathToAttack;
    List<ReachableTile> playerTankPathToAttack;

    static Ability ability1;
    static bool ability1Use;
    static Ability ability2;
    static bool ability2Use;

    static bool haveEndTurn;
    static float saveOfLife = -1f;
    static bool isFarAway;

    public int lifeLoseForPrio = 6;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        iaEntityFunction = IAHealer;
        conditionFunction = ConditionHealthToHeal;
        healerAbilityCall = IAUtils.LambdaAbilityCallDelegate;


        InitStart(entityBehaviour);
        iaEntityFunction();

        saveOfLife = healer.CurrentHealth;
    }

    private void InitStart(EntityBehaviour entityBehaviour)
    {
        healer = entityBehaviour;

        enemyTank = new List<EntityBehaviour>();
        enemyDPS = new List<EntityBehaviour>();
        enemyHealer = new List<EntityBehaviour>();
        enemyMinion = new List<EntityBehaviour>();

        ability1 = healer.GetAbilities(0);
        ability2 = healer.GetAbilities(1);

        ability1Use = false;
        ability2Use = false;

        haveEndTurn = false;
        isFarAway = false;

        if (saveOfLife == -1)
        {
            saveOfLife = healer.data.maxHealth;
        }
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
     * Gere un deplacement/attack du Healer
     */
    private void IAHealer()
    {
        InitEachLoop();

        IAUtils.GetAllEntity(healer, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank, ref enemyDPS, ref enemyHealer, ref enemyMinion);

        if (IAUtils.CheckEndTurn(healer, CanMakeAction())) return;

        if (IsInDanger()) return;

        if (Heal()) { ability1Use = true; return; }

        if (Attack()) { ability2Use = true; return; }

        if (MoveToShortestAlly()) return;

        IAUtils.CheckEndTurn(healer, CanMakeAction(), true);
    }

    /*
     * Verifie si le Healer peut encore effectue une action 
     */
    private bool CanMakeAction()
    {
        if (isFarAway || haveEndTurn) return false;
        if (!ability1Use && healer.CurrentActionPoints >= ability1.cost) return true;
        if (!ability2Use && healer.CurrentActionPoints >= ability2.cost) return true;

        return IAUtils.CanWalkAround(healer, healer.CurrentActionPoints, true);
    }

    /*
     * Si le healer est le dernier enemy autre que healer || si un player est a cote || s'il n'a plus toute sa vie
     */
    private bool IsInDanger()
    {
        if (Solo() || IAUtils.HaveXEntityAround(healer, Alignement.Player, healer.currentTile) || healer.CurrentHealth < saveOfLife)
        {
            RunAtMaxDistanceOfAll();
            return true;
        }

        return false;

        bool Solo()
        {
            if (enemyDPS.Count.Equals(0) && enemyTank.Count.Equals(0) && enemyMinion.Count.Equals(0))
                return true;

            return false;
        }
    }

    /*
     * Cherche quelle allie peut et doit etre Heal parmis Tank > DPS > Healer > Minion
     */
    private bool Heal()
    {
        if (ability1Use) return false;

        List<ReachableTile> pathToHeal = new List<ReachableTile>();
        reachableTiles = IAUtils.FindAllReachablePlace(healer.GetPosition(), healer.CurrentActionPoints - ability1.cost, true);

        if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyTank, reachableTiles, iaEntityFunction, ability1, ref pathToHeal,
                                                                        healerAbilityCall, conditionFunction, null, true))
        {
            if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyDPS, reachableTiles, iaEntityFunction, ability1, ref pathToHeal,
                                                                            healerAbilityCall, conditionFunction, null, true))
            {
                if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyHealer, reachableTiles, iaEntityFunction, ability1, ref pathToHeal,
                                                                                healerAbilityCall, conditionFunction, null, true))
                {
                    if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyMinion, reachableTiles, iaEntityFunction, ability1, ref pathToHeal,
                                                                                    healerAbilityCall, conditionFunction, null, true))
                    {
                        for (int i = 0; i < pathToHeal.Count; i++)
                        {
                            if (IAUtils.MoveAndTriggerAbilityIfNeed(healer, pathToHeal[i], iaEntityFunction, true, healerAbilityCall, ability1, pathToHeal[i].castTile))
                            {
                                ability1Use = true;
                                return true;
                            }
                        }

                        return false;
                    }
                }
            }
        }

        ability1Use = true;
        return true;
    }

    /*
     * Essaye d'attaquer sans ce deplacer
     */
    private bool Attack()
    {
        if (ability2Use) return false;
        if (healer.CurrentActionPoints < ability2.cost) return false;

        reachableTiles = new List<ReachableTile>() { new ReachableTile(new List<TileData>() { healer.currentTile }, 0) };
        IAUtils.GetPlayerInRange(reachableTiles, ability2, ref playerHealerPathToAttack, ref playerDPSPathToAttack, ref playerTankPathToAttack, playerHealer, playerDPS, playerTank);

        if (IAUtils.AttackWithPriority(healer, playerHealerPathToAttack, playerDPSPathToAttack, playerTankPathToAttack, iaEntityFunction, healerAbilityCall, ability2))
        {
            ability2Use = true;
            return true;
        }

        return false;
    }
        

    /*
     * Cherche quelle allie est le plus proche dans l'ordre : Tank > DPS > Healer > Minion
     */
    private bool MoveToShortestAlly()
    {
        reachableTiles = IAUtils.FindAllReachablePlace(healer.GetPosition(), healer.CurrentActionPoints, true, false, true, true);
        
        if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyTank, reachableTiles, iaEntityFunction, null, null, ability1, true))
        {
            if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyDPS, reachableTiles, iaEntityFunction, null, null, ability1, true))
            {
                if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyMinion, reachableTiles, iaEntityFunction, null, null, ability1, true))
                {
                    if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyTank, reachableTiles, iaEntityFunction, null, null, null, true))
                    {
                        if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyDPS, reachableTiles, iaEntityFunction, null, null, null, true))
                        {
                            if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyMinion, reachableTiles, iaEntityFunction, null, null, null, true))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }

        haveEndTurn = true;
        return true;
    }

    /*
     * Cherche la tile la plus eloigne des trois perso du player dans la range du healer et lui dit d'y aller
     */
    private void RunAtMaxDistanceOfAll()
    {
        ReachableTile bestReachableTile = null;
        float bestMoyenne = -1;
        float furthestTile;

        reachableTiles = IAUtils.FindAllReachablePlace(healer.GetPosition(), healer.CurrentActionPoints, true);

        for (int i = 0; i < reachableTiles.Count; i++)
        {
            furthestTile = 0;
            if (playerHealer != null) furthestTile += Vector2Int.Distance(reachableTiles[i].GetCoordPosition(), playerHealer.GetPosition());
            if (playerDPS != null) furthestTile += Vector2Int.Distance(reachableTiles[i].GetCoordPosition(), playerDPS.GetPosition());
            if (playerTank != null) furthestTile += Vector2Int.Distance(reachableTiles[i].GetCoordPosition(), playerTank.GetPosition());

            if (bestMoyenne <= furthestTile)
            {
                bestReachableTile = reachableTiles[i];
                bestMoyenne = furthestTile;
            }
        }

        if (IAUtils.MoveAndTriggerAbilityIfNeed(healer, bestReachableTile, iaEntityFunction))
        {
            isFarAway = true;
        }
    }

    /*
     * Return true si "entity" a perdu "lifeLoseForPrio" ou plus de HP
     */
    private bool ConditionHealthToHeal(EntityBehaviour entity)
    {
        return (entity.GetMaxHealth() - entity.CurrentHealth >= lifeLoseForPrio);
    }
}