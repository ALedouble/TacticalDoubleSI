using System;
using System.Collections.Generic;
using System.Linq;

public class DPS : Brain
{
    IAUtils.IAEntity iaEntityFunction;
    IAUtils.SpecificConditionEntity conditionFunction;
    IAUtils.LambdaAbilityCall dpsAbilityCall;

    EntityBehaviour dps;
    List<ReachableTile> reachableTiles;

    List<EntityBehaviour> enemyTank = new List<EntityBehaviour>();
    List<EntityBehaviour> enemyHealer = new List<EntityBehaviour>();

    EntityBehaviour playerHealer = null;
    EntityBehaviour playerDPS = null;
    EntityBehaviour playerTank = null;

    ReachableTile playerHealerPathToAttack = null;
    ReachableTile playerDPSPathToAttack = null;
    ReachableTile playerTankPathToAttack = null;

    static string nameAbility1 = "Cac";
    static Ability ability1;
    static bool ability1Use = false;

    static string nameAbility2 = "Dist";
    static Ability ability2;
    static bool ability2Use = false;

    static int percentOfLifeNeedForAttackPrio = 25;
    static int percentOfLifeNeedForHealer = 25;
    static int percentOfLifeNeedForTank = 50;
    static bool lastAttack = false;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        iaEntityFunction = IA_DPS;
        conditionFunction = SpecificConditionForMove;
        dpsAbilityCall = IAUtils.LambdaAbilityCallDelegate;

        dps = entityBehaviour;

        iaEntityFunction();
    }

    /*
     * Gere un deplacement/attack du Minion
     */
    private void IA_DPS()
    {
        IAUtils.GetAbility(dps, nameAbility1, nameAbility2, ref ability1, ref ability2);

        IAUtils.GetAllEntity(dps, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank, ref enemyHealer);

        reachableTiles = IAUtils.FindAllReachablePlace(dps.GetPosition(), dps.CurrentActionPoints, true);


        //if (IAUtils.CheckEndTurn(dps, CanMakeAction())) return;


        if (GoToHealer()) return;

        if (GoToTank()) return;

        /*
        if (Attack())

        if (GoVersPrio())
        */
               
    }

    private bool GoVersPrio()
    {
        throw new NotImplementedException();
    }


    private bool SpecificConditionForMove(EntityBehaviour target)
    {
        throw new NotImplementedException();
    }

    /*
     * Verifie si le DPS peut encore effectue une action 
     */
    private bool CanMakeAction()
    {
        

        return false;
    }

    /*
     * Se rend au pres du Healer le plus proche, attaque a distance une fois a cote du healer (si possible --> s'il y a une cible), fini son tour.
     * 
     * S'il n'y a pas de Healer ou que la vie n'est pas inferieur à "percentOfLifeNeedForHealer", return false et continue
     */
    private bool GoToHealer()
    {
        if (dps.CurrentHealth < ((dps.GetMaxHealth() * percentOfLifeNeedForHealer) / 100))
        {
            List<Tuple<ReachableTile, EntityBehaviour>> listOfPathToHealer = IAUtils.PathToCastOrToJoin(true, IAUtils.GetReachableTileFromCastOrPathDelegate, dps, enemyHealer, reachableTiles, null);

            if (listOfPathToHealer != null)
            {
                ReachableTile pathToHealer;
                EntityBehaviour healer;

                listOfPathToHealer[0].Deconstruct(out pathToHealer, out healer);

                List<TileData> zoneCast = new List<TileData>() { pathToHealer.GetLastTile() };

                if (IAUtils.ValidCastFromTile(ability2.effectArea, zoneCast, playerHealer.currentTile.GetCoordPosition()).Count > 0)
                {
                    IAUtils.MoveAndTriggerAbilityIfNeed(dps, pathToHealer, null, true, dpsAbilityCall, ability2, playerHealer.currentTile);
                }

                else if (IAUtils.ValidCastFromTile(ability2.effectArea, zoneCast, playerDPS.currentTile.GetCoordPosition()).Count > 0)
                {
                    IAUtils.MoveAndTriggerAbilityIfNeed(dps, pathToHealer, null, true, dpsAbilityCall, ability2, playerDPS.currentTile);
                }

                else if (IAUtils.ValidCastFromTile(ability2.effectArea, zoneCast, playerTank.currentTile.GetCoordPosition()).Count > 0)
                {
                    IAUtils.MoveAndTriggerAbilityIfNeed(dps, pathToHealer, null, true, dpsAbilityCall, ability2, playerTank.currentTile);
                }

                else
                {
                    IAUtils.MoveAndTriggerAbilityIfNeed(dps, pathToHealer, null);
                }

                IAUtils.CheckEndTurn(dps, CanMakeAction(), true);
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
        if (dps.CurrentHealth < ((dps.GetMaxHealth() * percentOfLifeNeedForTank) / 100))
        {
            List<Tuple<ReachableTile, EntityBehaviour>> listOfPathToTank = IAUtils.PathToCastOrToJoin(true, IAUtils.GetReachableTileFromCastOrPathDelegate, dps, enemyTank, reachableTiles, null);
            
            if (listOfPathToTank != null)
            {
                return WalkOnTargetAndSeeToUseAbilityDuringThePath(listOfPathToTank[0]);
            }
        }

        return false;
    }

    private bool Attack()
    {
        Tuple<ReachableTile, EntityBehaviour> target = FindPriorityForAllEntity();

        if (target == null) return false;

        return WalkOnTargetAndSeeToUseAbilityDuringThePath(target);
    }

    private Tuple<ReachableTile, EntityBehaviour> FindPriorityForAllEntity()
    {
        List<ReachableTile> tilesToCastOnEntity;
        reachableTiles = IAUtils.FindAllReachablePlace(dps.GetPosition(), dps.CurrentActionPoints - ability1.cost, true);

        if (playerHealer.CurrentHealth < ((playerHealer.GetMaxHealth() * percentOfLifeNeedForAttackPrio) / 100))
        {
            tilesToCastOnEntity = IAUtils.ValidCastFromTile(ability1.effectArea, reachableTiles, playerHealer.GetPosition());
            if (tilesToCastOnEntity.Count > 0)
            {
                return new Tuple<ReachableTile, EntityBehaviour>(tilesToCastOnEntity[0], playerHealer);
            }
        }

        if(playerDPS.CurrentHealth < ((playerDPS.GetMaxHealth() * percentOfLifeNeedForAttackPrio) / 100))
        {
            tilesToCastOnEntity = IAUtils.ValidCastFromTile(ability1.effectArea, reachableTiles, playerDPS.GetPosition());
            if (tilesToCastOnEntity.Count > 0)
            {
                return new Tuple<ReachableTile, EntityBehaviour>(tilesToCastOnEntity[0], playerDPS);
            }
        }

        if (playerTank.CurrentHealth < ((playerTank.GetMaxHealth() * percentOfLifeNeedForAttackPrio) / 100))
        {
            tilesToCastOnEntity = IAUtils.ValidCastFromTile(ability1.effectArea, reachableTiles, playerTank.GetPosition());
            if (tilesToCastOnEntity.Count > 0)
            {
                return new Tuple<ReachableTile, EntityBehaviour>(tilesToCastOnEntity[0], playerTank);
            }
        }

        tilesToCastOnEntity = IAUtils.ValidCastFromTile(ability1.effectArea, reachableTiles, playerHealer.GetPosition());
        if (tilesToCastOnEntity.Count > 0)
        {
            return new Tuple<ReachableTile, EntityBehaviour>(tilesToCastOnEntity[0], playerHealer);
        }

        tilesToCastOnEntity = IAUtils.ValidCastFromTile(ability1.effectArea, reachableTiles, playerDPS.GetPosition());
        if (tilesToCastOnEntity.Count > 0)
        {
            return new Tuple<ReachableTile, EntityBehaviour>(tilesToCastOnEntity[0], playerDPS);
        }

        tilesToCastOnEntity = IAUtils.ValidCastFromTile(ability1.effectArea, reachableTiles, playerTank.GetPosition());
        if (tilesToCastOnEntity.Count > 0)
        {
            return new Tuple<ReachableTile, EntityBehaviour>(tilesToCastOnEntity[0], playerTank);
        }

        return null;
    }


    private bool WalkOnTargetAndSeeToUseAbilityDuringThePath(Tuple<ReachableTile, EntityBehaviour> listOfPathToTank)
    {
        ReachableTile pathToTank;
        EntityBehaviour tank;

        listOfPathToTank.Deconstruct(out pathToTank, out tank);

        if (ability2Use)
        {
            IAUtils.MoveAndTriggerAbilityIfNeed(dps, pathToTank, null);
            IAUtils.CheckEndTurn(dps, CanMakeAction(), true);
            return true;
        }

        List<TileData> zoneCast = null;
        EntityBehaviour entityCast = null;
        for (int i = 0; i < pathToTank.path.Count; i++)
        {
            zoneCast = new List<TileData>() { pathToTank.path[i] };

            if (IAUtils.ValidCastFromTile(ability2.effectArea, zoneCast, playerHealer.currentTile.GetCoordPosition()).Count > 0)
            {
                entityCast = playerHealer;
                break;
            }

            else if (IAUtils.ValidCastFromTile(ability2.effectArea, zoneCast, playerDPS.currentTile.GetCoordPosition()).Count > 0)
            {
                entityCast = playerDPS;
                break;
            }

            else if (IAUtils.ValidCastFromTile(ability2.effectArea, zoneCast, playerTank.currentTile.GetCoordPosition()).Count > 0)
            {
                entityCast = playerTank;
                break;
            }
        }

        bool canCast = true;
        if (entityCast != null)
        {
            ReachableTile intermediateTile = IAUtils.FindShortestPath(false, dps.GetPosition(), zoneCast[0].GetCoordPosition());

            if (intermediateTile.cost + ability2.cost <= dps.CurrentActionPoints)
            {
                ability2Use = true;
                IAUtils.MoveAndTriggerAbilityIfNeed(dps, intermediateTile, iaEntityFunction, true, dpsAbilityCall, ability2, entityCast.currentTile);
            }

            else
            {
                canCast = false;
            }
        }

        if (entityCast == null || !canCast)
        {
            IAUtils.MoveAndTriggerAbilityIfNeed(dps, pathToTank, null);
            IAUtils.CheckEndTurn(dps, CanMakeAction(), true);
            return true;
        }

        return false;
    }





















}