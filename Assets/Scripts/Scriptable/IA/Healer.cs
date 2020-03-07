using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class Healer : Brain
{
    IAUtils.IAEntity iaEntityFunction;
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

    string nameAbility1 = "Heal";
    Ability ability1;
    bool ability1Use = false;

    string nameAbility2 = "Attack";
    Ability ability2;
    bool ability2Use = false;

    int lifeLoseForPrio = 6;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        iaEntityFunction = IAHealer;
        healerAbilityCall = IAUtils.LambdaAbilityCallDelegate;

        healer = entityBehaviour;
        IAHealer();
    }

    /*
     * Gere un deplacement/attack du Healer
     */
    private void IAHealer(bool parameter = false)
    {
        IAUtils.GetAbility(healer, nameAbility1, nameAbility2, ref ability1, ref ability2);

        IAUtils.GetAllEntity(healer, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank, ref enemyDPS, ref enemyHealer, ref enemyMinion);

        IAUtils.CheckEndTurn(healer, CanMakeAction());

        // Si le healer est le dernier enemy || si un player est a cote || s'il n'a plus toute sa vie
        if (Solo() || IAUtils.HaveXEntityAround(Alignement.Player, healer.currentTile) || healer.CurrentHealth < healer.GetMaxHealth())
        {
            reachableTiles = IAUtils.FindAllReachablePlace(healer.GetPosition(), healer.CurrentActionPoints, true);
            RunAtMaxDistanceOfAll();
            return;
        }

        else if (!ability1Use)
        {
            reachableTiles = IAUtils.FindAllReachablePlace(healer.GetPosition(), healer.CurrentActionPoints - ability1.cost, true);
            if (Heal()) return;
        }

        else if (!ability2Use)
        {
            reachableTiles = IAUtils.FindAllReachablePlace(healer.GetPosition(), healer.CurrentActionPoints - ability2.cost, true);
            IAUtils.GetPlayerInRange(reachableTiles, ability2.effectArea, ref playerHealerPathToAttack, ref playerDPSPathToAttack, ref playerTankPathToAttack, healer, playerHealer, playerDPS, playerTank);
            if (IAUtils.AttackWithPriority(healer, playerHealerPathToAttack, playerDPSPathToAttack, playerTankPathToAttack, iaEntityFunction,
                                        healerAbilityCall, ability2, playerHealer.currentTile, playerDPS.currentTile, playerTank.currentTile)) return;
        }

        else
        {
            if (MoveToShortestAlly()) return;
        }

        IAUtils.CheckEndTurn(healer, CanMakeAction(), true);
    }

    /*
     * Verifie si le Healer peut encore effectue une action 
     */
    private bool CanMakeAction()
    {
        if (!ability1Use && healer.CurrentActionPoints >= ability1.cost) return true;
        if (!ability2Use && healer.CurrentActionPoints >= ability2.cost) return true;

        List<TileData> around = IAUtils.TilesAround(healer.currentTile);
        for (int i = 0; i < around.Count; i++)
        {
            if (around[i].IsWalkable && (int)around[i].tileType <= healer.CurrentActionPoints)
            {
                return true;
            }
        }

        return false;
    }

    /*
     * Regarde s'il est le dernier des enemy restant
     */
    private bool Solo()
    {
        if (enemyDPS.Count.Equals(0) && enemyTank.Count.Equals(0) && enemyMinion.Count.Equals(0) && enemyHealer.Count.Equals(0))
            return true;

        return false;
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

        for (int i = 0; i < reachableTiles.Count; i++)
        {
            playerHealerPath = IAUtils.FindShortestPath(reachableTiles[i].GetCoordPosition(), playerHealer.GetPosition());
            playerDPSPath = IAUtils.FindShortestPath(reachableTiles[i].GetCoordPosition(), playerDPS.GetPosition());
            playerTankPath = IAUtils.FindShortestPath(reachableTiles[i].GetCoordPosition(), playerTank.GetPosition());

            float moyenne = MoyenneDistance(i);
            if (bestMoyenne <= moyenne)
            {
                bestReachableTile = reachableTiles[i];
                bestMoyenne = moyenne;
            }
        }

        IAUtils.MoveAndTriggerAbilityIfNeed(healer, bestReachableTile, iaEntityFunction);


        float MoyenneDistance(int index)
        {
            float somme = 0;
            int total = 0;

            if (playerHealerPath != null)
            {
                somme += reachableTiles[index].cost;
                total++;
            }

            if (playerDPSPath != null)
            {
                somme += reachableTiles[index].cost;
                total++;
            }

            if (playerTankPath != null)
            {
                somme += reachableTiles[index].cost;
                total++;
            }

            if (total == 0) return -1;
            return somme / total;
        }
    }

    /*
     * Cherche quelle allie peut et doit etre Heal parmis Tank > DPS > Healer > Minion
     */
    private bool Heal()
    {
        ReachableTile pathToHeal = null;
        EntityBehaviour entityToHeal = null;

        if (!HealOneType(enemyTank, ref pathToHeal, ref entityToHeal))
        {
            if (!HealOneType(enemyDPS, ref pathToHeal, ref entityToHeal))
            {
                if (!HealOneType(enemyHealer, ref pathToHeal, ref entityToHeal))
                {
                    if (!HealOneType(enemyMinion, ref pathToHeal, ref entityToHeal))
                    {
                        if (pathToHeal != null)
                        {
                            return IAUtils.MoveAndTriggerAbilityIfNeed(healer, pathToHeal, iaEntityFunction, true, false, healerAbilityCall, ability1, entityToHeal.currentTile);
                        }
                    }
                }
            }
        }

        return false;
    }

    /*
     * Regade si on peut Heal une entity de "listEntity" avec la condition "ConditionHealthToHeal" accepte
     * 
     * Si oui, le fait et return null
     * Sinon, return le plus à meme a recevoir le soin.
     */
    private bool HealOneType(List<EntityBehaviour> listEntity, ref ReachableTile pathToHeal, ref EntityBehaviour entityToHeal)
    {
        // La liste des combinaisons de "EntityBehaviour" et de la Tile la moins couteuse, a partir desquels "EntityBehaviour" peut etre Heal
        List<Tuple<ReachableTile, EntityBehaviour>> allEntitiesReachableBestTileForCast = new List<Tuple<ReachableTile, EntityBehaviour>>();
        for (int i = 0; i < listEntity.Count; i++)
        {
            List<ReachableTile> canCastAbility = IAUtils.ValidCastFromTile(ability1.effectArea, reachableTiles, listEntity[i].GetPosition());
            if (canCastAbility.Count > 0) allEntitiesReachableBestTileForCast.Add(new Tuple<ReachableTile, EntityBehaviour>(canCastAbility[0], listEntity[i]));
        }

        allEntitiesReachableBestTileForCast.Sort((x, y) => x.Item1.CompareTo(x.Item2));
        for (int i = 0; i < allEntitiesReachableBestTileForCast.Count; i++)
        {
            if (IAUtils.MoveAndTriggerAbilityIfNeed(healer, allEntitiesReachableBestTileForCast[i].Item1, iaEntityFunction, ConditionHealthToHeal(allEntitiesReachableBestTileForCast[i].Item2),
                                                    false, healerAbilityCall, ability1, allEntitiesReachableBestTileForCast[i].Item2.currentTile))
            {
                return true;
            }

            else if (pathToHeal == null)
            {
                pathToHeal = allEntitiesReachableBestTileForCast[i].Item1;
                entityToHeal = allEntitiesReachableBestTileForCast[i].Item2;
            }
        }

        return false;
    }


    /*
     * Cherche quelle allie est a la fois le plus proche, et peut recevoir "ability1" du healer dans l'ordre : Tank > DPS > Healer > Minion
     */
    private bool MoveToShortestAlly()
    {
        if (!MoveToShortestAllyOfOneType(enemyTank))
        {
            if (!MoveToShortestAllyOfOneType(enemyDPS))
            {
                if (!MoveToShortestAllyOfOneType(enemyHealer))
                {
                    if (!MoveToShortestAllyOfOneType(enemyMinion))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    /*
     * Regade si on peut se rapproche d'une entity de "listEntity" au point d'utiliser "ability1" sur lui
     * 
     * Si oui, se rapproche sans utiliser "ability1" et return true
     * Sinon, return false
     */
    private bool MoveToShortestAllyOfOneType(List<EntityBehaviour> listEntity)
    {
        List<ReachableTile> listOfShortestPathToAlly = new List<ReachableTile>();

        for (int i = 0; i < listEntity.Count; i++)
        {
            ReachableTile shortestPathToEntity = IAUtils.FindShortestPath(healer.GetPosition(), listEntity[i].GetPosition(), false, -1, true);
            if (shortestPathToEntity.path.Count > 0) listOfShortestPathToAlly.Add(shortestPathToEntity);
        }

        listOfShortestPathToAlly.Sort();
        for (int i = 0; i < listOfShortestPathToAlly.Count; i++)
        {
            List<ReachableTile> canCastAbility = IAUtils.ValidCastFromTile(ability1.effectArea, MapManager.GetMap(), healer.GetPosition(), listOfShortestPathToAlly[i].GetCoordPosition());
            if (canCastAbility.Count > 0)
            {
                IAUtils.MoveAndTriggerAbilityIfNeed(healer, canCastAbility[0], iaEntityFunction);
                return true;
            }
        }

        return false;
    }

    /*
     * Return true si "entity" a perdu "lifeLoseForPrio" ou plus de HP
     */
    private bool ConditionHealthToHeal(EntityBehaviour entity)
    {
        return (entity.GetMaxHealth() - entity.CurrentHealth >= lifeLoseForPrio);
    }
}