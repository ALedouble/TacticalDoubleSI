﻿using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum NavigationQueryType { Area, Path };

public static class IAUtils
{
    private static int fixedWeight = 2;

    //#################################################################################################################################################################################################
    //#################################################################################################################################################################################################
    //############################################################################################ PUBLIC #############################################################################################
    //#################################################################################################################################################################################################
    //#################################################################################################################################################################################################


    //#################################################################################################################################################################################################
    //#################################################################################### FONCTION IA DE BASE ########################################################################################
    //#################################################################################################################################################################################################
    /*
     * Trouve l'ensemble des positions atteignables depuis "startPosition"
     */
    public static List<ReachableTile> FindAllReachablePlace(Vector2Int startPosition, int range,
                                                                bool ignoreWeightMove = false, bool ignoreWalkable = false, bool canWalkOnDamageTile = true, bool stopJustBeforeTarget = false)
    {
        List<ReachableTile> reachableTiles = new List<ReachableTile>() { new ReachableTile(new List<TileData>() { MapManager.GetTile(startPosition) }, 0) };

        LookAround(stopJustBeforeTarget, NavigationQueryType.Area, ref reachableTiles, reachableTiles[0], Vector2Int.zero, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove);
        
        return reachableTiles;
    }

    /*
     * Trouve le plus court chemin depuis "startPosition" a "target", calculer par la fonction de l'IA
     */
    public static ReachableTile FindShortestPath(bool stopJustBeforeTarget, Vector2Int startPosition, Vector2Int target, 
                                                    bool canWalkOnDamageTile = true, int range = -1, bool ignoreWeightMove = false, bool ignoreWalkable = false)
    {
        List<ReachableTile> reachableTiles = new List<ReachableTile>() { new ReachableTile(new List<TileData>() { MapManager.GetTile(startPosition) }, 0) };
        List<Vector2Int> deletedPlaces = new List<Vector2Int>();

        if (!startPosition.Equals(target))
        {
            while (reachableTiles.Count > 0 && !LookAround(stopJustBeforeTarget, NavigationQueryType.Path, ref reachableTiles, reachableTiles[0], target, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove))
            {
                deletedPlaces.Add(reachableTiles[0].GetCoordPosition());
                reachableTiles.RemoveAt(0);
                reachableTiles.Sort();

                for (int i = 0; i < reachableTiles.Count; i++)
                {
                    if (deletedPlaces.Contains(reachableTiles[i].GetCoordPosition()))
                    {
                        reachableTiles.RemoveAt(i--);
                    }
                }
            }
        }

        return ReturnPath(stopJustBeforeTarget, reachableTiles, range, ignoreWeightMove);
    }

    /*
     * Regarde dans "reachableTiles" ("tilesOnPath") quels sont les Tiles desquelles on peut lancer l'attaque "attackArea" et toucher "target".
     * 
     * return l'ensemble de ces Tiles.
     */
    public static List<ReachableTile> ValidCastFromTile(Ability ability, List<ReachableTile> reachableTiles, Vector2Int target)
    {
        List<ReachableTile> canCastAndHitTarget = new List<ReachableTile>();
        List<Vector2Int> attackRangeCast = ability.castArea.GetTiles();
        List<Vector2Int> attackRangeEffect;

        for (int i = 0; i < reachableTiles.Count; i++)
        {
            for (int j = 0; j < attackRangeCast.Count; j++)
            {
                attackRangeEffect = ability.effectArea.GetRotatedTiles(reachableTiles[i].GetCoordPosition(), reachableTiles[i].GetCoordPosition() + attackRangeCast[j]);
                for (int k = 0; k < attackRangeEffect.Count; k++)
                {
                    if ((reachableTiles[i].GetCoordPosition() + attackRangeCast[j] + attackRangeEffect[k]).Equals(target))
                    {
                        ReachableTile reachableTile = new ReachableTile(reachableTiles[i].path, reachableTiles[i].cost);
                        reachableTile.castTile = MapManager.GetTile(reachableTiles[i].GetCoordPosition() + attackRangeCast[j]);

                        canCastAndHitTarget.Add(reachableTile);
                        break;
                    }
                }
            }
        }

        canCastAndHitTarget.Sort();
        return canCastAndHitTarget;
    }

    /*
     * Regarde dans la "attackRangeCast" de chaque Tile de "tilesOnPath", quels sont les Tiles desquelles on peut lancer l'attaque "attackArea" et toucher "target".
     * 
     * return la Tiles de cast
     */
    public static TileData ValidCastFromTile(Ability ability, List<TileData> tilesOnPath, Vector2Int target)
    {
        List<Vector2Int> attackRangeCast = ability.castArea.GetTiles();
        List<Vector2Int> attackRangeEffect;

        for (int i = 0; i < tilesOnPath.Count; i++)
        {
                for (int j = 0; j < attackRangeCast.Count; j++)
            {
                attackRangeEffect = ability.effectArea.GetRotatedTiles(tilesOnPath[i].GetCoordPosition(), tilesOnPath[i].GetCoordPosition() + attackRangeCast[j]);
                for (int k = 0; k < attackRangeEffect.Count; k++)
                {
                    if ((tilesOnPath[i].GetCoordPosition() + attackRangeCast[j] + attackRangeEffect[k]).Equals(target))
                    {
                        return MapManager.GetTile(tilesOnPath[i].GetCoordPosition() + attackRangeCast[j]);
                    }
                }
            }
        }

        return null;
    }



    //#################################################################################################################################################################################################
    //################################################################################ FONCTION UTILS AU BRAIN IA #####################################################################################
    //#################################################################################################################################################################################################
    /*
     * Return les Tiles présentes autour de "currentTile"
     */
    public static List<TileData> TilesAround(TileData currentTile)
    {
        Vector2Int position = currentTile.position;
        List<TileData> around = new List<TileData>();

        if (position.x - 1 >= 0)
            around.Add(MapManager.GetTile(new Vector2Int(position.x - 1, position.y)));

        if (position.y + 1 < MapManager.GetSize())
            around.Add(MapManager.GetTile(new Vector2Int(position.x, position.y + 1)));

        if (position.x + 1 < MapManager.GetSize())
            around.Add(MapManager.GetTile(new Vector2Int(position.x + 1, position.y)));

        if (position.y - 1 >= 0)
            around.Add(MapManager.GetTile(new Vector2Int(position.x, position.y - 1)));

        return around;
    }

    /*
     * Regarde si les entity du players sont dans l'area que peut atteindre current avec mouvement + attack
     */
    public static void GetPlayerInRange(List<ReachableTile> reachableTiles, Ability ability,
                                        ref List<ReachableTile> playerHealerPathToAttack, ref List<ReachableTile> playerDPSPathToAttack, ref List<ReachableTile> playerTankPathToAttack,
                                        EntityBehaviour playerHealer, EntityBehaviour playerDPS, EntityBehaviour playerTank)
    {
        List<ReachableTile> resultForCast = null;

        if (playerHealer != null) resultForCast = ValidCastFromTile(ability, reachableTiles, playerHealer.GetPosition());
        if (resultForCast != null && resultForCast.Count > 0)
        {
            playerHealerPathToAttack = new List<ReachableTile>(resultForCast);
        }

        if (playerDPS != null) resultForCast = ValidCastFromTile(ability, reachableTiles, playerDPS.GetPosition());
        if (resultForCast != null && resultForCast.Count > 0)
        {
            playerDPSPathToAttack = new List<ReachableTile>(resultForCast);
        }

        if (playerTank != null) resultForCast = ValidCastFromTile(ability, reachableTiles, playerTank.GetPosition());
        if (resultForCast != null && resultForCast.Count > 0)
        {
            playerTankPathToAttack = new List<ReachableTile>(resultForCast);
        }

    }

    /*
     * Regarde s'il y a une Entity X ayant les parametre : "alignement" et "entityTag" autour de "target"
     */
    public static bool HaveXEntityAround(EntityBehaviour current, Alignement alignement, ReachableTile target, EntityTag? entityTag = null)
    {
        return HaveXEntityAround(current, alignement, target.GetLastTile(), entityTag);
    }
    public static bool HaveXEntityAround(EntityBehaviour current, Alignement alignement, TileData target, EntityTag? entityTag = null)
    {
        List<TileData> around = TilesAround(target);

        for (int i = 0; i < around.Count; i++)
        {
            for (int j = 0; j < around[i].entities.Count; j++)
            {
                if (around[i].entities[j] != current)
                {
                    if (around[i].entities[j].GetAlignement().Equals(alignement))
                    {
                        if (entityTag == null || around[i].entities[j].GetEntityTag().Equals(entityTag))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    /*
     * Return la listes des path possible vers les enemies
     */
    public static List<ReachableTile> ShortestsPathToEnemy(bool stopJustBeforeTarget, EntityBehaviour current, EntityBehaviour firstEntity, EntityBehaviour secondEntity, EntityBehaviour thirdEntity,
                                                        bool canWalkOnDamageTile = true, int range = - 1, bool havePriority = false, bool ignoreWeightMove = false)
    {
        List<ReachableTile> listOfPathPlayers = new List<ReachableTile>();
        List<EntityBehaviour> listPlayer = new List<EntityBehaviour>() { firstEntity, secondEntity, thirdEntity };

        for (int i = 0; i < listPlayer.Count; i++)
        {
            ReachableTile pathToPlayer = null;
            if (listPlayer[i] != null)
            {
                pathToPlayer = FindShortestPath(stopJustBeforeTarget, current.GetPosition(), listPlayer[i].GetPosition(), canWalkOnDamageTile, -1, ignoreWeightMove);

                if (pathToPlayer != null)
                    listOfPathPlayers.Add(pathToPlayer);
            }
        }

        if (!havePriority) listOfPathPlayers.Sort();

        for (int i = 0; i < listOfPathPlayers.Count; i++)
        {
            listOfPathPlayers[i] = CutPathInRange(listOfPathPlayers[i], range, ignoreWeightMove);
        }

        if (listOfPathPlayers.Count > 0) return listOfPathPlayers;
        else return null;
    }
        
    /*
     * Regarde si le turn d'entity est fini
     */
    public static bool CheckEndTurn(EntityBehaviour entity, bool canMakeAction, bool pass = false)
    {
        if (pass || !canMakeAction)
        {
            RoundManager.Instance.EndTurn();
            return true;
        }

        return false;
    }

    /*
     * Regarde si l'on a encore assez de PA pour move.
     */
    public static bool CanWalkAround(EntityBehaviour current, int remainingActionPoints, bool ignoreWeightMove = false)
    {
        List<TileData> around = TilesAround(current.currentTile);
        for (int i = 0; i < around.Count; i++)
        {
            if (around[i].IsWalkable && ((ignoreWeightMove) ? (fixedWeight <= remainingActionPoints) : ((int)around[i].tileType <= remainingActionPoints)))
            {
                return true;
            }
        }

        return false;
    }

    /*
     * Recupere l'ensemble des ReachableTile menant à une entity de "listEntity" en path direct ou avec "ability" dans l'area "reachableTiles"
     */
    public static List<Tuple<ReachableTile, EntityBehaviour>> PathToCastOrToJoin(bool stopJustBeforeTarget, GetReachableTileFromCastOrPath pathTo, EntityBehaviour current,
                                                                                    List<EntityBehaviour> listEntity, List<ReachableTile> reachableTiles, Ability ability, bool ignoreWeightMove = false)
    {
        List<Tuple<ReachableTile, EntityBehaviour>> allEntitiesReachableBestTileForCast = new List<Tuple<ReachableTile, EntityBehaviour>>();

        for (int i = 0; i < listEntity.Count; i++)
        {
            ReachableTile canCastAbility = pathTo(stopJustBeforeTarget, ability, reachableTiles, current.GetPosition(), listEntity[i].GetPosition(), current.CurrentActionPoints, ignoreWeightMove);
            if (canCastAbility != null && canCastAbility.path.Count > 0) allEntitiesReachableBestTileForCast.Add(new Tuple<ReachableTile, EntityBehaviour>(canCastAbility, listEntity[i]));
        }

        if (allEntitiesReachableBestTileForCast.Count > 0)
        {
            allEntitiesReachableBestTileForCast.Sort((x, y) => x.Item1.CompareTo(y.Item1));
            return allEntitiesReachableBestTileForCast;
        }

        return null;
    }
    


    //#################################################################################################################################################################################################
    //######################################################################### FONCTION QUI APPELLE DES FONCTIONS / LAMBDA ###########################################################################
    //#################################################################################################################################################################################################
    public delegate void IAEntity();
    public delegate bool SpecificConditionReachable(ReachableTile target);
    public delegate bool SpecificConditionEntity(EntityBehaviour entity);

    /*
     * Recupere la ReachableTile par "ValidCastFromTile" si l'on utilise l'Ability "ability", ou "FindShortestPath" si l'on cherche par le plus court chemin
     */
    public delegate ReachableTile GetReachableTileFromCastOrPath(bool stopJustBeforeTarget, Ability ability, List<ReachableTile> reachableTiles,
                                                                        Vector2Int startPosition, Vector2Int target, int range, bool ignoreWeightMove);
    public static ReachableTile GetReachableTileFromCastOrPathDelegate(bool stopJustBeforeTarget, Ability ability, List<ReachableTile> reachableTiles,
                                                                            Vector2Int startPosition, Vector2Int target, int range, bool ignoreWeightMove)
    {
        if (ability != null)
        {
            List<ReachableTile> tilesToCastAbility = ValidCastFromTile(ability, reachableTiles, target);

            if (tilesToCastAbility.Count > 0) return tilesToCastAbility[0];
            return null;
        }

        else return FindShortestPath(stopJustBeforeTarget, startPosition, target, true, range, ignoreWeightMove);
    }

    /*
     * Utilise l'abilite "ability" de "curent" sur "target".
     * 
     * Une fois fait appel "iaEntityFunction"
     */
    public delegate void LambdaAbilityCall(EntityBehaviour current, Ability ability, TileData target, IAEntity iaEntityFunction);
    public static void LambdaAbilityCallDelegate(EntityBehaviour current, Ability ability, TileData target, IAEntity iaEntityFunction)
    {
        current.UseAbility(ability, target).OnComplete(() => { iaEntityFunction(); });
    }

    /*
     * Deplace current sur "moveTarget" apres avoir verifier condition
     * 
     * Si l'on use une Ability, celle-ci est appele apres le deplacement ; sinon, il continue son tour
     */
    public static bool MoveAndTriggerAbilityIfNeed(EntityBehaviour current, ReachableTile moveTarget, IAEntity iaEntityFunction, 
                                                    bool condition = true, LambdaAbilityCall functionToCallAfterTheMove = null, Ability ability = null, TileData abilityTarget = null)
    {

        if (moveTarget == null) return false;

        if (condition)
        {
            if (moveTarget.path != null && moveTarget.path.Count > 0 && moveTarget.path[0].GetCoordPosition().Equals(current.GetPosition())) moveTarget.path.RemoveAt(0);

            if (functionToCallAfterTheMove != null)
            {
                if (current.CurrentActionPoints < ability.cost) return false;
                if (abilityTarget.GetPlayer() != null && abilityTarget.GetPlayer().stasis) return false;

                current.MoveTo(moveTarget).OnComplete(() => { functionToCallAfterTheMove(current, ability, abilityTarget, iaEntityFunction); });
            }

            else
            {
                current.MoveTo(moveTarget).OnComplete(() => { iaEntityFunction(); });
            }

            return true;
        }

        return false;
    }

    /*
     * Attack par l'odre de priorite first > second > third
     * 
     * Return si l'on a attaque
     */
    public static bool AttackWithPriority(EntityBehaviour current, List<ReachableTile> firstToTestMovement, List<ReachableTile> secondToTestMovement, List<ReachableTile> thirdToTestMovement,
                                            IAEntity iaEntityFunction,  LambdaAbilityCall functionToCallAfterTheMove, Ability ability,
                                            SpecificConditionReachable functionConditionReachable = null)
    {
        List<List<ReachableTile>> allReachableTilesForAllPlayers = new List<List<ReachableTile>>() { firstToTestMovement, secondToTestMovement, thirdToTestMovement };

        for (int i = 0; i < allReachableTilesForAllPlayers.Count; i++)
        {
            if (allReachableTilesForAllPlayers[i] != null)
            {
                for (int j = 0; j < allReachableTilesForAllPlayers[i].Count; j++)
                {
                    if (allReachableTilesForAllPlayers[i][j] != null && MoveAndTriggerAbilityIfNeed(current, allReachableTilesForAllPlayers[i][j], iaEntityFunction,
                                                                                    ((functionConditionReachable == null) ? (true) : (functionConditionReachable(allReachableTilesForAllPlayers[i][j]))),
                                                                                    functionToCallAfterTheMove, ability, allReachableTilesForAllPlayers[i][j].castTile))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /*
     * Regarde si l'un des players prepare une explosion et agit en consequence
     * 
     * return si l'action "MoveAndTriggerAbilityIfNeed" c'est bien derouler, ou false s'il n'y a pas d'explosion
     */
    public static bool IsThereAnExplosion(EntityBehaviour current, EntityBehaviour firstToTestTargetTile, EntityBehaviour secondToTestTargetTile, EntityBehaviour thirdToTestTargetTile,
                                            List<ReachableTile> firstToTestMovement, List<ReachableTile> secondToTestMovement, List<ReachableTile> thirdToTestMovement,
                                            IAEntity iaEntityFunction, LambdaAbilityCall functionToCallAfterTheMove, Ability ability, 
                                            SpecificConditionReachable functionConditionReachable = null, SpecificConditionEntity functionConditionEntity = null)
    {
        List<EntityBehaviour> allPlayers = new List<EntityBehaviour>() { firstToTestTargetTile, secondToTestTargetTile, thirdToTestTargetTile };
        List<List<ReachableTile>> allReachableTilesForAllPlayers = new List<List<ReachableTile>>() { firstToTestMovement, secondToTestMovement, thirdToTestMovement };
        List<(EntityBehaviour, ReachableTile)> shortestExplosion = new List<(EntityBehaviour, ReachableTile)>();

        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i] != null && allReachableTilesForAllPlayers[i] != null)
            {
                for (int j = 0; j < allReachableTilesForAllPlayers[i].Count; j++)
                {
                    if (allReachableTilesForAllPlayers[i][j] != null)
                    {
                        shortestExplosion.Add((allPlayers[i], allReachableTilesForAllPlayers[i][j]));
                    }
                }
            }
        }

        shortestExplosion.Sort((x, y) => x.Item2.CompareTo(y.Item2));

        for (int i = 0; i < shortestExplosion.Count; i++)
        {
            if (shortestExplosion[i].Item1.IsChannelingBurst)
            {
                if(MoveAndTriggerAbilityIfNeed(current, shortestExplosion[i].Item2, iaEntityFunction,
                                                (((functionConditionReachable == null) ? (true) : (functionConditionReachable(shortestExplosion[i].Item2))) &&
                                                ((functionConditionEntity == null) ? (true) : (functionConditionEntity(shortestExplosion[i].Item1)))),
                                                functionToCallAfterTheMove, ability, shortestExplosion[i].Item2.castTile))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /*
     * Se rapproche de l'ally le plus proche
     */
    public static bool WalkOnShortest(EntityBehaviour current, EntityBehaviour playerHealer, EntityBehaviour playerDPS, EntityBehaviour playerTank,
                                            IAEntity iaEntityFunction, SpecificConditionReachable conditionReachable = null)
    {
        List<ReachableTile> pathToShortestEnemy = ShortestsPathToEnemy(true, current, playerHealer, playerDPS, playerTank, true, current.CurrentActionPoints, false, true);

        if (pathToShortestEnemy != null)
        {
            for (int i = 0; i < pathToShortestEnemy.Count; i++)
            {
                if (conditionReachable != null)
                {
                    if (MoveAndTriggerAbilityIfNeed(current, pathToShortestEnemy[i], iaEntityFunction, conditionReachable(pathToShortestEnemy[i])))
                    {
                        return true;
                    }
                }

                else
                {
                    if (MoveAndTriggerAbilityIfNeed(current, pathToShortestEnemy[i], iaEntityFunction))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /*
     * Trouve un chemin avec priorite pour ce deplacer, meme si la destination final n'est pas atteignable
     */
    public static bool LastChancePath(EntityBehaviour current, EntityBehaviour firstEntity, EntityBehaviour secondEntity, EntityBehaviour thirdEntity, IAEntity iaEntityFunction, 
                                        SpecificConditionReachable functionConditionReachable = null)
    {
        List<EntityBehaviour> entities = new List<EntityBehaviour>();
        if (firstEntity != null) entities.Add(firstEntity);
        if (secondEntity != null) entities.Add(secondEntity);
        if (thirdEntity != null) entities.Add(thirdEntity);

        ReachableTile maxReachableTile = null;
        ReachableTile shortReachableTile = null;

        for (int i = 0; i < entities.Count; i++)
        {
            maxReachableTile = FindShortestPath(true, current.GetPosition(), entities[i].GetPosition(), true, -1, true, true);
            if (maxReachableTile != null && maxReachableTile.path != null)
            {
                for (int j = maxReachableTile.path.Count - 1; j >= 0; j--)
                {
                    if (maxReachableTile.path[j].IsWalkable)
                    {
                        shortReachableTile = FindShortestPath(true, current.GetPosition(), maxReachableTile.path[j].GetCoordPosition(), true, current.CurrentActionPoints, true);
                        if (shortReachableTile != null && shortReachableTile.path.Count > 0)
                        {
                            bool minionCondition = false;
                            if (functionConditionReachable != null)
                            {
                                minionCondition = true;
                                if (functionConditionReachable(shortReachableTile))
                                {
                                    minionCondition = false;
                                }
                            }

                            if (!minionCondition)
                            {
                                if (MoveAndTriggerAbilityIfNeed(current, shortReachableTile, iaEntityFunction))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }



    //#################################################################################################################################################################################################
    //############################################################################### PROBLEME REF : DEFAULT-VALUE ####################################################################################
    //#################################################################################################################################################################################################
    /*
     * Ensemble des fonctions "GetAllPlayers" que l'on peut appeller => réponse au faite qu'un param "ref" ne puisse avoir de "default-value"
     */
    public static void GetAllEntity(EntityBehaviour current, ref EntityBehaviour playerHealer, ref EntityBehaviour playerDPS, ref EntityBehaviour playerTank)
    {
        List<EntityBehaviour> temp = null;
        GetAllEntity(current, ref playerHealer, ref playerDPS, ref playerTank, ref temp, ref temp, ref temp, ref temp, false, false, false, false);
    }
    public static void GetAllEntity(EntityBehaviour current, ref EntityBehaviour playerHealer, ref EntityBehaviour playerDPS, ref EntityBehaviour playerTank,
                                        ref List<EntityBehaviour> enemyTank)
    {
        List<EntityBehaviour> temp = null;
        GetAllEntity(current, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank, ref temp, ref temp, ref temp, true, false, false, false);
    }
    public static void GetAllEntity(EntityBehaviour current, ref EntityBehaviour playerHealer, ref EntityBehaviour playerDPS, ref EntityBehaviour playerTank,
                                        ref List<EntityBehaviour> enemyTank, ref List<EntityBehaviour> enemyHealer)
    {
        List<EntityBehaviour> temp = null;
        GetAllEntity(current, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank, ref temp, ref enemyHealer, ref temp, true, false, true, false);
    }
    public static void GetAllEntity(EntityBehaviour current, ref EntityBehaviour playerHealer, ref EntityBehaviour playerDPS, ref EntityBehaviour playerTank,
                                        ref List<EntityBehaviour> enemyTank, ref List<EntityBehaviour> enemyDPS, ref List<EntityBehaviour> enemyHealer, ref List<EntityBehaviour> enemyMinion)
    {
        GetAllEntity(current, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank, ref enemyDPS, ref enemyHealer, ref enemyMinion, true, true, true, true);
    }

    /*
     * Ensemble des fonctions "MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup" que l'on peut appeller => réponse au faite qu'un param "ref" ne puisse avoir de "default-value"
     */
    public static bool MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(EntityBehaviour current, List<EntityBehaviour> listEntity, List<ReachableTile> reachableTiles, IAEntity iaEntityFunction,
                                                                            SpecificConditionEntity functionConditionEntity = null, SpecificConditionReachable functionConditionReachable = null,
                                                                            Ability ability = null, bool ignoreWeightMove = false)
    {
        List<ReachableTile> pathToUseAbility = null;

        GetReachableTileFromCastOrPath pathTo = GetReachableTileFromCastOrPathDelegate;

        return MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(pathTo, current, listEntity, reachableTiles, iaEntityFunction, false, ability,
                                                                    ref pathToUseAbility, null, functionConditionEntity, functionConditionReachable, true, ignoreWeightMove);
    }
    public static bool MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(EntityBehaviour current, List<EntityBehaviour> listEntity, List<ReachableTile> reachableTiles, IAEntity iaEntityFunction,
                                                                            Ability ability, ref List<ReachableTile> pathToUseAbility, LambdaAbilityCall functionToCallAfterTheMove,
                                                                            SpecificConditionEntity functionConditionEntity = null, SpecificConditionReachable functionConditionReachable = null,
                                                                            bool ignoreWeightMove = false)
    {
        GetReachableTileFromCastOrPath pathTo = GetReachableTileFromCastOrPathDelegate;

        return MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(pathTo, current, listEntity, reachableTiles, iaEntityFunction, true, ability,
                                                                ref pathToUseAbility, functionToCallAfterTheMove, functionConditionEntity, functionConditionReachable, ignoreWeightMove);
    }



    //#################################################################################################################################################################################################
    //#################################################################################################################################################################################################
    //############################################################################################ PRIVATE ############################################################################################
    //#################################################################################################################################################################################################
    //#################################################################################################################################################################################################

    /*
     * Regarde si les 4 Place entourant "position" existe
     * Si oui, on regarde si la Place est atteignable
     */
    private static bool LookAround(bool stopJustBeforeTarget, NavigationQueryType navigationType, ref List<ReachableTile> reachableTiles, ReachableTile precedentPlace,
                            Vector2Int target, bool canWalkOnDamageTile = true, int range = -1, bool ignoreWalkable = false, bool ignoreWeightMove = false)
    {
        Vector2Int position = precedentPlace.GetCoordPosition();

        TileData lookingTileData;
        Vector2Int lookingPosition;

        if (position.x - 1 >= 0)
        {
            lookingPosition = new Vector2Int(position.x - 1, position.y);
            if (LookFor(ref reachableTiles))
            {
                return true;
            }
        }

        if (position.y + 1 < MapManager.GetSize())
        {
            lookingPosition = new Vector2Int(position.x, position.y + 1);
            if (LookFor(ref reachableTiles))
            {
                return true;
            }
        }

        if (position.x + 1 < MapManager.GetSize())
        {
            lookingPosition = new Vector2Int(position.x + 1, position.y);
            if (LookFor(ref reachableTiles))
            {
                return true;
            }
        }

        if (position.y - 1 >= 0)
        {
            lookingPosition = new Vector2Int(position.x, position.y - 1);
            if (LookFor(ref reachableTiles))
            {
                return true;
            }
        }

        bool LookFor(ref List<ReachableTile> reachableTile)
        {
            if (stopJustBeforeTarget && lookingPosition.Equals(target))
            {
                return true;
            }

            lookingTileData = MapManager.GetTile(lookingPosition);

            if (navigationType.Equals(NavigationQueryType.Area))
            {
                IsMovementPossible(stopJustBeforeTarget, navigationType, ref reachableTile, precedentPlace, lookingTileData, lookingPosition, target, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove);
            }

            else if (IsMovementPossible(stopJustBeforeTarget, navigationType, ref reachableTile, precedentPlace, lookingTileData, lookingPosition, target, canWalkOnDamageTile, -1, ignoreWalkable, ignoreWeightMove))
            {
                return true;
            }

            return false;
        }

        return false;
    }

    /*
     * Verifie que la TileData est Walkable OU si on ne le prend pas en consideration
     * 
     **** Verifie si l'on doit prendre en consideration le poids de deplacement de la TileData
     **** Verifie qu'il nous reste assez de points de deplacement
     * 
     * Si toutes les verifications sont passer, alors on regarde pour ajouter la Place
     */
    private static bool IsMovementPossible(bool stopJustBeforeTarget, NavigationQueryType navigationType, ref List<ReachableTile> reachableTiles, ReachableTile precedentPlace, TileData lookingTileData, 
                                            Vector2Int lookingPosition, Vector2Int target, bool canWalkOnDamageTile, int range = -1, bool ignoreWalkable = false, bool ignoreWeightMove = false)
    {
        if (!canWalkOnDamageTile && lookingTileData.entities.Find((x) => x.GetEntityTag().Equals(EntityTag.Trap)) != null) return false;

        if (ignoreWalkable || lookingTileData.IsWalkable) // Si l'on peut marcher sur la prochaine TileData OU si cela ne nous interesse pas
        {
            ReachableTile currentPlaceAlreadyFind = reachableTiles.Where(elem => elem.GetCoordPosition().Equals(lookingPosition)).FirstOrDefault();

            int cout;
            if (ignoreWeightMove) cout = precedentPlace.cost + fixedWeight;
            else cout = precedentPlace.cost + (int)lookingTileData.tileType;

            if (navigationType.Equals(NavigationQueryType.Area))
            {
                if (cout <= range) // S'il nous reste assez de points de deplacement pour aller sur lookingTileData
                {
                    ReachableTile currentPlace = new ReachableTile(new List<TileData>(precedentPlace.path) { lookingTileData }, cout);
                    AddTilesInList(stopJustBeforeTarget, navigationType, ref reachableTiles, currentPlaceAlreadyFind, currentPlace, target, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove);
                }
            }

            else
            {
                ReachableTile currentPlace = new ReachableTile(new List<TileData>(precedentPlace.path) { lookingTileData }, cout);
                return AddTilesInList(stopJustBeforeTarget, navigationType, ref reachableTiles, currentPlaceAlreadyFind, currentPlace, target, canWalkOnDamageTile);
            }
        }

        return false;
    }

    /*
     * Ajoute la nouvelle Tiles atteignable a la List ou modifie la preexistante
     */
    private static bool AddTilesInList(bool stopJustBeforeTarget, NavigationQueryType navigationType, ref List<ReachableTile> reachableTiles, ReachableTile currentPlaceAlreadyFind, ReachableTile currentPlace,
                                Vector2Int target, bool canWalkOnDamageTile, int range = -1, bool ignoreWalkable = false, bool ignoreWeightMove = false)
    {
        if (currentPlaceAlreadyFind != null) // Si le TileData est deja dans la liste des points atteignable
        {
            if (currentPlace.IsBetterThat(currentPlaceAlreadyFind)) // Si le chemin actuel est meilleur que le precedent
            {
                reachableTiles.Remove(currentPlaceAlreadyFind);
                reachableTiles.Add(currentPlace);

                if (navigationType.Equals(NavigationQueryType.Area)) LookAround(stopJustBeforeTarget, navigationType, ref reachableTiles, currentPlace, target, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove);
            }
        }

        else // Si le TileData na pas encore ete atteint
        {
            reachableTiles.Add(currentPlace);
            if (navigationType.Equals(NavigationQueryType.Area)) LookAround(stopJustBeforeTarget, navigationType, ref reachableTiles, currentPlace, target, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove);
            else if (currentPlace.GetCoordPosition().Equals(target)) return true;
        }

        return false;
    }

    /*
     * return "null" s'il n'existe pas de path, le path complet si "fullPath" ou le path dans la limite de "range" sinon
     */
    private static ReachableTile ReturnPath(bool stopJustBeforeTarget, List<ReachableTile> reachableTiles, int range, bool ignoreWeightMove)
    {
        if (reachableTiles == null || reachableTiles.Count <= 0)
        {
            return null;
        }

        ReachableTile shortest;
        if (stopJustBeforeTarget) shortest = reachableTiles[0];
        else shortest = reachableTiles[reachableTiles.Count - 1];

        shortest.path.RemoveAt(0);

        if (range >= 0)
        {
            return CutPathInRange(shortest, range, ignoreWeightMove);
        }
        else
        {
            return shortest;
        }
    }

    /*
     * Cut le path de "shortest" dans la limite "range" donner
     */
    private static ReachableTile CutPathInRange(ReachableTile shortest, int range, bool ignoreWeightMove)
    {
        if (shortest == null) return null;

        List<TileData> path = new List<TileData>(shortest.path);
        int lenght = 0;
        int cost = 0;

        for (int i = 0; i < path.Count; i++)
        {
            cost += ignoreWeightMove ? fixedWeight : (int)path[i].tileType;
            if (cost > range)
            {
                cost -= ignoreWeightMove ? fixedWeight : (int)path[i].tileType;
                break;
            }
            lenght++;
        }

        shortest.path = path.Take(lenght).ToList();
        shortest.cost = cost;
        return shortest;
    }
    
    /*
     * Récupère l'ensemble des entity ou qu'ils soit sur la map (hormis celui qui appelle)
     */
    private static void GetAllEntity(EntityBehaviour current, ref EntityBehaviour playerHealer, ref EntityBehaviour playerDPS, ref EntityBehaviour playerTank,
                                        ref List<EntityBehaviour> enemyTank, ref List<EntityBehaviour> enemyDPS, ref List<EntityBehaviour> enemyHealer, ref List<EntityBehaviour> enemyMinion,
                                        bool haveEnemyTank, bool haveEnemyDPS, bool haveEnemyHealer, bool haveEnemyMinion)
    {
        for (int i = 0; i < MapManager.GetListOfEntity().Count; i++)
        {
            if (current != MapManager.GetListOfEntity()[i])
            {
                if (MapManager.GetListOfEntity()[i].GetAlignement().Equals(Alignement.Player))
                {
                    if (MapManager.GetListOfEntity()[i].GetEntityTag().Equals(EntityTag.Healer))
                    {
                        playerHealer = MapManager.GetListOfEntity()[i];
                    }

                    else if (MapManager.GetListOfEntity()[i].GetEntityTag().Equals(EntityTag.DPS))
                    {
                        playerDPS = MapManager.GetListOfEntity()[i];
                    }

                    else if (MapManager.GetListOfEntity()[i].GetEntityTag().Equals(EntityTag.Tank))
                    {
                        playerTank = MapManager.GetListOfEntity()[i];
                    }
                }

                else if (MapManager.GetListOfEntity()[i].GetAlignement().Equals(Alignement.Enemy))
                {
                    if (MapManager.GetListOfEntity()[i] != current)
                    {
                        if (MapManager.GetListOfEntity()[i].GetEntityTag().Equals(EntityTag.Tank))
                        {
                            if (haveEnemyTank) enemyTank.Add(MapManager.GetListOfEntity()[i]);
                        }

                        else if (MapManager.GetListOfEntity()[i].GetEntityTag().Equals(EntityTag.DPS))
                        {
                            if (haveEnemyDPS) enemyDPS.Add(MapManager.GetListOfEntity()[i]);
                        }

                        else if (MapManager.GetListOfEntity()[i].GetEntityTag().Equals(EntityTag.Healer))
                        {
                            if (haveEnemyHealer) enemyHealer.Add(MapManager.GetListOfEntity()[i]);
                        }

                        else if (MapManager.GetListOfEntity()[i].GetEntityTag().Equals(EntityTag.Minion))
                        {
                            if (haveEnemyMinion) enemyMinion.Add(MapManager.GetListOfEntity()[i]);
                        }
                    }
                }
            }
        }
    }

    /*
    * Cherche quel entity de "listEntity" est a la fois la plus proche de "current" et est touchable par "ability" depuis "reachableTiles" en respectant "functionCondition"
    * 
    * Une fois trouve, move et trigger l'ability si besoin sur l'entity. return true
    * Si aucune entity ne correspond a "functionConditionForAbility" ou aucune n'est dans "reachableTiles" alors renvoi ceux qui ne repondait pas a "functionCondition". return false
    * 
    * -------------------------------------------
    * 
    * Cherche quel l'entity de "listEntity" est la plus proche de "current"
    * 
    * Une fois trouve, move sur l'entity. return true
    * Sinon, return false
    */
    private static bool MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(GetReachableTileFromCastOrPath pathTo, EntityBehaviour current, List<EntityBehaviour> listEntity, List<ReachableTile> reachableTiles,
                                                                            IAEntity iaEntityFunction, bool useAbility, Ability ability, ref List<ReachableTile> pathToUseAbility,
                                                                            LambdaAbilityCall functionToCallAfterTheMove, SpecificConditionEntity functionConditionEntity,
                                                                            SpecificConditionReachable functionConditionReachable, bool stopJustBeforeTarget = false, bool ignoreWeightMove = false)
    {
        List<Tuple<ReachableTile, EntityBehaviour>> allEntitiesReachableBestTileForCast = PathToCastOrToJoin(stopJustBeforeTarget, pathTo, current, listEntity, reachableTiles, ability, ignoreWeightMove);

        if (allEntitiesReachableBestTileForCast == null) return false;

        for (int i = 0; i < allEntitiesReachableBestTileForCast.Count; i++)
        {
            if (useAbility && MoveAndTriggerAbilityIfNeed(current, allEntitiesReachableBestTileForCast[i].Item1, iaEntityFunction,
                                                            (((functionConditionReachable == null) ? (true) : (functionConditionReachable(allEntitiesReachableBestTileForCast[i].Item1))) &&
                                                            ((functionConditionEntity == null) ? (true) : (functionConditionEntity(allEntitiesReachableBestTileForCast[i].Item2)))),
                                                            functionToCallAfterTheMove, ability, allEntitiesReachableBestTileForCast[i].Item1.castTile))
            {
                return true;
            }

            else if (!useAbility && MoveAndTriggerAbilityIfNeed(current, allEntitiesReachableBestTileForCast[i].Item1, iaEntityFunction,
                                                                    (((functionConditionReachable == null) ? (true) : (functionConditionReachable(allEntitiesReachableBestTileForCast[i].Item1))) &&
                                                                    ((functionConditionEntity == null) ? (true) : (functionConditionEntity(allEntitiesReachableBestTileForCast[i].Item2))))))
            {
                return true;
            }

            if (useAbility && pathToUseAbility == null)
            {
                if (allEntitiesReachableBestTileForCast[i].Item2.CurrentHealth < allEntitiesReachableBestTileForCast[i].Item2.data.maxHealth)
                {
                    pathToUseAbility.Add(allEntitiesReachableBestTileForCast[i].Item1);
                }
            }
        }

        return false;
    }
}
