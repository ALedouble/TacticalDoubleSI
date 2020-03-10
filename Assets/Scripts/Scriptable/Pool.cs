using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Pool", menuName = "Pooling/Pool")]
public class Pool : ScriptableObject
{
    public GameObject prefab;
    public int size;

    [HideInInspector] public bool renaming;

    public List<GameObject> inactiveObjects = new List<GameObject>();
    public List<GameObject> activeObjects = new List<GameObject>();

    public GameObject GetObject()
    {
        GameObject obj = null;
        
        if (inactiveObjects.Count < 1)
        {
            obj = Instantiate(prefab);
            activeObjects.Add(obj);

            return obj;
        }
        else
        {
            obj = inactiveObjects[0];
            obj.transform.rotation = prefab.transform.rotation;

            inactiveObjects.Remove(obj);
            activeObjects.Add(obj);
        }
        
        return obj;
    }
}
