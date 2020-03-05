using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum NavigationQueryType { Area, Path };

public static class IAUtils
{
    //#################################################################################################################################################################################################
    //#################################################################################################################################################################################################
    //############################################################################################ PUBLIC #############################################################################################
    //#################################################################################################################################################################################################
    //#################################################################################################################################################################################################

    /*
     * Trouve l'ensemble des positions atteignables depuis "startPosition"
     */
    public static List<ReachableTile> FindAllReachablePlace(Vector2Int startPosition, List<List<TileData>> map, int range, bool ignoreWalkable = false, bool ignoreWeightMove = false)
    {
        List<ReachableTile> reachableTiles = new List<ReachableTile>() { new ReachableTile(new List<TileData>() { map[startPosition.x][startPosition.y] }, 0) };

        LookAround(NavigationQueryType.Area, ref reachableTiles, reachableTiles[0], Vector2Int.zero, map, range, ignoreWalkable, ignoreWeightMove);

        reachableTiles.RemoveAt(0);
        return reachableTiles;
    }

    /*
     * Trouve le plus court chemin depuis "startPosition" a "target", calculer par la fonction de l'IA
     */
    public static ReachableTile FindShortestPath(Vector2Int startPosition, List<List<TileData>> map, Vector2Int target, int range, bool fullPath = false)
    {
        List<ReachableTile> reachableTiles = new List<ReachableTile>() { new ReachableTile(new List<TileData>() { map[startPosition.x][startPosition.y] }, 0) };
        List<Vector2Int> deletedPlaces = new List<Vector2Int>();

        while (reachableTiles.Count > 0 && !LookAround(NavigationQueryType.Path, ref reachableTiles, reachableTiles[0], target, map))
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

        return ReturnPath(reachableTiles, range, fullPath);
    }

    /*
     * Regarde dans "reachableTiles" quels sont les Tiles desquelles on peut lancer l'attaque "attackArea" et toucher "target".
     * 
     * return l'ensemble de ces Tiles.
     */
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

        return canCastAndHitTarget;
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
                            Vector2Int target, List<List<TileData>> map, int range = -1, bool ignoreWalkable = false, bool ignoreWeightMove = false)
    {
        Vector2Int position = precedentPlace.GetCoordPosition();

        TileData lookingTileData;
        Vector2Int lookingPosition;

        if (position.x - 1 >= 0)
        {
            lookingTileData = map[position.x - 1][position.y];
            lookingPosition = new Vector2Int(position.x - 1, position.y);

            if (navigationType.Equals(NavigationQueryType.Area)) IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target, map, range, ignoreWalkable, ignoreWeightMove);
            else if (IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target)) return true;

        }

        if (position.y + 1 < MapManager.GetSize())
        {
            lookingTileData = map[position.x][position.y + 1];
            lookingPosition = new Vector2Int(position.x, position.y + 1);

            if (navigationType.Equals(NavigationQueryType.Area)) IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target, map, range, ignoreWalkable, ignoreWeightMove);
            else if (IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target)) return true;

        }

        if (position.x + 1 < MapManager.GetSize())
        {
            lookingTileData = map[position.x + 1][position.y];
            lookingPosition = new Vector2Int(position.x + 1, position.y);

            if (navigationType.Equals(NavigationQueryType.Area)) IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target, map, range, ignoreWalkable, ignoreWeightMove);
            else if (IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target)) return true;

        }

        if (position.y - 1 >= 0)
        {
            lookingTileData = map[position.x][position.y - 1];
            lookingPosition = new Vector2Int(position.x, position.y - 1);

            if (navigationType.Equals(NavigationQueryType.Area)) IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target, map, range, ignoreWalkable, ignoreWeightMove);
            else if (IsMovementPossible(navigationType, ref reachableTiles, precedentPlace, lookingTileData, lookingPosition, target)) return true;

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
                                     Vector2Int target, List<List<TileData>> map = null, int range = -1, bool ignoreWalkable = false, bool ignoreWeightMove = false)
    {
        if (ignoreWalkable || lookingTileData.IsWalkable) // Si l'on peut marcher sur la prochaine TileData OU si cela ne nous interesse pas
        {
            ReachableTile currentPlaceAlreadyFind = reachableTiles.Where(elem => elem.GetCoordPosition().Equals(lookingPosition)).FirstOrDefault();

            if (navigationType.Equals(NavigationQueryType.Area))
            {
                int cout;
                if (ignoreWeightMove) cout = precedentPlace.cost + 1;
                else cout = precedentPlace.cost + (int)lookingTileData.tileType;

                if (cout <= range) // S'il nous reste assez de points de deplacement pour aller sur lookingTileData
                {
                    ReachableTile currentPlace = new ReachableTile(new List<TileData>(precedentPlace.path) { lookingTileData }, cout);
                    AddPlaceInList(navigationType, ref reachableTiles, currentPlaceAlreadyFind, currentPlace, target, map, range, ignoreWalkable, ignoreWeightMove);
                }
            }

            else
            {
                ReachableTile currentPlace = new ReachableTile(new List<TileData>(precedentPlace.path) { lookingTileData }, precedentPlace.cost + (int)lookingTileData.tileType);
                return AddPlaceInList(navigationType, ref reachableTiles, currentPlaceAlreadyFind, currentPlace, target);
            }
        }

        return false;
    }

    /*
     * Ajoute la nouvelle Place atteignable a la List ou modifie la preexistante
     */
    private static bool AddPlaceInList(NavigationQueryType navigationType, ref List<ReachableTile> reachableTiles, ReachableTile currentPlaceAlreadyFind, ReachableTile currentPlace,
                                Vector2Int target, List<List<TileData>> map = null, int range = -1, bool ignoreWalkable = false, bool ignoreWeightMove = false)
    {
        if (currentPlaceAlreadyFind != null) // Si le TileData est deja dans la liste des points atteignable
        {
            if (currentPlace.IsBetterThat(currentPlaceAlreadyFind)) // Si le chemin actuel est meilleur que le precedent
            {
                reachableTiles.Remove(currentPlaceAlreadyFind);
                reachableTiles.Add(currentPlace);

                if (navigationType.Equals(NavigationQueryType.Area)) LookAround(navigationType, ref reachableTiles, currentPlace, target, map, range, ignoreWalkable, ignoreWeightMove);
            }
        }

        else // Si le TileData na pas encore ete atteint
        {
            reachableTiles.Add(currentPlace);
            if (navigationType.Equals(NavigationQueryType.Area)) LookAround(navigationType, ref reachableTiles, currentPlace, target, map, range, ignoreWalkable, ignoreWeightMove);
            else if (currentPlace.GetCoordPosition().Equals(target)) return true;
        }

        return false;
    }

    /*
     * return "null" s'il n'existe pas de path, le path complet si "fullPath" ou le path dans la limite de "range" sinon
     */
    private static ReachableTile ReturnPath(List<ReachableTile> reachableTiles, int range, bool fullPath)
    {
        if (reachableTiles.Count <= 0)
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
        List<TileData> path = new List<TileData>(shortest.path);
        int lenght = 0;
        int cost = 0;

        for (int i = 0; i < path.Count; i++)
        {
            lenght++;
            cost += (int)path[i].tileType;
            if (cost > range)
                break;
        }

        shortest.path = path.Take(lenght).ToList();

        return shortest;
    }
}
