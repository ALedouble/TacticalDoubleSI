using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class CameraDepth : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log("Suce");
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }
}