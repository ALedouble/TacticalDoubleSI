using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DPSBrain", menuName = "ScriptableObjects/IA_Brain/DPS_Brain", order = 999)]
public class DPS : Brain
{
    IAUtils.IAEntity iaEntityFunction;
    IAUtils.LambdaAbilityCall dpsAbilityCall;

    EntityBehaviour dps;
    List<ReachableTile> reachableTiles;

    List<EntityBehaviour> enemyTank;
    List<EntityBehaviour> enemyHealer;

    EntityBehaviour playerHealer;
    EntityBehaviour playerDPS;
    EntityBehaviour playerTank;
    List<EntityBehaviour> listOfEntity;

    static Ability ability1;
    static Ability ability2;
    static bool ability2Use;

    static bool haveEndTurn;

    public int percentOfLifeNeedForAttackPrio = 25;
    public int percentOfLifeNeedForRunToHealer = 25;
    public int percentOfLifeNeedForRunToTank = 50;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        iaEntityFunction = IA_DPS;
        dpsAbilityCall = IAUtils.LambdaAbilityCallDelegate;

        Debug.LogError("New DPS");

        Init(entityBehaviour);
        iaEntityFunction();
    }

    private void Init(EntityBehaviour entityBehaviour)
    {
        dps = entityBehaviour;

        enemyTank = new List<EntityBehaviour>();
        enemyHealer = new List<EntityBehaviour>();

        playerHealer = null;
        playerDPS = null;
        playerTank = null;

        ability1 = dps.GetAbilities(0);
        ability2 = dps.GetAbilities(1);
        ability2Use = false;

        haveEndTurn = false;
    }

    /*
     * Gere un deplacement/attack du Minion
     */
    private void IA_DPS()
    {
        IAUtils.GetAllEntity(dps, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank, ref enemyHealer);

        listOfEntity = new List<EntityBehaviour>() { playerHealer, playerDPS, playerTank };
        reachableTiles = IAUtils.FindAllReachablePlace(dps.GetPosition(), dps.CurrentActionPoints, true);


        if (IAUtils.CheckEndTurn(dps, CanMakeAction())) return;

        if (GoToHealer()) return;

        if (GoToTank()) return;

        if (Attack()) return;

        WalkVersPrio();
               
    }

    /*
     * Verifie si le DPS peut encore effectue une action 
     */
    private bool CanMakeAction()
    {
        if (haveEndTurn) return false;
        if (dps.CurrentActionPoints >= ability1.cost) return true;
        if (!ability2Use && dps.CurrentActionPoints >= ability2.cost) return true;

        return IAUtils.CanWalkAround(dps, dps.CurrentActionPoints);
    }

    /*
     * Se rend au pres du Healer le plus proche, attaque a distance une fois a cote du healer (si possible --> s'il y a une cible), fini son tour.
     * 
     * S'il n'y a pas de Healer ou que la vie n'est pas inferieur à "percentOfLifeNeedForHealer", return false et continue
     */
    private bool GoToHealer()
    {
        if (dps.CurrentHealth < ((dps.GetMaxHealth() * percentOfLifeNeedForRunToHealer) / 100))
        {
            List<Tuple<ReachableTile, EntityBehaviour>> listOfPathToHealer = IAUtils.PathToCastOrToJoin(true, IAUtils.GetReachableTileFromCastOrPathDelegate, dps, enemyHealer, reachableTiles, null);

            if (listOfPathToHealer != null)
            {
                ReachableTile pathToHealer;
                EntityBehaviour healer;

                listOfPathToHealer[0].Deconstruct(out pathToHealer, out healer);

                List<TileData> zoneCast = new List<TileData>() { pathToHealer.GetLastTile() };
                bool canCast = false;

                for (int i = 0; i < listOfEntity.Count; i++)
                {
                    TileData tileToCast = IAUtils.ValidCastFromTile(ability2, zoneCast, listOfEntity[i].currentTile.GetCoordPosition());
                    if (tileToCast != null)
                    {
                        canCast = IAUtils.MoveAndTriggerAbilityIfNeed(dps, pathToHealer, iaEntityFunction, true, dpsAbilityCall, ability2, tileToCast);
                        break;
                    }
                }

                if (!canCast)
                {
                    if (!IAUtils.MoveAndTriggerAbilityIfNeed(dps, pathToHealer, iaEntityFunction)) return false;
                }
                else
                {
                    ability2Use = true;
                }

                haveEndTurn = true;
                return true;
            }
        }

        return false;
    }

    /*
     * Se rend au pres du Tank le plus proche, attaque a distance une fois sur le chemin (si possible --> s'il y a une cible), fini son tour.
     * 
     * S'il n'y a pas de Tank ou que la vie n'est pas inferieur à "percentOfLifeNeedForTank", return false et continue
     */
    private bool GoToTank()
    {
        if (dps.CurrentHealth < ((dps.GetMaxHealth() * percentOfLifeNeedForRunToTank) / 100))
        {
            List<Tuple<ReachableTile, EntityBehaviour>> listOfPathToTank = IAUtils.PathToCastOrToJoin(true, IAUtils.GetReachableTileFromCastOrPathDelegate, dps, enemyTank, reachableTiles, null);
            
            if (listOfPathToTank != null)
            {
                if(!SeeToUseAbilityDuringThePathToTarget(listOfPathToTank[0], false))
                {
                    if (IAUtils.MoveAndTriggerAbilityIfNeed(dps, listOfPathToTank[0].Item1, iaEntityFunction))
                    {
                        haveEndTurn = true;
                        return true;
                    }
                }

                else return true;
            }
        }

        return false;
    }

    /*
     * Trouve le player a attaquer en priorite
     * Regarde et attaque a distance s'il trouve un enemy sur le chemin
     * Attaque l'enemy en priorite
     * 
     * return true s'il a pu attaquer
     * return false sinon
     */
    private bool Attack()
    {
        reachableTiles = IAUtils.FindAllReachablePlace(dps.GetPosition(), dps.CurrentActionPoints - ability1.cost, true);
        Tuple<ReachableTile, EntityBehaviour> target = FindPriorityForAllEntity();

        if (target == null) return false;

        if (!SeeToUseAbilityDuringThePathToTarget(target, true))
        {
            return IAUtils.MoveAndTriggerAbilityIfNeed(dps, target.Item1, iaEntityFunction, true, dpsAbilityCall, ability1, target.Item1.castTile);
        }

        return true;
    }

    /*
     * Regarde vers quel entity il va se deplace selon l'ordre de priorite :
     *          Heal (percentOfLifeNeedForAttackPrio % HP) > DPS (percentOfLifeNeedForAttackPrio % HP) > Tank (percentOfLifeNeedForAttackPrio % HP) > Heal > DPS > Tank
     */
    private void WalkVersPrio()
    {
        reachableTiles = IAUtils.FindAllReachablePlace(dps.GetPosition(), dps.CurrentActionPoints, true);
        Tuple<ReachableTile, EntityBehaviour> target = FindPriorityForAllEntity();

        if (target != null)
        { 
            IAUtils.MoveAndTriggerAbilityIfNeed(dps, target.Item1, iaEntityFunction);
        }
    }

    /*
     * Trouve parmis les unite du player, laquelle le dps doit cibler en priorite
     */
    private Tuple<ReachableTile, EntityBehaviour> FindPriorityForAllEntity()
    {
        Tuple<ReachableTile, EntityBehaviour> tilesToCastOnEntity;
        List<bool> conditionOnPlayer = new List<bool>() { true, false };
        
        for (int i = 0; i < conditionOnPlayer.Count; i++)
        {
            for (int j = 0; j < listOfEntity.Count; j++)
            {
                tilesToCastOnEntity = FindPriorityForEntity(listOfEntity[j], conditionOnPlayer[i]);
                if (tilesToCastOnEntity != null) return tilesToCastOnEntity;
            }
        }

        return null;
    }

    /*
     Regarde si le dps doit cibler entity en priorite (entity pouvant avoir une condition sur ses HP)
     */
    private Tuple<ReachableTile, EntityBehaviour> FindPriorityForEntity(EntityBehaviour entity, bool haveAConditionOnEntity)
    {
        List<ReachableTile> tilesToCastOnEntity;

        if (!haveAConditionOnEntity || entity.CurrentHealth < ((entity.GetMaxHealth() * percentOfLifeNeedForAttackPrio) / 100))
        {
            tilesToCastOnEntity = IAUtils.ValidCastFromTile(ability1, reachableTiles, entity.GetPosition());
            if (tilesToCastOnEntity.Count > 0)
            {
                return new Tuple<ReachableTile, EntityBehaviour>(tilesToCastOnEntity[0], entity);
            }
        }

        return null;
    }

    /*
     * Regarde dans le chemin de "listOfPathToEntityCac" si l'on peut attaquer a distance un ennemie (il y a un enemy ?, ai-je deja utilise "ability2" ?)   
     *      
     * Si l'utilisation d'"ability2" est possible, on l'utilise et return true.
     * Sinon, return false.
     */
    private bool SeeToUseAbilityDuringThePathToTarget(Tuple<ReachableTile, EntityBehaviour> listOfPathToEntityCac, bool priorityOnRun)
    {
        ReachableTile pathToEntityCac;
        EntityBehaviour entityCac;

        listOfPathToEntityCac.Deconstruct(out pathToEntityCac, out entityCac);

        if (ability2Use) return false;

        List<TileData> zoneCastDist = null;
        for (int i = 0; i < pathToEntityCac.path.Count; i++)
        {
            zoneCastDist = new List<TileData>() { pathToEntityCac.path[i] };
            for (int j = 0; j < listOfEntity.Count; j++)
            {
                TileData tileToCast = IAUtils.ValidCastFromTile(ability2, zoneCastDist, listOfEntity[j].currentTile.GetCoordPosition());
                if (tileToCast != null)
                {
                    return UseAbilityDuringThePathToTarget(pathToEntityCac, tileToCast, zoneCastDist, priorityOnRun);
                }
            }
        }

        return false;
    }

    /*
     * Utilise "ability2" si possible :
     * 
     * Soit on prioritise la Run vers "listOfPathToEntityCac" et alors on attaquera uniquement si le cout de la Run plus de "ability2" est inferieur au nombre de PA restant
     * Sinon, si on ne prioritise pas la Run, alors on verifie seulement si l'utilisation d'"ability2" et du deplacent pour le caster est inferieur au nombre de PA restant.
     */
    private bool UseAbilityDuringThePathToTarget(ReachableTile pathToEntityCac, TileData tileToCast, List<TileData> zoneCastDist, bool priorityOnRun)
    {
        ReachableTile intermediateTile = IAUtils.FindShortestPath(false, dps.GetPosition(), zoneCastDist[0].GetCoordPosition());

        int deplacementCost;
        if (priorityOnRun) deplacementCost = pathToEntityCac.cost;
        else deplacementCost = intermediateTile.cost;

        if (deplacementCost + ability2.cost <= dps.CurrentActionPoints)
        {
            if(IAUtils.MoveAndTriggerAbilityIfNeed(dps, intermediateTile, iaEntityFunction, true, dpsAbilityCall, ability2, tileToCast))
            {
                ability2Use = true;
                return true;
            }
        }

        return false;
    }
}