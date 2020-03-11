using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    void Start()
    {
        // berk berk les nombres magiques !
        transform.position = new Vector3(MapManager.GetCenter().x, 0, MapManager.GetCenter().y);
        transform.GetChild(0).localPosition = Vector3.forward * -30;
        Camera.main.orthographicSize = MapManager.GetSize()/1.9f;
    }
}
