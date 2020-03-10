using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    void Start()
    {
        // berk berk les nombres magiques !
        transform.position = new Vector3(Mathf.Floor(MapManager.GetSize() * .5f)-1, 0, Mathf.Floor(MapManager.GetSize() * .5f)+1);
        transform.GetChild(0).localPosition = Vector3.forward * -30;
        Camera.main.orthographicSize = MapManager.GetSize()/2;
    }
}
