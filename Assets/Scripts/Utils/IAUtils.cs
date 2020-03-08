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
    public static List<ReachableTile> FindAllReachablePlace(Vector2Int startPosition, int range, bool ignoreWeightMove = false, bool ignoreWalkable = false, bool canWalkOnDamageTile = true, bool stopJustBeforeTarget = false)
    {
        List<ReachableTile> reachableTiles = new List<ReachableTile>() { new ReachableTile(new List<TileData>() { MapManager.GetTile(startPosition) }, 0) };

        LookAround(stopJustBeforeTarget, NavigationQueryType.Area, ref reachableTiles, reachableTiles[0], Vector2Int.zero, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove);

        return reachableTiles;
    }

    /*
     * Trouve le plus court chemin depuis "startPosition" a "target", calculer par la fonction de l'IA
     */
    public static ReachableTile FindShortestPath(bool stopJustBeforeTarget, Vector2Int startPosition, Vector2Int target, bool canWalkOnDamageTile = true, int range = -1)
    {
        List<ReachableTile> reachableTiles = new List<ReachableTile>() { new ReachableTile(new List<TileData>() { MapManager.GetTile(startPosition) }, 0) };
        List<Vector2Int> deletedPlaces = new List<Vector2Int>();

        if (!startPosition.Equals(target))
        {
            while (reachableTiles.Count > 0 && !LookAround(stopJustBeforeTarget, NavigationQueryType.Path, ref reachableTiles, reachableTiles[0], target, canWalkOnDamageTile, range))
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

        return ReturnPath(reachableTiles, range);
    }

    /*
     * Regarde dans "reachableTiles" ("tilesOnPath") quels sont les Tiles desquelles on peut lancer l'attaque "attackArea" et toucher "target".
     * 
     * return l'ensemble de ces Tiles.
     */
    public static List<ReachableTile> ValidCastFromTile(TileArea attackArea, List<ReachableTile> reachableTiles, Vector2Int target)
    {
        List<ReachableTile> canCastAndHitTarget = new List<ReachableTile>();
        List<Vector2Int> attackRange = attackArea.GetTiles();

        for (int i = 0; i < reachableTiles.Count; i++)
        {
            for (int j = 0; j < attackRange.Count; j++)
            {
                if ((reachableTiles[i].GetCoordPosition() + attackRange[j]).Equals(target))
                {
                    canCastAndHitTarget.Add(reachableTiles[i]);
                    break;
                }
            }
        }

        canCastAndHitTarget.Sort();
        return canCastAndHitTarget;
    }
    public static List<TileData> ValidCastFromTile(TileArea attackArea, List<TileData> tilesOnPath, Vector2Int target)
    {
        List<TileData> canCastAndHitTarget = new List<TileData>();
        List<Vector2Int> attackRange = attackArea.GetTiles();

        for (int i = 0; i < tilesOnPath.Count; i++)
        {
            for (int j = 0; j < attackRange.Count; j++)
            {
                if ((tilesOnPath[i].GetCoordPosition() + attackRange[j]).Equals(target))
                {
                    canCastAndHitTarget.Add(tilesOnPath[i]);
                    break;
                }
            }
        }

        return canCastAndHitTarget;
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
    public static void GetPlayerInRange(List<ReachableTile> reachableTiles, TileArea attackArea,
                                        ref ReachableTile playerHealerPathToAttack, ref ReachableTile playerDPSPathToAttack, ref ReachableTile playerTankPathToAttack,
                                        EntityBehaviour playerHealer, EntityBehaviour playerDPS, EntityBehaviour playerTank)
    {
        List<ReachableTile> resultForCast;

        for (int i = 0; i < reachableTiles.Count; i++)
        {
            resultForCast = ValidCastFromTile(attackArea, reachableTiles, playerHealer.GetPosition());
            if (resultForCast != null)
            {
                playerHealerPathToAttack = resultForCast[0];
            }

            resultForCast = ValidCastFromTile(attackArea, reachableTiles, playerDPS.GetPosition());
            if (resultForCast != null)
            {
                playerDPSPathToAttack = resultForCast[0];
            }

            resultForCast = ValidCastFromTile(attackArea, reachableTiles, playerTank.GetPosition());
            if (resultForCast != null)
            {
                playerTankPathToAttack = resultForCast[0];
            }
        }
    }

    /*
     * Regarde s'il y a une Entity X ayant les parametre : "alignement" et "entityTag" autour de "target"
     */
    public static bool HaveXEntityAround(Alignement alignement, ReachableTile target, EntityTag? entityTag = null)
    {
        return HaveXEntityAround(alignement, target.GetLastTile(), entityTag);
    }
    public static bool HaveXEntityAround(Alignement alignement, TileData target, EntityTag? entityTag = null)
    {
        List<TileData> around = TilesAround(target);

        for (int i = 0; i < around.Count; i++)
        {
            for (int j = 0; j < around[i].entities.Count; j++)
            {
                if (entityTag == null)
                {
                    return true;
                }

                else if (around[i].entities[j].GetAlignement().Equals(alignement))
                {
                    if (around[i].entities[j].GetEntityTag().Equals(entityTag))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /*
     * Return le path vers l'enemy le plus proche
     */
    public static ReachableTile PathToShortestEnemy(bool stopJustBeforeTarget, EntityBehaviour current, EntityBehaviour firstEntity, EntityBehaviour secondEntity, EntityBehaviour thirdEntity,
                                                        bool canWalkOnDamageTile = true, int range = - 1, bool havePriority = false)
    {
        List<ReachableTile> listOfPathPlayers = new List<ReachableTile>();
        List<EntityBehaviour> listPlayer = new List<EntityBehaviour>() { firstEntity, secondEntity, thirdEntity };

        for (int i = 0; i < listPlayer.Count; i++)
        {
            ReachableTile pathToPlayer = null;
            if (listPlayer[i] != null)
            {
                pathToPlayer = FindShortestPath(stopJustBeforeTarget, current.GetPosition(), listPlayer[i].GetPosition(), canWalkOnDamageTile, range);

                if (pathToPlayer != null)
                    listOfPathPlayers.Add(pathToPlayer);
            }
        }

        if (!havePriority) listOfPathPlayers.Sort();

        if (listOfPathPlayers.Count > 0) return listOfPathPlayers[0];
        else return null;
    }

    /*
     * Recupere les ability de current
     */
    public static void GetAbility(EntityBehaviour current, string nameAbility1, string nameAbility2, ref Ability ability1, ref Ability ability2)
    {
        for (int i = 0; i < current.GetAbilities().Count; i++)
        {
            if (current.GetAbilities(0).name.Equals(nameAbility1))
            {
                ability1 = current.GetAbilities(0);
            }

            else if (current.GetAbilities(0).name.Equals(nameAbility2))
            {
                ability2 = current.GetAbilities(0);
            }
        }
    }
    
    /*
     * Regarde si le turn d'entity est fini
     */
    public static bool CheckEndTurn(EntityBehaviour entity, bool canMakeAction, bool pass = false)
    {
        if (pass || !canMakeAction)
        {
            RoundManager.Instance.EndTurn(entity);
            return true;
        }

        return false;
    }

    /*
     * Regarde si l'on a encore assez de PA pour move.
     */
    public static bool CanWalkAround(EntityBehaviour current, int remainingActionPoints)
    {
        List<TileData> around = IAUtils.TilesAround(current.currentTile);
        for (int i = 0; i < around.Count; i++)
        {
            if (around[i].IsWalkable && (int)around[i].tileType <= remainingActionPoints)
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
                                                                                    List<EntityBehaviour> listEntity, List<ReachableTile> reachableTiles, Ability ability)
    {
        List<Tuple<ReachableTile, EntityBehaviour>> allEntitiesReachableBestTileForCast = new List<Tuple<ReachableTile, EntityBehaviour>>();

        for (int i = 0; i < listEntity.Count; i++)
        {
            ReachableTile canCastAbility = pathTo(stopJustBeforeTarget, ability, reachableTiles, current.GetPosition(), listEntity[i].GetPosition(), current.CurrentActionPoints);
            if (canCastAbility != null && canCastAbility.path.Count > 0) allEntitiesReachableBestTileForCast.Add(new Tuple<ReachableTile, EntityBehaviour>(canCastAbility, listEntity[i]));
        }

        if (allEntitiesReachableBestTileForCast.Count > 0)
        {
            allEntitiesReachableBestTileForCast.Sort((x, y) => x.Item1.CompareTo(x.Item2));
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
    public delegate ReachableTile GetReachableTileFromCastOrPath(bool stopJustBeforeTarget, Ability ability, List<ReachableTile> reachableTiles, Vector2Int startPosition, Vector2Int target, int range);
    public static ReachableTile GetReachableTileFromCastOrPathDelegate(bool stopJustBeforeTarget, Ability ability, List<ReachableTile> reachableTiles, Vector2Int startPosition, Vector2Int target, int range)
    {
        if (ability != null) return ValidCastFromTile(ability.effectArea, reachableTiles, target)[0];
        else return FindShortestPath(stopJustBeforeTarget, startPosition, target, true, range);
    }

    /*
     * Utilise l'abilite "ability" de "curent" sur "target".
     * 
     * Une fois fait appel "iaEntityFunction"
     */
    public delegate void LambdaAbilityCall(EntityBehaviour current, Ability ability, TileData target, IAEntity iaEntityFunction);
    public static void LambdaAbilityCallDelegate(EntityBehaviour current, Ability ability, TileData target, IAEntity iaEntityFunction)
    {
        current.CurrentActionPoints -= ability.cost;

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
            current.CurrentActionPoints -= moveTarget.cost;

            if (functionToCallAfterTheMove != null)
            {
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
    public static bool AttackWithPriority(EntityBehaviour current, ReachableTile firstToTestMovement, ReachableTile secondToTestMovement, ReachableTile thirdToTestMovement,
                                            IAEntity iaEntityFunction,  LambdaAbilityCall functionToCallAfterTheMove, Ability ability, 
                                            EntityBehaviour firstToTestTargetTile, EntityBehaviour secondToTestTargetTile, EntityBehaviour thirdToTestTargetTile, 
                                            SpecificConditionReachable functionConditionReachable = null, SpecificConditionEntity functionConditionEntity = null)
    {
        if (!MoveAndTriggerAbilityIfNeed(current, firstToTestMovement, iaEntityFunction,
                                            functionConditionReachable == null ? true : functionConditionReachable(firstToTestMovement) &&
                                            functionConditionEntity == null ? true : functionConditionEntity(firstToTestTargetTile),
                                            functionToCallAfterTheMove, ability, firstToTestTargetTile.currentTile))
        {
            if (!MoveAndTriggerAbilityIfNeed(current, secondToTestMovement, iaEntityFunction, 
                                                functionConditionReachable == null ? true : functionConditionReachable(secondToTestMovement) &&
                                                functionConditionEntity == null ? true : functionConditionEntity(secondToTestTargetTile),
                                                functionToCallAfterTheMove, ability, secondToTestTargetTile.currentTile))
            {
                if (!MoveAndTriggerAbilityIfNeed(current, thirdToTestMovement, iaEntityFunction, 
                                                    functionConditionReachable == null ? true : functionConditionReachable(thirdToTestMovement) &&
                                                    functionConditionEntity == null ? true : functionConditionEntity(thirdToTestTargetTile),
                                                    functionToCallAfterTheMove, ability, thirdToTestTargetTile.currentTile))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /*
     * Regarde si l'un des players prepare une explosion et agit en consequence
     * 
     * return si l'action "MoveAndTriggerAbilityIfNeed" c'est bien derouler, ou false s'il n'y a pas d'explosion
     */
    public static bool IsThereAnExplosion(EntityBehaviour current, EntityBehaviour playerHealer, EntityBehaviour playerDPS, EntityBehaviour playerTank,
                                            ReachableTile playerHealerPathToAttack, ReachableTile playerDPSPathToAttack, ReachableTile playerTankPathToAttack,
                                            IAEntity iaEntityFunction, LambdaAbilityCall functionToCallAfterTheMove, Ability ability, 
                                            SpecificConditionReachable functionConditionReachable = null, SpecificConditionEntity functionConditionEntity = null)
    {
        List<(EntityBehaviour, ReachableTile)> shortestExplosion = new List<(EntityBehaviour, ReachableTile)>();
        if (playerHealerPathToAttack != null) shortestExplosion.Add((playerHealer, playerHealerPathToAttack));
        if (playerDPSPathToAttack != null) shortestExplosion.Add((playerDPS, playerDPSPathToAttack));
        if (playerTankPathToAttack != null) shortestExplosion.Add((playerTank, playerTankPathToAttack));

        shortestExplosion.Sort((x, y) => x.Item2.CompareTo(y.Item2));

        for (int i = 0; i < shortestExplosion.Count; i++)
        {
            if (shortestExplosion[i].Item1 != null && shortestExplosion[i].Item1.IsChannelingBurst && shortestExplosion[i].Item2 != null)
            {
                if(MoveAndTriggerAbilityIfNeed(current, shortestExplosion[i].Item2, iaEntityFunction,
                                                functionConditionReachable == null ? true : functionConditionReachable(playerHealerPathToAttack) &&
                                                functionConditionEntity == null ? true : functionConditionEntity(shortestExplosion[i].Item1),
                                                functionToCallAfterTheMove, ability, shortestExplosion[i].Item1.currentTile))
                {
                    return true;
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
                                                                            Ability ability = null)
    {
        ReachableTile pathToHeal = null;
        EntityBehaviour entityToHeal = null;

        GetReachableTileFromCastOrPath pathTo = GetReachableTileFromCastOrPathDelegate;

        return MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(pathTo, current, listEntity, reachableTiles, iaEntityFunction, false, ability,
                                                                    ref pathToHeal, ref entityToHeal, null, functionConditionEntity, functionConditionReachable, true);
    }
    public static bool MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(EntityBehaviour current, List<EntityBehaviour> listEntity, List<ReachableTile> reachableTiles, IAEntity iaEntityFunction,
                                                                            Ability ability, ref ReachableTile pathToHeal, ref EntityBehaviour entityToHeal, LambdaAbilityCall functionToCallAfterTheMove,
                                                                            SpecificConditionEntity functionConditionEntity = null, SpecificConditionReachable functionConditionReachable = null)
    {
        GetReachableTileFromCastOrPath pathTo = GetReachableTileFromCastOrPathDelegate;

        return MoveAndTriggerAbilityIfNeedOnTheShortestOfAGroup(pathTo, current, listEntity, reachableTiles, iaEntityFunction, true, ability,
                                                                ref pathToHeal, ref entityToHeal, functionToCallAfterTheMove, functionConditionEntity, functionConditionReachable);
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

            else if (IsMovementPossible(stopJustBeforeTarget, navigationType, ref reachableTile, precedentPlace, lookingTileData, lookingPosition, target, canWalkOnDamageTile))
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
    private static bool IsMovementPossible(bool stopJustBeforeTarget, NavigationQueryType navigationType, ref List<ReachableTile> reachableTiles, ReachableTile precedentPlace, TileData lookingTileData, Vector2Int lookingPosition,
                                     Vector2Int target, bool canWalkOnDamageTile, int range = -1, bool ignoreWalkable = false, bool ignoreWeightMove = false)
    {
        if (!canWalkOnDamageTile && lookingTileData.entities.Find((x) => x.GetEntityTag().Equals(EntityTag.Trap)) != null) return false;

        if (ignoreWalkable || lookingTileData.IsWalkable) // Si l'on peut marcher sur la prochaine TileData OU si cela ne nous interesse pas
        {
            ReachableTile currentPlaceAlreadyFind = reachableTiles.Where(elem => elem.GetCoordPosition().Equals(lookingPosition)).FirstOrDefault();

            if (navigationType.Equals(NavigationQueryType.Area))
            {
                int cout;
                if (ignoreWeightMove) cout = precedentPlace.cost + fixedWeight;
                else cout = precedentPlace.cost + (int)lookingTileData.tileType;

                if (cout <= range) // S'il nous reste assez de points de deplacement pour aller sur lookingTileData
                {
                    ReachableTile currentPlace = new ReachableTile(new List<TileData>(precedentPlace.path) { lookingTileData }, cout);
                    AddTilesInList(stopJustBeforeTarget, navigationType, ref reachableTiles, currentPlaceAlreadyFind, currentPlace, target, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove);
                }
            }

            else
            {
                ReachableTile currentPlace = new ReachableTile(new List<TileData>(precedentPlace.path) { lookingTileData }, precedentPlace.cost + (int)lookingTileData.tileType);
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
    private static ReachableTile ReturnPath(List<ReachableTile> reachableTiles, int range)
    {
        if (reachableTiles == null || reachableTiles.Count <= 0)
        {
            return null;
        }

        ReachableTile shortest = reachableTiles[reachableTiles.Count - 1];
        shortest.path.RemoveAt(0);

        if (range >= 0)
        {
            return CutPathInRange(shortest, range);
        }
        else
        {
            return shortest;
        }
    }

    /*
     * Cut le path de "shortest" dans la limite "range" donner
     */
    private static ReachableTile CutPathInRange(ReachableTile shortest, int range)
    {
        if (shortest == null) return null;

        List<TileData> path = new List<TileData>(shortest.path);
        int lenght = 0;
        int cost = 0;

        for (int i = 0; i < path.Count; i++)
        {
            cost += (int)path[i].tileType;
            if (cost > range)
                break;
            lenght++;
        }

        shortest.path = path.Take(lenght).ToList();
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
                                                                            IAEntity iaEntityFunction, bool useAbuility, Ability ability, ref ReachableTile pathToHeal, ref EntityBehaviour entityToHeal,
                                                                            LambdaAbilityCall functionToCallAfterTheMove, SpecificConditionEntity functionConditionEntity,
                                                                            SpecificConditionReachable functionConditionReachable, bool stopJustBeforeTarget = false)
    {
        List<Tuple<ReachableTile, EntityBehaviour>> allEntitiesReachableBestTileForCast = PathToCastOrToJoin(stopJustBeforeTarget, pathTo, current, listEntity, reachableTiles, ability);
        if (allEntitiesReachableBestTileForCast == null) return false;

        for (int i = 0; i < allEntitiesReachableBestTileForCast.Count; i++)
        {
            if (useAbuility && MoveAndTriggerAbilityIfNeed(current, allEntitiesReachableBestTileForCast[i].Item1, iaEntityFunction,
                                                            functionConditionReachable == null ? true : functionConditionReachable(allEntitiesReachableBestTileForCast[i].Item1) &&
                                                            functionConditionEntity == null ? true : functionConditionEntity(allEntitiesReachableBestTileForCast[i].Item2),
                                                            functionToCallAfterTheMove, ability, allEntitiesReachableBestTileForCast[i].Item2.currentTile))
            {
                return true;
            }

            else if (!useAbuility && MoveAndTriggerAbilityIfNeed(current, allEntitiesReachableBestTileForCast[i].Item1, iaEntityFunction,
                                                                    functionConditionReachable == null ? true : functionConditionReachable(allEntitiesReachableBestTileForCast[i].Item1) &&
                                                                    functionConditionEntity == null ? true : functionConditionEntity(allEntitiesReachableBestTileForCast[i].Item2)))
            {
                return true;
            }

            else if (useAbuility && pathToHeal == null)
            {
                pathToHeal = allEntitiesReachableBestTileForCast[i].Item1;
                entityToHeal = allEntitiesReachableBestTileForCast[i].Item2;
            }
        }

        return false;
    }
}
