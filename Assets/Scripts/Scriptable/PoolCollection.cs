using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="new PoolCollection",menuName ="Pooling/PoolCollection")]
public class PoolCollection : ScriptableObject
{
    public List<Pool> pools = new List<Pool>();
}
