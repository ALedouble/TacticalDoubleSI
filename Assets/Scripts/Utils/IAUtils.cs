using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum NavigationQueryType { Area, Path };

public static class IAUtils
{
    /*
     * Trouve l'ensemble des positions atteignables depuis "startPosition"
     */
    public static List<ReachableTiles> FindAllReachablePlace(Vector2Int startPosition, List<List<TileData>> map, int range, bool ignoreWalkable = false, bool ignoreWeightMove = false)
    {
        List<ReachableTiles> reachablePlaces = new List<ReachableTiles>() { new ReachableTiles(startPosition, new List<TileData>() { map[startPosition.x][startPosition.y] }, 0) };

        LookAround(NavigationQueryType.Area, ref reachablePlaces, reachablePlaces[0], Vector2Int.zero, map, range, ignoreWalkable, ignoreWeightMove);

        reachablePlaces.RemoveAt(0);
        return reachablePlaces;
    }

    /*
     * Trouve le plus court chemin depuis "startPosition" a "objectif", calculer par la fonction de l'IA
     */
    public static List<TileData> FindShortestPath(Vector2Int startPosition, List<List<TileData>> map, Vector2Int objectif, int range, bool fullPath = false)
    {
        List<ReachableTiles> reachablePlaces = new List<ReachableTiles>() { new ReachableTiles(startPosition, new List<TileData>() { map[startPosition.x][startPosition.y] }, 0) };
        List<Vector2Int> deletedPlaces = new List<Vector2Int>();

        while (reachablePlaces.Count > 0 && !LookAround(NavigationQueryType.Path, ref reachablePlaces, reachablePlaces[0], objectif, map))
        {
            deletedPlaces.Add(reachablePlaces[0].coordPosition);
            reachablePlaces.RemoveAt(0);
            reachablePlaces.Sort();

            for (int i = 0; i < reachablePlaces.Count; i++)
            {
                if (deletedPlaces.Contains(reachablePlaces[i].coordPosition))
                {
                    reachablePlaces.RemoveAt(i--);
                }
            }
        }

        if (reachablePlaces.Count <= 0)
        {
            return null;
        }

        ReachableTiles shortest = reachablePlaces[reachablePlaces.Count - 1];
        shortest.path.RemoveAt(0);

        if (!fullPath)
        {
            return CutPathInRange(shortest, range);
        }
        else
        {
            return shortest.path;
        }
        
    }





    /*
     * Regarde si les 4 Place entourant "position" existe
     * Si oui, on regarde si la Place est atteignable
     */
    private static bool LookAround(NavigationQueryType navigationType, ref List<ReachableTiles> reachablePlaces, ReachableTiles precedentPlace,
                            Vector2Int objectif, List<List<TileData>> map, int range = -1, bool ignoreWalkable = false, bool ignoreWeightMove = false)
    {
        Vector2Int position = precedentPlace.coordPosition;

        TileData lookingTileData;
        Vector2Int lookingPosition;

        if (position.x - 1 >= 0)
        {
            lookingTileData = map[position.x - 1][position.y];
            lookingPosition = new Vector2Int(position.x - 1, position.y);

            if (navigationType.Equals(NavigationQueryType.Area)) IsMovementPossible(navigationType, ref reachablePlaces, precedentPlace, lookingTileData, lookingPosition, objectif, map, range, ignoreWalkable, ignoreWeightMove);
            else if (IsMovementPossible(navigationType, ref reachablePlaces, precedentPlace, lookingTileData, lookingPosition, objectif)) return true;

        }

        if (position.y + 1 < DebugMapManager.GetSize())
        {
            lookingTileData = map[position.x][position.y + 1];
            lookingPosition = new Vector2Int(position.x, position.y + 1);

            if (navigationType.Equals(NavigationQueryType.Area)) IsMovementPossible(navigationType, ref reachablePlaces, precedentPlace, lookingTileData, lookingPosition, objectif, map, range, ignoreWalkable, ignoreWeightMove);
            else if (IsMovementPossible(navigationType, ref reachablePlaces, precedentPlace, lookingTileData, lookingPosition, objectif)) return true;

        }

        if (position.x + 1 < DebugMapManager.GetSize())
        {
            lookingTileData = map[position.x + 1][position.y];
            lookingPosition = new Vector2Int(position.x + 1, position.y);

            if (navigationType.Equals(NavigationQueryType.Area)) IsMovementPossible(navigationType, ref reachablePlaces, precedentPlace, lookingTileData, lookingPosition, objectif, map, range, ignoreWalkable, ignoreWeightMove);
            else if (IsMovementPossible(navigationType, ref reachablePlaces, precedentPlace, lookingTileData, lookingPosition, objectif)) return true;

        }

        if (position.y - 1 >= 0)
        {
            lookingTileData = map[position.x][position.y - 1];
            lookingPosition = new Vector2Int(position.x, position.y - 1);

            if (navigationType.Equals(NavigationQueryType.Area)) IsMovementPossible(navigationType, ref reachablePlaces, precedentPlace, lookingTileData, lookingPosition, objectif, map, range, ignoreWalkable, ignoreWeightMove);
            else if (IsMovementPossible(navigationType, ref reachablePlaces, precedentPlace, lookingTileData, lookingPosition, objectif)) return true;

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
    private static bool IsMovementPossible(NavigationQueryType navigationType, ref List<ReachableTiles> reachablePlaces, ReachableTiles precedentPlace, TileData lookingTileData, Vector2Int lookingPosition,
                                     Vector2Int objectif, List<List<TileData>> map = null, int range = -1, bool ignoreWalkable = false, bool ignoreWeightMove = false)
    {
        if (ignoreWalkable || lookingTileData.IsWalkable) // Si l'on peut marcher sur la prochaine TileData OU si cela ne nous interesse pas
        {
            ReachableTiles currentPlaceAlreadyFind = reachablePlaces.Where(elem => elem.coordPosition.Equals(lookingPosition)).FirstOrDefault();

            if (navigationType.Equals(NavigationQueryType.Area))
            {
                int cout;
                if (ignoreWeightMove) cout = precedentPlace.cost + 1;
                else cout = precedentPlace.cost + lookingTileData.Cost;

                if (cout <= range) // S'il nous reste assez de points de deplacement pour aller sur lookingTileData
                {
                    ReachableTiles currentPlace = new ReachableTiles(lookingPosition, new List<TileData>(precedentPlace.path) { lookingTileData }, cout);
                    AddPlaceInList(navigationType, ref reachablePlaces, currentPlaceAlreadyFind, currentPlace, objectif, map, range, ignoreWalkable, ignoreWeightMove);
                }
            }

            else
            {
                ReachableTiles currentPlace = new ReachableTiles(lookingPosition, new List<TileData>(precedentPlace.path) { lookingTileData }, precedentPlace.cost + lookingTileData.Cost);
                return AddPlaceInList(navigationType, ref reachablePlaces, currentPlaceAlreadyFind, currentPlace, objectif);
            }
        }

        return false;
    }

    /*
     * Ajoute la nouvelle Place atteignable a la List ou modifie la preexistante
     */
    private static bool AddPlaceInList(NavigationQueryType navigationType, ref List<ReachableTiles> reachablePlaces, ReachableTiles currentPlaceAlreadyFind, ReachableTiles currentPlace,
                                Vector2Int objectif, List<List<TileData>> map = null, int range = -1, bool ignoreWalkable = false, bool ignoreWeightMove = false)
    {
        if (currentPlaceAlreadyFind != null) // Si le TileData est deja dans la liste des points atteignable
        {
            if (currentPlace.IsBetterThat(currentPlaceAlreadyFind)) // Si le chemin actuel est meilleur que le precedent
            {
                reachablePlaces.Remove(currentPlaceAlreadyFind);
                reachablePlaces.Add(currentPlace);

                if (navigationType.Equals(NavigationQueryType.Area)) LookAround(navigationType, ref reachablePlaces, currentPlace, objectif, map, range, ignoreWalkable, ignoreWeightMove);
            }
        }

        else // Si le TileData na pas encore ete atteint
        {
            reachablePlaces.Add(currentPlace);
            if (navigationType.Equals(NavigationQueryType.Area)) LookAround(navigationType, ref reachablePlaces, currentPlace, objectif, map, range, ignoreWalkable, ignoreWeightMove);
            else if (currentPlace.coordPosition.Equals(objectif)) return true;
        }

        return false;
    }

    /*
     * Cut le path de "shortest" dans la limite "range" donner
     */
    private static List<TileData> CutPathInRange(ReachableTiles shortest, int range)
    {
        List<TileData> path = new List<TileData>(shortest.path);
        int lenght = -1;
        int cost = 0;

        foreach (TileData elem in path)
        {
            lenght++;
            cost += elem.Cost;
            if (cost > range)
                break;
        }

        return path.Take(lenght).ToList();
    }





    private static Vector2Int MaFctIa()
    {
        // TO - DO

        return Vector2Int.zero;
    }
}
