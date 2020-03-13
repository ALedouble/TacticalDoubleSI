using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    public PoolCollection poolCollection;
    public List<PoolObjectReference> poolObjectReferences = new List<PoolObjectReference>();

    void Awake()
    {
        Instance = this;

        InitPools();
    }

    void InitPools()
    {
        for (int i = 0; i < poolCollection.pools.Count; i++)
        {
            if (poolCollection.pools[i].activeObjects != null) poolCollection.pools[i].activeObjects.Clear();
            if (poolCollection.pools[i].inactiveObjects != null) poolCollection.pools[i].inactiveObjects.Clear();

            for (int j = 0; j < poolCollection.pools[i].size; j++)
            {
                GameObject newGameObject = Instantiate(poolCollection.pools[i].prefab, transform);
                newGameObject.SetActive(false);

                poolCollection.pools[i].inactiveObjects.Add(newGameObject);
            }
        }
    }

    public static GameObject InstantiatePooled(GameObject gameObject, Vector3 position, Transform parent = null)
    {
        if (gameObject == null)
        {
            return null;
        }

        GameObject obj = null;

        for (int i = 0; i < Instance.poolCollection.pools.Count; i++)
        {
            if (gameObject == Instance.poolCollection.pools[i].prefab)
            {
                obj = Instance.poolCollection.pools[i].GetObject();

                obj.transform.position = position;
                obj.SetActive(true);
                obj.transform.parent = parent;
                
                Instance.poolObjectReferences.Add(new PoolObjectReference(obj, Instance.poolCollection.pools[i]));
            }
        }

        if (obj == null) Debug.LogError("GameObject " + gameObject.name + " not found, please add a pool for this object");

        return obj;
    }

    public static void Recycle(GameObject gameObject)
    {
        bool recycled = false;

        for (int i = 0; i < Instance.poolObjectReferences.Count; i++)
        {
            if (Instance.poolObjectReferences[i].sceneObject == gameObject)
            {
                gameObject.SetActive(false);
                gameObject.transform.parent = Instance.transform;

                Instance.poolObjectReferences[i].pool.activeObjects.Remove(gameObject);
                Instance.poolObjectReferences[i].pool.inactiveObjects.Add(gameObject);

                Instance.poolObjectReferences.Remove(Instance.poolObjectReferences[i]);

                recycled = true;
            }
        }

        if (!recycled) Debug.LogError("GameObject " + gameObject.name + " cannot be recycled, as there is no pool for this object");
    }
}

[System.Serializable]
public struct PoolObjectReference
{
    public GameObject sceneObject;
    public Pool pool;

    public PoolObjectReference(GameObject _gameObject, Pool _pool)
    {
        sceneObject = _gameObject;
        pool = _pool;
    }
}