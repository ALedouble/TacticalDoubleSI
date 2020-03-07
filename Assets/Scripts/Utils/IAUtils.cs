using DG.Tweening;
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
    public static List<ReachableTile> FindAllReachablePlace(Vector2Int startPosition, int range, bool ignoreWeightMove = false, bool ignoreWalkable = false, bool canWalkOnDamageTile = true)
    {
        List<ReachableTile> reachableTiles = new List<ReachableTile>() { new ReachableTile(new List<TileData>() { MapManager.GetTile(startPosition) }, 0) };

        LookAround(NavigationQueryType.Area, ref reachableTiles, reachableTiles[0], Vector2Int.zero, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove);

        return reachableTiles;
    }

    /*
     * Trouve le plus court chemin depuis "startPosition" a "target", calculer par la fonction de l'IA
     */
    public static ReachableTile FindShortestPath(Vector2Int startPosition, Vector2Int target, bool canWalkOnDamageTile = true, int range = -1, bool fullPath = false)
    {
        List<ReachableTile> reachableTiles = new List<ReachableTile>() { new ReachableTile(new List<TileData>() { MapManager.GetTile(startPosition) }, 0) };
        List<Vector2Int> deletedPlaces = new List<Vector2Int>();

        if (!startPosition.Equals(target))
        {
            while (reachableTiles.Count > 0 && !LookAround(NavigationQueryType.Path, ref reachableTiles, reachableTiles[0], target))
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

        return ReturnPath(reachableTiles, range, fullPath);
    }

    /*
     * Regarde dans "reachableTiles" quels sont les Tiles desquelles on peut lancer l'attaque "attackArea" et toucher "target".
     * 
     * return l'ensemble de ces Tiles.
     */
    public static List<ReachableTile> ValidCastFromTile(TileArea attackArea, List<TileData> tiles, Vector2Int startPosition, Vector2Int target)
    {
        List<ReachableTile> reachableTiles = new List<ReachableTile>();

        for (int i = 0; i < tiles.Count; i++)
        {
            reachableTiles.Add(FindShortestPath(startPosition, tiles[i].GetCoordPosition(), false, -1, true));
        }

        return ValidCastFromTile(attackArea, reachableTiles, target);
    }
    public static List<ReachableTile> ValidCastFromTile(TileArea attackArea, List<ReachableTile> reachableTiles, Vector2Int target)
    {
        List<ReachableTile> canCastAndHitTarget = new List<ReachableTile>();
        List<Vector2Int> attackRange = attackArea.RelativeArea();

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
                                        EntityBehaviour current, EntityBehaviour playerHealer, EntityBehaviour playerDPS, EntityBehaviour playerTank)
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
    public static ReachableTile PathToShortestEnemy(EntityBehaviour current, EntityBehaviour playerHealer, EntityBehaviour playerDPS, EntityBehaviour playerTank)
    {
        List<ReachableTile> listOfPathPlayers = new List<ReachableTile>();

        ReachableTile pathToHealer = null;
        if (playerHealer != null)
        {
            pathToHealer = FindShortestPath(current.GetPosition(), playerHealer.GetPosition());

            if (pathToHealer != null)
                listOfPathPlayers.Add(pathToHealer);
        }

        ReachableTile pathToDPS = null;
        if (playerDPS != null)
        {
            pathToDPS = FindShortestPath(current.GetPosition(), playerDPS.GetPosition());

            if (pathToDPS != null)
                listOfPathPlayers.Add(pathToDPS);
        }

        ReachableTile pathToTank = null;
        if (playerTank != null)
        {
            pathToTank = FindShortestPath(current.GetPosition(), playerTank.GetPosition());

            if (pathToTank != null)
                listOfPathPlayers.Add(pathToTank);
        }

        listOfPathPlayers.Sort();

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



    //#################################################################################################################################################################################################
    //######################################################################### FONCTION QUI APPELLE DES FONCTIONS / LAMBDA ###########################################################################
    //#################################################################################################################################################################################################
    public delegate void IAEntity(bool lowLife = false);
    public delegate bool SpecificConditionForMove(ReachableTile target);
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
                                                    bool condition = true, bool lowLife = false, LambdaAbilityCall functionToCallAfterTheMove = null, Ability ability = null, TileData abilityTarget = null)
    {
        if (moveTarget == null) return false;

        if (condition)
        {
            if (functionToCallAfterTheMove != null)
            {
                current.MoveTo(moveTarget).OnComplete(() => { functionToCallAfterTheMove(current, ability, abilityTarget, iaEntityFunction); });
            }

            else
            {
                current.MoveTo(moveTarget).OnComplete(() => { iaEntityFunction(lowLife); });
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
                                            IAEntity iaEntityFunction,  LambdaAbilityCall functionToCallAfterTheMove, Ability ability, TileData firstToTestTargetTile,
                                            TileData secondToTestTargetTile, TileData thirdToTestTargetTile, SpecificConditionForMove functionCondition = null)
    {
        if (!MoveAndTriggerAbilityIfNeed(current, firstToTestMovement, iaEntityFunction, functionCondition == null ? true : functionCondition(firstToTestMovement),
                                            false, functionToCallAfterTheMove, ability, firstToTestTargetTile))
        {
            if (!MoveAndTriggerAbilityIfNeed(current, secondToTestMovement, iaEntityFunction, functionCondition == null ? true : functionCondition(secondToTestMovement),
                                                false, functionToCallAfterTheMove, ability, secondToTestTargetTile))
            {
                if (!MoveAndTriggerAbilityIfNeed(current, thirdToTestMovement, iaEntityFunction, functionCondition == null ? true : functionCondition(thirdToTestMovement),
                                                    false, functionToCallAfterTheMove, ability, thirdToTestTargetTile))
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
                                            IAEntity iaEntityFunction, LambdaAbilityCall functionToCallAfterTheMove, Ability ability, SpecificConditionForMove functionCondition = null)
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
                if(MoveAndTriggerAbilityIfNeed(current, shortestExplosion[i].Item2, iaEntityFunction, functionCondition == null ? true : functionCondition(playerHealerPathToAttack),
                                                            false, functionToCallAfterTheMove, ability, shortestExplosion[i].Item1.currentTile))
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
    public static void GetAllEntity(EntityBehaviour current, ref EntityBehaviour playerHealer, ref EntityBehaviour playerDPS, ref EntityBehaviour playerTank)
    {
        List<EntityBehaviour> temp = new List<EntityBehaviour>();
        GetAllEntity(current, ref playerHealer, ref playerDPS, ref playerTank, ref temp, ref temp, ref temp, ref temp, false, false, false, false);
    }
    public static void GetAllEntity(EntityBehaviour current, ref EntityBehaviour playerHealer, ref EntityBehaviour playerDPS, ref EntityBehaviour playerTank,
                                        ref List<EntityBehaviour> enemyTank)
    {
        List<EntityBehaviour> temp = new List<EntityBehaviour>();
        GetAllEntity(current, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank, ref temp, ref temp, ref temp, true, false, false, false);
    }
    public static void GetAllEntity(EntityBehaviour current, ref EntityBehaviour playerHealer, ref EntityBehaviour playerDPS, ref EntityBehaviour playerTank,
                                        ref List<EntityBehaviour> enemyTank, ref List<EntityBehaviour> enemyDPS)
    {
        List<EntityBehaviour> temp = new List<EntityBehaviour>();
        GetAllEntity(current, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank, ref enemyDPS, ref temp, ref temp, true, true, false, false);
    }
    public static void GetAllEntity(EntityBehaviour current, ref EntityBehaviour playerHealer, ref EntityBehaviour playerDPS, ref EntityBehaviour playerTank,
                                        ref List<EntityBehaviour> enemyTank, ref List<EntityBehaviour> enemyDPS, ref List<EntityBehaviour> enemyHealer)
    {
        List<EntityBehaviour> temp = new List<EntityBehaviour>();
        GetAllEntity(current, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank, ref enemyDPS, ref enemyHealer, ref temp, true, true, true, false);
    }
    public static void GetAllEntity(EntityBehaviour current, ref EntityBehaviour playerHealer, ref EntityBehaviour playerDPS, ref EntityBehaviour playerTank,
                                        ref List<EntityBehaviour> enemyTank, ref List<EntityBehaviour> enemyDPS, ref List<EntityBehaviour> enemyHealer, ref List<EntityBehaviour> enemyMinion)
    {
        GetAllEntity(current, ref playerHealer, ref playerDPS, ref playerTank, ref enemyTank, ref enemyDPS, ref enemyHealer, ref enemyMinion, true, true, true, true);
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
    private static bool LookAround(NavigationQueryType navigationType, ref List<ReachableTile> reachableTiles, ReachableTile precedentPlace,
                            Vector2Int target, bool canWalkOnDamageTile = true, int range = -1, bool ignoreWalkable = false, bool ignoreWeightMove = false)
    {
        Vector2Int position = precedentPlace.GetCoordPosition();

        TileData lookingTileData;
        Vector2Int lookingPosition;

        if (position.x - 1 >= 0)
        {
            lookingPosition = new Vector2Int(position.x - 1, position.y);
            lookingTileData = MapManager.GetTile(lookingPosition);

            if (navigationType.Equals(NavigationQueryType.Area)) IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove);
            else if (IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target, canWalkOnDamageTile)) return true;

        }

        if (position.y + 1 < MapManager.GetSize())
        {
            lookingPosition = new Vector2Int(position.x, position.y + 1);
            lookingTileData = MapManager.GetTile(lookingPosition);

            if (navigationType.Equals(NavigationQueryType.Area)) IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove);
            else if (IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target, canWalkOnDamageTile)) return true;

        }

        if (position.x + 1 < MapManager.GetSize())
        {
            lookingPosition = new Vector2Int(position.x + 1, position.y);
            lookingTileData = MapManager.GetTile(lookingPosition);

            if (navigationType.Equals(NavigationQueryType.Area)) IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove);
            else if (IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target, canWalkOnDamageTile)) return true;

        }

        if (position.y - 1 >= 0)
        {
            lookingPosition = new Vector2Int(position.x, position.y - 1);
            lookingTileData = MapManager.GetTile(lookingPosition);

            if (navigationType.Equals(NavigationQueryType.Area)) IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove);
            else if (IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target, canWalkOnDamageTile)) return true;

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
    private static bool IsMovementPossible(NavigationQueryType navigationType, ref List<ReachableTile> reachableTiles, ReachableTile precedentPlace, TileData lookingTileData, Vector2Int lookingPosition,
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
                    AddTilesInList(navigationType, ref reachableTiles, currentPlaceAlreadyFind, currentPlace, target, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove);
                }
            }

            else
            {
                ReachableTile currentPlace = new ReachableTile(new List<TileData>(precedentPlace.path) { lookingTileData }, precedentPlace.cost + (int)lookingTileData.tileType);
                return AddTilesInList(navigationType, ref reachableTiles, currentPlaceAlreadyFind, currentPlace, target, canWalkOnDamageTile);
            }
        }

        return false;
    }

    /*
     * Ajoute la nouvelle Tiles atteignable a la List ou modifie la preexistante
     */
    private static bool AddTilesInList(NavigationQueryType navigationType, ref List<ReachableTile> reachableTiles, ReachableTile currentPlaceAlreadyFind, ReachableTile currentPlace,
                                Vector2Int target, bool canWalkOnDamageTile, int range = -1, bool ignoreWalkable = false, bool ignoreWeightMove = false)
    {
        if (currentPlaceAlreadyFind != null) // Si le TileData est deja dans la liste des points atteignable
        {
            if (currentPlace.IsBetterThat(currentPlaceAlreadyFind)) // Si le chemin actuel est meilleur que le precedent
            {
                reachableTiles.Remove(currentPlaceAlreadyFind);
                reachableTiles.Add(currentPlace);

                if (navigationType.Equals(NavigationQueryType.Area)) LookAround(navigationType, ref reachableTiles, currentPlace, target, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove);
            }
        }

        else // Si le TileData na pas encore ete atteint
        {
            reachableTiles.Add(currentPlace);
            if (navigationType.Equals(NavigationQueryType.Area)) LookAround(navigationType, ref reachableTiles, currentPlace, target, canWalkOnDamageTile, range, ignoreWalkable, ignoreWeightMove);
            else if (currentPlace.GetCoordPosition().Equals(target)) return true;
        }

        return false;
    }

    /*
     * return "null" s'il n'existe pas de path, le path complet si "fullPath" ou le path dans la limite de "range" sinon
     */
    private static ReachableTile ReturnPath(List<ReachableTile> reachableTiles, int range, bool fullPath)
    {
        if (reachableTiles == null || reachableTiles.Count <= 0)
        {
            return null;
        }

        ReachableTile shortest = reachableTiles[reachableTiles.Count - 1];
        shortest.path.RemoveAt(0);

        if (!fullPath)
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

}
