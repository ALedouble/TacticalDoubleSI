using System.Collections.Generic;

public class Healer : Brain
{
    IAUtils.IAEntity iaEntityFunction;
    IAUtils.SpecificConditionEntity conditionFunction;
    IAUtils.LambdaAbilityCall healerAbilityCall;

    EntityBehaviour healer;
    List<ReachableTile> reachableTiles;

    List<EntityBehaviour> enemyTank = new List<EntityBehaviour>();
    List<EntityBehaviour> enemyDPS = new List<EntityBehaviour>();
    List<EntityBehaviour> enemyHealer = new List<EntityBehaviour>();
    List<EntityBehaviour> enemyMinion = new List<EntityBehaviour>();

    EntityBehaviour playerHealer = null;
    EntityBehaviour playerDPS = null;
    EntityBehaviour playerTank = null;

    ReachableTile playerHealerPathToAttack = null;
    ReachableTile playerDPSPathToAttack = null;
    ReachableTile playerTankPathToAttack = null;

    static string nameAbility1 = "Heal";
    static Ability ability1;
    static bool ability1Use;

    static string nameAbility2 = "Attack";
    static Ability ability2;
    static bool ability2Use;

    static int lifeLoseForPrio = 6;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        iaEntityFunction = IAHealer;
        conditionFunction = ConditionHealthToHeal;
        healerAbilityCall = IAUtils.LambdaAbilityCallDelegate;

        healer = entityBehaviour;
        ability1Use = false;
        ability2Use = false;

        iaEntityFunction();
    }

    /*
     * Gere un deplacement/attack du Healer
     */
    private void IAHealer()
    {
        IAUtils.GetAbility(healer, nameAbility1, nameAbility2, ref ability1, ref ability2);
        IAUtils.GetAllEntity(healer, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank, ref enemyDPS, ref enemyHealer, ref enemyMinion);


        IAUtils.CheckEndTurn(healer, CanMakeAction());

        if (IsInDanger()) return;

        else if (Heal()) { ability1Use = true; return; }

        else if (Attack()) { ability2Use = true; return; }

        else if (MoveToShortestAlly()) return;

        RunAtMaxDistanceOfAll();
    }

    /*
     * Verifie si le Healer peut encore effectue une action 
     */
    private bool CanMakeAction()
    {
        if (!ability1Use && healer.CurrentActionPoints >= ability1.cost) return true;
        if (!ability2Use && healer.CurrentActionPoints >= ability2.cost) return true;

        return IAUtils.CanWalkAround(healer, healer.CurrentActionPoints);
    }

    /*
     * Si le healer est le dernier enemy || si un player est a cote || s'il n'a plus toute sa vie
     */
    private bool IsInDanger()
    {
        if (Solo() || IAUtils.HaveXEntityAround(Alignement.Player, healer.currentTile) || healer.CurrentHealth < healer.GetMaxHealth())
        {
            RunAtMaxDistanceOfAll();
            return true;
        }

        return false;

        bool Solo()
        {
            if (enemyDPS.Count.Equals(0) && enemyTank.Count.Equals(0) && enemyMinion.Count.Equals(0) && enemyHealer.Count.Equals(0))
                return true;

            return false;
        }
    }

    /*
     * Cherche quelle allie peut et doit etre Heal parmis Tank > DPS > Healer > Minion
     */
    private bool Heal()
    {
        if (ability1) return false;

        ReachableTile pathToHeal = null;
        EntityBehaviour entityToHeal = null;

        reachableTiles = IAUtils.FindAllReachablePlace(healer.GetPosition(), healer.CurrentActionPoints - ability1.cost, true);

        if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyTank, reachableTiles, iaEntityFunction, ability1, ref pathToHeal, ref entityToHeal,
                                                                        healerAbilityCall, conditionFunction))
        {
            if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyDPS, reachableTiles, iaEntityFunction, ability1, ref pathToHeal, ref entityToHeal,
                                                                            healerAbilityCall, conditionFunction))
            {
                if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyHealer, reachableTiles, iaEntityFunction, ability1, ref pathToHeal, ref entityToHeal,
                                                                                healerAbilityCall, conditionFunction))
                {
                    if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyMinion, reachableTiles, iaEntityFunction, ability1, ref pathToHeal, ref entityToHeal,
                                                                                    healerAbilityCall, conditionFunction))
                    {
                        if (pathToHeal != null)
                        {
                            return IAUtils.MoveAndTriggerAbilityIfNeed(healer, pathToHeal, iaEntityFunction, true, healerAbilityCall, ability1, entityToHeal.currentTile);
                        }
                    }
                }
            }
        }

        return false;
    }

    /*
     * Essaye d'attaquer sans ce deplacer
     */
    private bool Attack()
    {
        if (ability2Use) return false;
        if (healer.CurrentActionPoints < ability2.cost) return false;

        reachableTiles = new List<ReachableTile>() { new ReachableTile(new List<TileData>(), 0) };
        IAUtils.GetPlayerInRange(reachableTiles, ability2.effectArea, ref playerHealerPathToAttack, ref playerDPSPathToAttack, ref playerTankPathToAttack, playerHealer, playerDPS, playerTank);

        return IAUtils.AttackWithPriority(healer, playerHealerPathToAttack, playerDPSPathToAttack, playerTankPathToAttack, iaEntityFunction,
                                        healerAbilityCall, ability2, playerHealer, playerDPS, playerTank);
    }
        

    /*
     * Cherche quelle allie est a la fois le plus proche, et peut recevoir "ability1" du healer dans l'ordre : Tank > DPS > Healer > Minion
     */
    private bool MoveToShortestAlly()
    {
        reachableTiles = new List<ReachableTile>();
        for (int i = 0; i < MapManager.GetMap().Count; i++)
        {
            reachableTiles.Add(IAUtils.FindShortestPath(false, healer.GetPosition(), MapManager.GetMap()[i].GetCoordPosition(), false));
        }

        if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyTank, reachableTiles, iaEntityFunction, null, null, ability1))
        {
            if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyDPS, reachableTiles, iaEntityFunction, null, null, ability1))
            {
                if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyHealer, reachableTiles, iaEntityFunction, null, null, ability1))
                {
                    if (!IAUtils.MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(healer, enemyMinion, reachableTiles, iaEntityFunction, null, null, ability1))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    /*
     * Cherche la tile la plus eloigne des trois perso du player dans la range du healer et lui dit d'y aller
     */
    private void RunAtMaxDistanceOfAll()
    {
        ReachableTile bestReachableTile = null;
        float bestMoyenne = -1;

        ReachableTile playerHealerPath;
        ReachableTile playerDPSPath;
        ReachableTile playerTankPath;

        reachableTiles = IAUtils.FindAllReachablePlace(healer.GetPosition(), healer.CurrentActionPoints, true);

        for (int i = 0; i < reachableTiles.Count; i++)
        {
            playerHealerPath = IAUtils.FindShortestPath(true, reachableTiles[i].GetCoordPosition(), playerHealer.GetPosition(), false, healer.CurrentActionPoints);
            playerDPSPath = IAUtils.FindShortestPath(true, reachableTiles[i].GetCoordPosition(), playerDPS.GetPosition(), false, healer.CurrentActionPoints);
            playerTankPath = IAUtils.FindShortestPath(true, reachableTiles[i].GetCoordPosition(), playerTank.GetPosition(), false, healer.CurrentActionPoints);

            float moyenne = SommeDistance(i);
            if (bestMoyenne <= moyenne)
            {
                bestReachableTile = reachableTiles[i];
                bestMoyenne = moyenne;
            }
        }

        IAUtils.MoveAndTriggerAbilityIfNeed(healer, bestReachableTile, iaEntityFunction);
        IAUtils.CheckEndTurn(healer, CanMakeAction(), true);


        float SommeDistance(int index)
        {
            float somme = 0;

            if (playerHealerPath != null)
                somme += reachableTiles[index].cost;

            if (playerDPSPath != null)
                somme += reachableTiles[index].cost;

            if (playerTankPath != null)
                somme += reachableTiles[index].cost;

            return somme;
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