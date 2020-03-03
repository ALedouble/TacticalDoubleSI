using UnityEngine;

public class Map_Manager : MonoBehaviour
{
    public Map map;

    public static Map_Manager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }
}
