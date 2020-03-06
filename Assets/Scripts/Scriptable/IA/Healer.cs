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
            if (MoveToSlowestAly()) return;
        }



        //
        // End
        //




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
     * Cherche quelle alie peut et doit etre Heal parmis Tank > DPS > Healer > Minion
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
        List<Tuple<List<ReachableTile>, EntityBehaviour>> listOfTilesPossibleForHeal = new List<Tuple<List<ReachableTile>, EntityBehaviour>>();

        // La liste des combinaisons de "EntityBehaviour" et de leur list de Tiles, a partir desquels "EntityBehaviour" peut etre Heal (peut etre list vide)
        for (int i = 0; i < listEntity.Count; i++)
        {
            listOfTilesPossibleForHeal.Add(new Tuple<List<ReachableTile>, EntityBehaviour>(IAUtils.ValidCastFromTile(ability1.effectArea, reachableTiles, listEntity[i].GetPosition()), listEntity[i]));
        }

        // La liste des combinaisons de "EntityBehaviour" et de la Tile la moins couteuse, a partir desquels "EntityBehaviour" peut etre Heal
        List<Tuple<ReachableTile, EntityBehaviour>> allEntitiesReachableBestTileForCast = new List<Tuple<ReachableTile, EntityBehaviour>>();
        for (int i = 0; i < listOfTilesPossibleForHeal.Count; i++)
        {
            if (listOfTilesPossibleForHeal[i].Item1.Count > 0)
            {
                allEntitiesReachableBestTileForCast.Add(new Tuple<ReachableTile, EntityBehaviour>(listOfTilesPossibleForHeal[i].Item1[0], listOfTilesPossibleForHeal[i].Item2));
            }

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
     * 
     */
    private bool MoveToSlowestAly()
    {
        throw new NotImplementedException();
    }

    /*
     * Return true si "entity" a perdu "lifeLoseForPrio" ou plus de HP
     */
    private bool ConditionHealthToHeal(EntityBehaviour entity)
    {
        return (entity.GetMaxHealth() - entity.CurrentHealth >= lifeLoseForPrio);
    }
}